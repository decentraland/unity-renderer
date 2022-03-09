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

        private static bool VERBOSE = false;
        private static Logger logger = new Logger("SceneMetricsCounter") { verboseEnabled = VERBOSE };
        public event Action<ISceneMetricsCounter> OnMetricsUpdated;

        private SceneMetricsModel maxCountValue = new SceneMetricsModel();
        private SceneMetricsModel currentCountValue = new SceneMetricsModel();

        public SceneMetricsModel currentCount
        {
            get
            {
                UpdateMetrics();
                return currentCountValue.Clone();
            }
        }

        public SceneMetricsModel maxCount => maxCountValue.Clone();

        public bool dirty { get; private set; }

        private string sceneId;

        private Vector2Int scenePosition;

        private int sceneParcelCount;

        private DataStore_WorldObjects data;
        private bool enabled = false;

        public SceneMetricsCounter(DataStore_WorldObjects dataStore, string sceneId, Vector2Int scenePosition, int sceneParcelCount)
        {
            this.data = dataStore;
            Configure(sceneId, scenePosition, sceneParcelCount);
        }

        public SceneMetricsCounter(DataStore_WorldObjects dataStore)
        {
            this.data = dataStore;
        }

        public void Configure(string sceneId, Vector2Int scenePosition, int sceneParcelCount)
        {
            this.sceneId = sceneId;
            this.scenePosition = scenePosition;
            this.sceneParcelCount = sceneParcelCount;

            Assert.IsTrue( !string.IsNullOrEmpty(sceneId), "Scene must have an ID!" );
            maxCountValue = ComputeMaxCount();
        }

        public void Dispose()
        {
        }


        public void Enable()
        {
            if ( enabled )
                return;

            var sceneData = data.sceneData[sceneId];

            sceneData.materials.OnAdded += OnDataChanged;
            sceneData.materials.OnRemoved += OnDataChanged;

            sceneData.textures.OnAdded += OnDataChanged;
            sceneData.textures.OnRemoved += OnDataChanged;

            sceneData.meshes.OnAdded += OnDataChanged;
            sceneData.meshes.OnRemoved += OnDataChanged;

            sceneData.renderers.OnAdded += OnDataChanged;
            sceneData.renderers.OnRemoved += OnDataChanged;

            sceneData.owners.OnAdded += OnDataChanged;
            sceneData.owners.OnRemoved += OnDataChanged;

            sceneData.triangles.OnChange += OnDataChanged;

            enabled = true;
        }

        public void Disable()
        {
            if ( !enabled )
                return;

            var sceneData = data.sceneData[sceneId];

            sceneData.materials.OnAdded -= OnDataChanged;
            sceneData.materials.OnRemoved -= OnDataChanged;

            sceneData.textures.OnAdded -= OnDataChanged;
            sceneData.textures.OnRemoved -= OnDataChanged;

            sceneData.meshes.OnAdded -= OnDataChanged;
            sceneData.meshes.OnRemoved -= OnDataChanged;

            sceneData.renderers.OnAdded -= OnDataChanged;
            sceneData.renderers.OnRemoved -= OnDataChanged;

            sceneData.owners.OnAdded -= OnDataChanged;
            sceneData.owners.OnRemoved -= OnDataChanged;

            sceneData.triangles.OnChange -= OnDataChanged;

            enabled = false;
        }

        private SceneMetricsModel ComputeMaxCount()
        {
            var result = new SceneMetricsModel();

            float log = Mathf.Log(sceneParcelCount + 1, 2);
            float lineal = sceneParcelCount;

            result.triangles = (int) (lineal * LimitsConfig.triangles);
            result.bodies = (int) (lineal * LimitsConfig.bodies);
            result.entities = (int) (lineal * LimitsConfig.entities);
            result.materials = (int) (log * LimitsConfig.materials);
            result.textures = (int) (log * LimitsConfig.textures);
            result.meshes = (int) (log * LimitsConfig.meshes);
            result.sceneHeight = (int) (log * LimitsConfig.height);

            return result;
        }

        public bool IsInsideTheLimits()
        {
            UpdateMetrics();
            SceneMetricsModel limits = ComputeMaxCount();
            SceneMetricsModel usage = currentCountValue;

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

        private void MarkDirty()
        {
            dirty = true;
        }

        public void SendEvent()
        {
            if (!dirty)
                return;

            dirty = false;

            UpdateMetrics();

            Interface.WebInterface.ReportOnMetricsUpdate(sceneId, currentCountValue.ToMetricsModel(), maxCount.ToMetricsModel());
        }

        void OnDataChanged<T>(T obj)
            where T : class
        {
            MarkDirty();
        }

        void OnDataChanged(int obj1, int obj2)
        {
            MarkDirty();
        }

        private void UpdateMetrics()
        {
            if (string.IsNullOrEmpty(sceneId))
                return;
            
            var sceneData = data?.sceneData[sceneId];

            if ( sceneData != null )
            {
                currentCountValue.materials = sceneData.materials.Count();
                currentCountValue.textures = sceneData.textures.Count();
                currentCountValue.meshes = sceneData.meshes.Count();
                currentCountValue.entities = sceneData.owners.Count();
                currentCountValue.bodies = sceneData.renderers.Count();
                currentCountValue.triangles = sceneData.triangles.Get() / 3;
            }

            logger.Verbose($"Current metrics: {currentCountValue}");
            RaiseMetricsUpdate();
        }

        private void UpdateWorstMetricsOffense()
        {
            DataStore_SceneMetrics metricsData = DataStore.i.Get<DataStore_SceneMetrics>();

            if ( maxCountValue != null && metricsData.worstMetricOffenseComputeEnabled.Get() )
            {
                bool isOffending = currentCountValue > maxCountValue;

                if ( !isOffending )
                    return;

                bool firstOffense = false;

                if (!metricsData.worstMetricOffenses.ContainsKey(sceneId))
                {
                    firstOffense = true;
                    metricsData.worstMetricOffenses[sceneId] = currentCountValue.Clone();
                }

                SceneMetricsModel worstOffense = metricsData.worstMetricOffenses[sceneId];
                SceneMetricsModel currentOffense = maxCountValue - currentCountValue;

                if ( firstOffense )
                    logger.Verbose($"New offending scene {sceneId} ({scenePosition})!\n{currentCountValue}");

                if ( currentOffense < worstOffense )
                    return;

                metricsData.worstMetricOffenses[sceneId] = currentOffense;
                logger.Verbose($"New offending scene {sceneId} {scenePosition}!\nmetrics: {currentCountValue}\nlimits: {maxCountValue}\ndelta:{currentOffense}");
            }
        }

        private void RaiseMetricsUpdate()
        {
            UpdateWorstMetricsOffense();
            OnMetricsUpdated?.Invoke(this);
        }
    }
}