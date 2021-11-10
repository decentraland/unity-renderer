using System;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    [System.Serializable]
    public sealed class SceneMetricsCounter : ISceneMetricsCounter
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
            public int bodies = 0;
            public int triangles = 0;
            public int textures = 0;

            public override string ToString()
            {
                return $"materials: {materials.Count}, bodies: {bodies}, triangles: {triangles}, textures: {textures}";
            }
        }

        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public IParcelScene scene { get; private set; }

        public HashSet<string> excludedEntities = new HashSet<string>();

        SceneMetricsModel cachedModel = null;

        private SceneMetricsModel model;

        private Dictionary<Material, int> uniqueMaterialsRefCount = new Dictionary<Material, int>();
        private Dictionary<Mesh, int> uniqueMeshesRefCount = new Dictionary<Mesh, int>();
        private Dictionary<IDCLEntity, EntityMetrics> entitiesMetrics = new Dictionary<IDCLEntity, EntityMetrics>();

        private WorldSceneObjectsTrackingHelper sceneObjectsTrackingHelper;
        public event Action<ISceneMetricsCounter> OnMetricsUpdated;

        public bool isDirty { get; private set; }

        public SceneMetricsModel GetModel() { return model.Clone(); }

        public SceneMetricsCounter(IParcelScene sceneOwner)
        {
            Assert.IsTrue( !string.IsNullOrEmpty(sceneOwner.sceneData.id), "Scene must have an ID!" );
            this.scene = sceneOwner;

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

            sceneObjectsTrackingHelper.OnWillAddRendereable += OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable += OnWillRemoveRendereable;

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

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;
        }

        private void OnWillAddRendereable(Rendereable rendereable)
        {
            if (excludedEntities.Contains(rendereable.ownerId))
                return;

            int trianglesToAdd = rendereable.totalTriangleCount / 3;
            model.triangles += trianglesToAdd;

            for ( int i = 0; i < rendereable.meshes.Count; i++)
            {
                Mesh mesh = rendereable.meshes[i];

                if (uniqueMeshesRefCount.ContainsKey(mesh))
                {
                    uniqueMeshesRefCount[mesh]++;
                }
                else
                {
                    uniqueMeshesRefCount.Add(mesh, 1);
                    model.meshes++;
                }
            }

            OnMetricsUpdated?.Invoke(this);
        }

        private void OnWillRemoveRendereable(Rendereable rendereable)
        {
            if (excludedEntities.Contains(rendereable.ownerId))
                return;

            int trianglesToRemove = rendereable.totalTriangleCount / 3;
            model.triangles -= trianglesToRemove;

            for ( int i = 0; i < rendereable.meshes.Count; i++)
            {
                Mesh mesh = rendereable.meshes[i];

                if (uniqueMeshesRefCount.ContainsKey(mesh))
                {
                    uniqueMeshesRefCount[mesh]--;

                    if (uniqueMeshesRefCount[mesh] == 0)
                    {
                        model.meshes--;
                        uniqueMeshesRefCount.Remove(mesh);
                    }
                }
                else
                {
                    logger.LogWarning("OnWillRemoveRendereable", "Trying to remove mesh that never was added");
                }
            }

            OnMetricsUpdated?.Invoke(this);
        }

        private void OnEntityAdded(IDCLEntity e)
        {
            e.OnMeshesInfoUpdated += OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned += OnEntityMeshInfoCleaned;
            model.entities++;
            isDirty = true;
            OnMetricsUpdated?.Invoke(this);
        }

        private void OnEntityRemoved(IDCLEntity e)
        {
            // TODO(Brian): When all the code is migrated to the Rendereable counting, remove this call
            SubstractMetrics(e);

            e.OnMeshesInfoUpdated -= OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned -= OnEntityMeshInfoCleaned;
            model.entities--;
            isDirty = true;
            OnMetricsUpdated?.Invoke(this);
        }

        // TODO(Brian): When all the code is migrated to the Rendereable counting, remove this method
        private void OnEntityMeshInfoUpdated(IDCLEntity entity)
        {
            AddOrReplaceMetrics(entity);
            OnMetricsUpdated?.Invoke(this);
        }

        // TODO(Brian): When all the code is migrated to the Rendereable counting, remove this method
        private void OnEntityMeshInfoCleaned(IDCLEntity entity)
        {
            SubstractMetrics(entity);
            OnMetricsUpdated?.Invoke(this);
        }

        // TODO(Brian): When all the code is migrated to the Rendereable counting, remove this method
        private void AddOrReplaceMetrics(IDCLEntity entity)
        {
            if (entitiesMetrics.ContainsKey(entity))
            {
                SubstractMetrics(entity);
            }

            AddMetrics(entity);
            OnMetricsUpdated?.Invoke(this);
        }

        // TODO(Brian): Move all this counting on OnWillRemoveRendereable instead
        [System.Obsolete]
        protected void SubstractMetrics(IDCLEntity entity)
        {
            if (excludedEntities.Contains(entity.entityId))
                return;

            if (!entitiesMetrics.ContainsKey(entity))
                return;

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

        // TODO(Brian): Move all this counting on OnWillAddRendereable instead
        [System.Obsolete]
        protected void AddMetrics(IDCLEntity entity)
        {
            if (excludedEntities.Contains(entity.entityId))
                return;

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

        // TODO(Brian): Move all this counting on OnWillAddRendereable instead
        [System.Obsolete]
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

        // TODO(Brian): Put this into OnWillAddRendereable instead
        [System.Obsolete]
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

        // TODO(Brian): Put this into OnWillRemoveRendereable instead
        [System.Obsolete]
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

        public void Dispose()
        {
            if (scene == null)
                return;

            sceneObjectsTrackingHelper.Dispose();

            scene.OnEntityAdded -= OnEntityAdded;
            scene.OnEntityRemoved -= OnEntityRemoved;

            logger.Log("Disposing...");
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

        public void AddExcludedEntity(string entityId)
        {
            if ( !excludedEntities.Contains(entityId))
                excludedEntities.Add(entityId);
        }

        public void RemoveExcludedEntity(string entityId)
        {
            if ( excludedEntities.Contains(entityId))
                excludedEntities.Remove(entityId);
        }

        public void SendEvent()
        {
            if (!isDirty)
                return;

            isDirty = false;

            Interface.WebInterface.ReportOnMetricsUpdate(scene.sceneData.id,
                model.ToMetricsModel(), GetLimits().ToMetricsModel());
        }
    }
}