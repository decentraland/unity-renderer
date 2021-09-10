using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    [System.Serializable]
    public class SceneMetricsController : ISceneMetricsController
    {
        private static bool VERBOSE = false;
        public IParcelScene scene;

        public static class LimitsConfig
        {
            // number of entities
            public const int entities = 200;

            // Number of faces (per parcel)
            public const int triangles = 10000;
            public const int bodies = 300;
            public const int textures = 10;
            public const int materials = 20;
            public const int meshes = 200;

            public const float height = 20;
            public const float visibleRadius = 10;
        }

        protected class EntityMetrics
        {
            public Dictionary<Material, int> materials = new Dictionary<Material, int>();
            public Dictionary<Mesh, int> meshes = new Dictionary<Mesh, int>();
            public int bodies = 0;
            public int triangles = 0;
            public int textures = 0;

            override public string ToString()
            {
                return string.Format("materials: {0}, meshes: {1}, bodies: {2}, triangles: {3}, textures: {4}",
                    materials.Count, meshes.Count, bodies, triangles, textures);
            }
        }

        [SerializeField]
        protected SceneMetricsModel model;

        protected Dictionary<IDCLEntity, EntityMetrics> entitiesMetrics;
        private Dictionary<Mesh, int> uniqueMeshesRefCount;
        private Dictionary<Material, int> uniqueMaterialsRefCount;
        private Dictionary<Mesh, int> meshToTriangleCount;

        public bool isDirty { get; protected set; }

        public SceneMetricsModel GetModel() { return model.Clone(); }

        public SceneMetricsController(IParcelScene sceneOwner)
        {
            this.scene = sceneOwner;

            uniqueMeshesRefCount = new Dictionary<Mesh, int>();
            uniqueMaterialsRefCount = new Dictionary<Material, int>();
            entitiesMetrics = new Dictionary<IDCLEntity, EntityMetrics>();
            meshToTriangleCount = new Dictionary<Mesh, int>();
            model = new SceneMetricsModel();

            RenderingGlobalEvents.OnWillUploadMeshToGPU += OnWillUploadMeshToGPU;

            if (VERBOSE)
            {
                Debug.Log("Start ScenePerformanceLimitsController...");
            }
        }

        public void Enable()
        {
            RenderingGlobalEvents.OnWillUploadMeshToGPU -= OnWillUploadMeshToGPU;
            RenderingGlobalEvents.OnWillUploadMeshToGPU += OnWillUploadMeshToGPU;

            if (scene == null)
                return;

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
            scene.OnEntityAdded += OnEntityAdded;
            scene.OnEntityRemoved += OnEntityRemoved;
        }

        public void Disable()
        {
            RenderingGlobalEvents.OnWillUploadMeshToGPU -= OnWillUploadMeshToGPU;

            if (scene == null)
                return;

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
        }

        SceneMetricsModel cachedModel = null;

        public SceneMetricsModel GetLimits()
        {
            if (cachedModel == null)
            {
                cachedModel = new SceneMetricsModel();

                int parcelCount = scene.sceneData.parcels.Length;
                float log = Mathf.Log(parcelCount + 1, 2);
                float lineal = parcelCount;

                cachedModel.triangles = (int) (lineal * LimitsConfig.triangles);
                cachedModel.bodies = (int) (lineal * LimitsConfig.bodies);
                cachedModel.entities = (int) (lineal * LimitsConfig.entities);
                cachedModel.materials = (int) (log * LimitsConfig.materials);
                cachedModel.textures = (int) (log * LimitsConfig.textures);
                cachedModel.meshes = (int) (log * LimitsConfig.meshes);
                cachedModel.sceneHeight = (int) (log * LimitsConfig.height);
            }

            return cachedModel;
        }

        public bool IsInsideTheLimits()
        {
            SceneMetricsModel limits = GetLimits();
            SceneMetricsModel usage = GetModel();

            if (usage.triangles > limits.triangles)
                return false;

            if (usage.bodies > limits.bodies)
                return false;

            if (usage.entities > limits.entities)
                return false;

            if (usage.materials > limits.materials)
                return false;

            if (usage.textures > limits.textures)
                return false;

            if (usage.meshes > limits.meshes)
                return false;

            return true;
        }

        public void SendEvent()
        {
            if (isDirty)
            {
                isDirty = false;

                Interface.WebInterface.ReportOnMetricsUpdate(scene.sceneData.id,
                    model.ToMetricsModel(), GetLimits().ToMetricsModel());
            }
        }

        protected virtual void OnEntityAdded(IDCLEntity e)
        {
            e.OnMeshesInfoUpdated += OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned += OnEntityMeshInfoCleaned;
            model.entities++;
            isDirty = true;
        }

        protected virtual void OnEntityRemoved(IDCLEntity e)
        {
            SubstractMetrics(e);
            e.OnMeshesInfoUpdated -= OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned -= OnEntityMeshInfoCleaned;
            model.entities--;
            isDirty = true;
        }

        protected virtual void OnEntityMeshInfoUpdated(IDCLEntity entity) { AddOrReplaceMetrics(entity); }

        protected virtual void OnEntityMeshInfoCleaned(IDCLEntity entity) { SubstractMetrics(entity); }

        protected void AddOrReplaceMetrics(IDCLEntity entity)
        {
            if (entitiesMetrics.ContainsKey(entity))
            {
                SubstractMetrics(entity);
            }

            AddMetrics(entity);
        }

        protected void SubstractMetrics(IDCLEntity entity)
        {
            if (!entitiesMetrics.ContainsKey(entity))
            {
                return;
            }

            EntityMetrics entityMetrics = entitiesMetrics[entity];

            RemoveEntitiesMaterial(entityMetrics);
            RemoveEntityMeshes(entityMetrics);

            model.materials = uniqueMaterialsRefCount.Count;
            model.meshes = uniqueMeshesRefCount.Count;
            model.triangles -= entityMetrics.triangles;
            model.bodies -= entityMetrics.bodies;

            if (entitiesMetrics.ContainsKey(entity))
            {
                entitiesMetrics.Remove(entity);
            }

            isDirty = true;
        }

        protected void AddMetrics(IDCLEntity entity)
        {
            if (entity.meshRootGameObject == null)
            {
                return;
            }

            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>()) // the object's MaterialTransitionController is destroyed when it finishes loading
            {
                return;
            }

            EntityMetrics entityMetrics = new EntityMetrics();

            int visualMeshRawTriangles = 0;

            // If this proves to be too slow we can spread it with a Coroutine spooler.
            MeshFilter[] meshFilters = entity.meshesInfo.meshFilters;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter mf = meshFilters[i];
                Mesh sharedMesh = mf.sharedMesh;

                if (mf == null || sharedMesh == null)
                    continue;

                if ( !meshToTriangleCount.ContainsKey(sharedMesh) )
                    continue;

                // We have to use meshToTriangleCount because if meshes are uploaded to GPU at
                // this stage, sharedMesh.triangles doesn't work.
                int triangleCount = 0;

                if ( sharedMesh.isReadable )
                    triangleCount = sharedMesh.triangles.Length;
                else
                    triangleCount = meshToTriangleCount[sharedMesh];

                visualMeshRawTriangles += triangleCount;
                AddMesh(entityMetrics, sharedMesh);

                if (VERBOSE)
                {
                    Debug.Log("SceneMetrics: tris count " + triangleCount + " from mesh " + sharedMesh.name + " of entity " + entity.entityId);
                    Debug.Log("SceneMetrics: mesh " + mf.sharedMesh.name + " of entity " + entity.entityId);
                }
            }

            CalculateMaterials(entity, entityMetrics);

            // The array is a list of triangles that contains indices into the vertex array.
            // The size of the triangle array must always be a multiple of 3.
            // Vertices can be shared by simply indexing into the same vertex.
            entityMetrics.triangles = visualMeshRawTriangles / 3;
            entityMetrics.bodies = entity.meshesInfo.meshFilters.Length;

            model.materials = uniqueMaterialsRefCount.Count;
            model.meshes = uniqueMeshesRefCount.Count;
            model.triangles += entityMetrics.triangles;
            model.bodies += entityMetrics.bodies;

            if (!entitiesMetrics.ContainsKey(entity))
            {
                entitiesMetrics.Add(entity, entityMetrics);
            }
            else
            {
                entitiesMetrics[entity] = entityMetrics;
            }

            if (VERBOSE)
            {
                Debug.Log("SceneMetrics: entity " + entity.entityId + " metrics " + entityMetrics.ToString());
            }

            isDirty = true;
        }

        void CalculateMaterials(IDCLEntity entity, EntityMetrics entityMetrics)
        {
            var originalMaterials = Environment.i.world.sceneBoundsChecker.GetOriginalMaterials(entity.meshesInfo);
            if (originalMaterials == null)
                return;

            int originalMaterialsCount = originalMaterials.Count;

            for (int i = 0; i < originalMaterialsCount; i++)
            {
                AddMaterial(entityMetrics, originalMaterials[i]);

                if (VERBOSE)
                    Debug.Log($"SceneMetrics: material (style: {Environment.i.world.sceneBoundsChecker.GetFeedbackStyle().GetType().FullName}) {originalMaterials[i].name} of entity {entity.entityId}");
            }
        }

        void AddMesh(EntityMetrics entityMetrics, Mesh mesh)
        {
            if (!uniqueMeshesRefCount.ContainsKey(mesh))
            {
                uniqueMeshesRefCount.Add(mesh, 1);
            }
            else
            {
                uniqueMeshesRefCount[mesh] += 1;
            }

            if (!entityMetrics.meshes.ContainsKey(mesh))
            {
                entityMetrics.meshes.Add(mesh, 1);
            }
            else
            {
                entityMetrics.meshes[mesh] += 1;
            }
        }

        void RemoveEntityMeshes(EntityMetrics entityMetrics)
        {
            var entityMeshes = entityMetrics.meshes;
            using (var iterator = entityMeshes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    Mesh key = iterator.Current.Key;

                    if (uniqueMeshesRefCount.ContainsKey(key))
                    {
                        uniqueMeshesRefCount[key] -= iterator.Current.Value;

                        if (uniqueMeshesRefCount[key] <= 0)
                        {
                            uniqueMeshesRefCount.Remove(key);

                            if ( meshToTriangleCount.ContainsKey(key) )
                                meshToTriangleCount.Remove(key);
                        }
                    }
                }
            }
        }

        void AddMaterial(EntityMetrics entityMetrics, Material material)
        {
            if (material == null)
                return;

            if (!uniqueMaterialsRefCount.ContainsKey(material))
            {
                uniqueMaterialsRefCount.Add(material, 1);
            }
            else
            {
                uniqueMaterialsRefCount[material] += 1;
            }

            if (!entityMetrics.materials.ContainsKey(material))
            {
                entityMetrics.materials.Add(material, 1);
            }
            else
            {
                entityMetrics.materials[material] += 1;
            }
        }

        void RemoveEntitiesMaterial(EntityMetrics entityMetrics)
        {
            var entityMaterials = entityMetrics.materials;
            using (var iterator = entityMaterials.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (uniqueMaterialsRefCount.ContainsKey(iterator.Current.Key))
                    {
                        uniqueMaterialsRefCount[iterator.Current.Key] -= iterator.Current.Value;
                        if (uniqueMaterialsRefCount[iterator.Current.Key] <= 0)
                        {
                            uniqueMaterialsRefCount.Remove(iterator.Current.Key);
                        }
                    }
                }
            }
        }

        public void OnWillUploadMeshToGPU(Mesh mesh)
        {
            if ( !meshToTriangleCount.ContainsKey(mesh) )
                meshToTriangleCount.Add(mesh, mesh.triangles.Length);
        }

        public void Dispose()
        {
            RenderingGlobalEvents.OnWillUploadMeshToGPU -= OnWillUploadMeshToGPU;

            if (scene == null)
                return;

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
        }
    }
}