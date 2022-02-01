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
    // - Redefine how metrics should be calculated
    //      - Unique loaded textures size
    //      - Vertex count
    //      - Skinned mesh renderers
    //      - Scene messages throughput (tbd criteria)
    //      - Do not take into account invisible shapes
    //      - Transparent vs opaque materials

    // - Send events with the metrics to measure world status
    // - Refactor SceneMetricsCounter to support textures + handle excluded entities correctly

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

        private class EntityMetrics
        {
            public HashSet<Rendereable> rendereables = new HashSet<Rendereable>();
        }

        public event Action<ISceneMetricsCounter> OnMetricsUpdated;
        private static bool VERBOSE = true;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        private WorldSceneObjectsTrackingHelper sceneObjectsTrackingHelper;
        public DataStore_SceneMetrics data => DataStore.i.Get<DataStore_SceneMetrics>();

        private Dictionary<string, EntityMetrics> entityMetrics = new Dictionary<string, EntityMetrics>();
        private HashSet<string> excludedEntities = new HashSet<string>();

        SceneMetricsModel sceneLimits = null;

        private SceneMetricsModel modelValue;

        public ref readonly SceneMetricsModel model
        {
            get
            {
                modelValue.entities = ComputeEntityCount();
                return ref modelValue;
            }
        }

        public bool isDirty { get; private set; }

        public HashSet<Rendereable> trackedRendereables = new HashSet<Rendereable>();

        private RefCountedMetric uniqueTextures = new RefCountedMetric();
        private RefCountedMetric uniqueMaterials = new RefCountedMetric();
        private RefCountedMetric uniqueMeshes = new RefCountedMetric();
        private RefCountedMetric uniqueEntities = new RefCountedMetric();

        private string sceneId;

        private Vector2Int scenePosition;

        private int sceneParcelCount;

        private Func<HashSet<string>> entitiesGetter;

        public SceneMetricsCounter(DataStore_WorldObjects dataStore, string sceneId, Vector2Int scenePosition, int sceneParcelCount, Func<HashSet<string>> entitiesGetter)
        {
            this.sceneId = sceneId;
            this.scenePosition = scenePosition;
            this.sceneParcelCount = sceneParcelCount;
            this.entitiesGetter = entitiesGetter;

            Assert.IsTrue( !string.IsNullOrEmpty(sceneId), "Scene must have an ID!" );

            modelValue = new SceneMetricsModel();
            sceneObjectsTrackingHelper = new WorldSceneObjectsTrackingHelper(dataStore, sceneId);
        }

        public void Dispose()
        {
            sceneObjectsTrackingHelper.Dispose();
        }

        public void Enable()
        {
            sceneObjectsTrackingHelper.OnWillAddRendereable -= OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable -= OnWillRemoveRendereable;

            sceneObjectsTrackingHelper.OnWillAddRendereable += OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable += OnWillRemoveRendereable;
        }

        public void Disable()
        {
            sceneObjectsTrackingHelper.OnWillAddRendereable -= OnWillAddRendereable;
            sceneObjectsTrackingHelper.OnWillRemoveRendereable -= OnWillRemoveRendereable;
        }

        private void OnWillAddRendereable(Rendereable rendereable)
        {
            string entityId = rendereable.ownerId;

            Assert.IsTrue(entityId != null, "rendereable.ownerId cannot be null!");

            if (excludedEntities.Contains(entityId))
                return;

            if (uniqueEntities.Add(entityId))
            {
                if (!entityMetrics.ContainsKey(entityId))
                    entityMetrics.Add(entityId, new EntityMetrics());
            }

            AddTrackedRendereable(rendereable);
            UpdateUniqueMetrics();

            isDirty = true;

            RaiseMetricsUpdate();
        }

        private void OnWillRemoveRendereable(Rendereable rendereable)
        {
            string entityId = rendereable.ownerId;

            Assert.IsTrue(entityId != null, "rendereable.ownerId cannot be null!");

            if (excludedEntities.Contains(rendereable.ownerId))
                return;

            if (uniqueEntities.Remove(entityId))
            {
                if (entityMetrics.ContainsKey(entityId))
                    entityMetrics.Remove(entityId);
            }

            RemoveTrackedRendereable(rendereable);
            UpdateUniqueMetrics();

            isDirty = true;

            RaiseMetricsUpdate();
        }

        public SceneMetricsModel ComputeSceneLimits()
        {
            if (sceneLimits == null)
            {
                sceneLimits = new SceneMetricsModel();

                float log = Mathf.Log(sceneParcelCount + 1, 2);
                float lineal = sceneParcelCount;

                sceneLimits.triangles = (int) (lineal * LimitsConfig.triangles);
                sceneLimits.bodies = (int) (lineal * LimitsConfig.bodies);
                sceneLimits.entities = (int) (lineal * LimitsConfig.entities);
                sceneLimits.materials = (int) (log * LimitsConfig.materials);
                sceneLimits.textures = (int) (log * LimitsConfig.textures);
                sceneLimits.meshes = (int) (log * LimitsConfig.meshes);
                sceneLimits.sceneHeight = (int) (log * LimitsConfig.height);
            }

            return sceneLimits;
        }

        public bool IsInsideTheLimits()
        {
            SceneMetricsModel limits = ComputeSceneLimits();
            SceneMetricsModel usage = modelValue;

            // Workaround for the issue of adding entities without rendereables.
            usage.entities = ComputeEntityCount();

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

        public void RemoveExcludedEntity(string entityId)
        {
            if (excludedEntities.Contains(entityId))
                excludedEntities.Remove(entityId);

            if (entityMetrics.ContainsKey(entityId))
            {
                foreach ( var rend in entityMetrics[entityId].rendereables )
                {
                    AddTrackedRendereable( rend );
                }
            }

            UpdateUniqueMetrics();
        }

        public void AddExcludedEntity(string entityId)
        {
            if (!excludedEntities.Contains(entityId))
                excludedEntities.Add(entityId);

            if (entityMetrics.ContainsKey(entityId))
            {
                foreach ( var rend in entityMetrics[entityId].rendereables )
                {
                    RemoveTrackedRendereable( rend );
                }
            }

            UpdateUniqueMetrics();
        }

        public void AddTrackedRendereable(Rendereable rend)
        {
            if (trackedRendereables.Contains(rend))
                RemoveTrackedRendereable(rend);

            logger.Log($"Adding rendereable {rend.ownerId} -- {rend}");
            trackedRendereables.Add(rend);

            int trianglesToAdd = rend.totalTriangleCount / 3;
            modelValue.triangles += trianglesToAdd;
            modelValue.bodies += rend.renderers.Count;

            foreach ( var mat in rend.materials )
            {
                uniqueMaterials.Add(mat);
            }

            foreach ( var mesh in rend.meshes )
            {
                uniqueMeshes.Add(mesh);
            }

            foreach ( var tex in rend.textures )
            {
                uniqueTextures.Add(tex);
            }
        }

        public void RemoveTrackedRendereable(Rendereable rend)
        {
            if ( !trackedRendereables.Contains(rend) )
                return;

            logger.Log($"Removing rendereable {rend.ownerId} -- {rend}");
            trackedRendereables.Remove(rend);

            int trianglesToAdd = rend.totalTriangleCount / 3;
            modelValue.triangles -= trianglesToAdd;
            modelValue.bodies -= rend.renderers.Count;

            foreach ( var mat in rend.materials )
            {
                uniqueMaterials.Remove(mat);
            }

            foreach ( var mesh in rend.meshes )
            {
                uniqueMeshes.Remove(mesh);
            }

            foreach ( var tex in rend.textures )
            {
                uniqueTextures.Remove(tex);
            }
        }

        void UpdateUniqueMetrics()
        {
            modelValue.materials = uniqueMaterials.GetObjectsCount();
            modelValue.textures = uniqueTextures.GetObjectsCount();
            modelValue.meshes = uniqueMeshes.GetObjectsCount();
            modelValue.entities = uniqueEntities.GetObjectsCount();
        }

        private void UpdateWorstMetricsOffense()
        {
            if ( sceneLimits != null && data.worstMetricOffenseComputeEnabled.Get() )
            {
                bool isOffending = model > sceneLimits;

                if ( !isOffending )
                    return;

                bool firstOffense = false;

                if (!data.worstMetricOffenses.ContainsKey(sceneId))
                {
                    firstOffense = true;
                    data.worstMetricOffenses[sceneId] = model.Clone();
                }

                SceneMetricsModel worstOffense = data.worstMetricOffenses[sceneId];
                SceneMetricsModel currentOffense = sceneLimits - model;

                if ( firstOffense )
                    logger.Log($"New offending scene {sceneId} ({scenePosition})!\n{model}");

                if ( currentOffense < worstOffense )
                    return;

                data.worstMetricOffenses[sceneId] = currentOffense;
                logger.Log($"New offending scene {sceneId} {scenePosition}!\nmetrics: {model}\nlimits: {sceneLimits}\ndelta:{currentOffense}");
            }
        }

        public SceneMetricsModel GetModel()
        {
            return modelValue;
        }

        private int ComputeEntityCount()
        {
            return entitiesGetter().Count(x => !excludedEntities.Contains(x));
        }

        private void RaiseMetricsUpdate()
        {
            UpdateWorstMetricsOffense();
            OnMetricsUpdated?.Invoke(this);
        }

        public void SendEvent()
        {
            if (!isDirty)
                return;

            isDirty = false;

            Interface.WebInterface.ReportOnMetricsUpdate(sceneId, modelValue.ToMetricsModel(), ComputeSceneLimits().ToMetricsModel());
        }
    }
}