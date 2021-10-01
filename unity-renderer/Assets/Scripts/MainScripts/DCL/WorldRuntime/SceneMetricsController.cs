using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    [System.Serializable]
    public class SceneMetricsController : ISceneMetricsController
    {
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

            public override string ToString()
            {
                return $"materials: {materials.Count}, meshes: {meshes.Count}, bodies: {bodies}, triangles: {triangles}, textures: {textures}";
            }
        }

        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public IParcelScene scene;
        SceneMetricsModel cachedModel = null;

        [SerializeField]
        protected SceneMetricsModel model;

        protected Dictionary<IDCLEntity, EntityMetrics> entitiesMetrics;
        private Dictionary<Material, int> uniqueMaterialsRefCount;

        private WorldSceneObjectsTrackingHelper sceneObjectsTrackingHelper;

        public bool isDirty { get; protected set; }

        public SceneMetricsModel GetModel() { return model.Clone(); }

        public SceneMetricsController(IParcelScene sceneOwner)
        {
            Assert.IsTrue( !string.IsNullOrEmpty(sceneOwner.sceneData.id), "Scene must have an ID!" );
            this.scene = sceneOwner;

            uniqueMaterialsRefCount = new Dictionary<Material, int>();
            entitiesMetrics = new Dictionary<IDCLEntity, EntityMetrics>();
            model = new SceneMetricsModel();

            sceneObjectsTrackingHelper = new WorldSceneObjectsTrackingHelper(DataStore.i, scene.sceneData.id);

            logger.Log("Start ScenePerformanceLimitsController...");
        }

        public void Enable()
        {
            if (scene == null)
                return;

            sceneObjectsTrackingHelper.OnWillAddRendereable -= OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable -= OnWillRemoveRendereable;
            sceneObjectsTrackingHelper.OnWillAddMesh -= OnWillAddUniqueMesh;
            sceneObjectsTrackingHelper.OnWillRemoveMesh -= OnWillRemoveUniqueMesh;

            sceneObjectsTrackingHelper.OnWillAddRendereable += OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable += OnWillRemoveRendereable;
            sceneObjectsTrackingHelper.OnWillAddMesh += OnWillAddUniqueMesh;
            sceneObjectsTrackingHelper.OnWillRemoveMesh += OnWillRemoveUniqueMesh;

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
            scene.OnEntityAdded += OnEntityAdded;
            scene.OnEntityRemoved += OnEntityRemoved;
        }

        public void Disable()
        {
            if (scene == null)
                return;

            sceneObjectsTrackingHelper.OnWillAddRendereable -= OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable -= OnWillRemoveRendereable;
            sceneObjectsTrackingHelper.OnWillAddMesh -= OnWillAddUniqueMesh;
            sceneObjectsTrackingHelper.OnWillRemoveMesh -= OnWillRemoveUniqueMesh;

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
        }

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

            model.materials = uniqueMaterialsRefCount.Count;
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
                return;

            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>())
                return;

            EntityMetrics entityMetrics = new EntityMetrics();

            CalculateMaterials(entity, entityMetrics);

            // TODO(Brian): We should move bodies and materials to DataStore_WorldObjects later
            entityMetrics.bodies = entity.meshesInfo.meshFilters.Length;

            model.materials = uniqueMaterialsRefCount.Count;
            model.bodies += entityMetrics.bodies;

            if (!entitiesMetrics.ContainsKey(entity))
                entitiesMetrics.Add(entity, entityMetrics);
            else
                entitiesMetrics[entity] = entityMetrics;

            logger.Log("SceneMetrics: entity " + entity.entityId + " metrics " + entityMetrics.ToString());
            isDirty = true;
        }

        void CalculateMaterials(IDCLEntity entity, EntityMetrics entityMetrics)
        {
            // TODO(Brian): Find a way to remove this dependency
            var originalMaterials = Environment.i.world.sceneBoundsChecker.GetOriginalMaterials(entity.meshesInfo);

            if (originalMaterials == null)
            {
                logger.Log($"SceneMetrics: material null of entity {entity.entityId} -- (style: {Environment.i.world.sceneBoundsChecker.GetFeedbackStyle().GetType().FullName})");
                return;
            }

            int originalMaterialsCount = originalMaterials.Count;

            for (int i = 0; i < originalMaterialsCount; i++)
            {
                AddMaterial(entityMetrics, originalMaterials[i]);
                logger.Log($"SceneMetrics: material {originalMaterials[i].name} of entity {entity.entityId} -- (style: {Environment.i.world.sceneBoundsChecker.GetFeedbackStyle().GetType().FullName})");
            }
        }

        void AddMaterial(EntityMetrics entityMetrics, Material material)
        {
            if (material == null)
                return;

            if (!uniqueMaterialsRefCount.ContainsKey(material))
                uniqueMaterialsRefCount.Add(material, 1);
            else
                uniqueMaterialsRefCount[material] += 1;

            if (!entityMetrics.materials.ContainsKey(material))
                entityMetrics.materials.Add(material, 1);
            else
                entityMetrics.materials[material] += 1;
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
                            uniqueMaterialsRefCount.Remove(iterator.Current.Key);
                    }
                }
            }
        }

        public void OnWillAddRendereable(Rendereable rendereable)
        {
            int trianglesToAdd = rendereable.totalTriangleCount / 3;
            model.triangles += trianglesToAdd;
        }

        private void OnWillRemoveRendereable(Rendereable rendereable)
        {
            int trianglesToRemove = rendereable.totalTriangleCount / 3;
            model.triangles -= trianglesToRemove;
        }

        private void OnWillAddUniqueMesh(Mesh mesh, int refCount)
        {
            Assert.IsTrue(refCount == 1, "refCount should be one");
            model.meshes++;
        }

        private void OnWillRemoveUniqueMesh(Mesh mesh, int refCount)
        {
            Assert.IsTrue(refCount == 0, "refCount should be zero");
            model.meshes--;
        }

        public void Dispose()
        {
            if (scene == null)
                return;

            sceneObjectsTrackingHelper.Dispose();

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;

            logger.Log("Disposing...");
        }
    }
}