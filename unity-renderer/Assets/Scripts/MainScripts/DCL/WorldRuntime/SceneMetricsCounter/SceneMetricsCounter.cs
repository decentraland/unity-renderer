using System;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

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
        private SceneMetricsModel lastCountValue = new SceneMetricsModel();

        // TODO: We should handle this better, right now if we get the current amount of limits, we update the metrics
        // So if someone check the current amount when subscribed to the OnMetricsUpdated, we will try to Update the metrics twice
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

        private int sceneNumber;

        private Vector2Int scenePosition;

        private int sceneParcelCount;

        private DataStore_WorldObjects data;
        private bool enabled = false;

        public SceneMetricsCounter(DataStore_WorldObjects dataStore, int sceneNumber, Vector2Int scenePosition, int sceneParcelCount)
        {
            this.data = dataStore;
            Configure(sceneNumber, scenePosition, sceneParcelCount);
        }

        public SceneMetricsCounter(DataStore_WorldObjects dataStore)
        {
            this.data = dataStore;
        }

        public void Configure(int sceneNumber, Vector2Int scenePosition, int sceneParcelCount)
        {
            this.sceneNumber = sceneNumber;
            this.scenePosition = scenePosition;
            this.sceneParcelCount = sceneParcelCount;

            Assert.IsTrue( sceneNumber > 0, "Scene must have a scene number!" );
            maxCountValue = ComputeMaxCount();
        }

        public void Dispose()
        {
        }

        public void Enable()
        {
            if ( enabled )
                return;

            var sceneData = data.sceneData[sceneNumber];

            sceneData.materials.OnAdded += OnDataChanged;
            sceneData.materials.OnRemoved += OnDataChanged;

            sceneData.textures.OnAdded += OnTextureAdded;
            sceneData.textures.OnRemoved += OnTextureRemoved;

            sceneData.meshes.OnAdded += OnMeshAdded;
            sceneData.meshes.OnRemoved += OnMeshRemoved;

            sceneData.animationClips.OnAdded += OnAnimationClipAdded;
            sceneData.animationClips.OnRemoved += OnAnimationClipRemoved;

            sceneData.animationClipSize.OnChange += OnAnimationClipSizeChange;
            sceneData.meshDataSize.OnChange += OnMeshDataSizeChange;

            sceneData.audioClips.OnAdded += OnAudioClipAdded;
            sceneData.audioClips.OnRemoved += OnAudioClipRemoved;

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

            var sceneData = data.sceneData[sceneNumber];

            sceneData.materials.OnAdded -= OnDataChanged;
            sceneData.materials.OnRemoved -= OnDataChanged;

            sceneData.textures.OnAdded -= OnTextureAdded;
            sceneData.textures.OnRemoved -= OnTextureRemoved;

            sceneData.meshes.OnAdded -= OnMeshAdded;
            sceneData.meshes.OnRemoved -= OnMeshRemoved;

            sceneData.animationClipSize.OnChange -= OnAnimationClipSizeChange;
            sceneData.meshDataSize.OnChange -= OnMeshDataSizeChange;

            sceneData.audioClips.OnAdded -= OnAudioClipAdded;
            sceneData.audioClips.OnRemoved -= OnAudioClipRemoved;

            sceneData.animationClips.OnAdded -= OnDataChanged;
            sceneData.animationClips.OnRemoved -= OnDataChanged;

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

            Interface.WebInterface.ReportOnMetricsUpdate(sceneNumber, currentCountValue.ToMetricsModel(), maxCount.ToMetricsModel());
        }

        void OnMeshAdded(Mesh mesh)
        {
            if (mesh == null)
                return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.meshMemoryProfiler += Profiler.GetRuntimeMemorySizeLong(mesh);
#endif
            MarkDirty();
        }

        void OnMeshRemoved(Mesh mesh)
        {
            if (mesh == null)
                return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.meshMemoryProfiler -= Profiler.GetRuntimeMemorySizeLong(mesh);
#endif
            MarkDirty();
        }

        void OnAnimationClipAdded(AnimationClip animationClip)
        {
            if (animationClip == null)
                return;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.animationClipMemoryProfiler += Profiler.GetRuntimeMemorySizeLong(animationClip);
#endif
            MarkDirty();
        }

        void OnAnimationClipRemoved(AnimationClip animationClip)
        {
            if (animationClip == null)
                return;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.animationClipMemoryProfiler -= Profiler.GetRuntimeMemorySizeLong(animationClip);
#endif
            MarkDirty();
        }

        void OnAudioClipAdded(AudioClip audioClip)
        {
            if (audioClip == null)
                return;

            MarkDirty();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.audioClipMemoryProfiler += Profiler.GetRuntimeMemorySizeLong(audioClip);
#endif
            currentCountValue.audioClipMemoryScore += MetricsScoreUtils.ComputeAudioClipScore(audioClip);
        }

        void OnAudioClipRemoved(AudioClip audioClip)
        {
            if (audioClip == null)
                return;

            MarkDirty();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            currentCountValue.audioClipMemoryProfiler -= Profiler.GetRuntimeMemorySizeLong(audioClip);
#endif
            currentCountValue.audioClipMemoryScore -= MetricsScoreUtils.ComputeAudioClipScore(audioClip);
        }


        private void OnMeshDataSizeChange(long current, long previous)
        {
            MarkDirty();
            currentCountValue.meshMemoryScore = current;
        }

        void OnAnimationClipSizeChange(long animationClipSize, long previous)
        {
            MarkDirty();
            currentCountValue.animationClipMemoryScore = animationClipSize;
        }

        void OnTextureAdded(Texture texture)
        {
            if (texture == null)
                return;

            MarkDirty();

            if (texture is Texture2D tex2D)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                currentCountValue.textureMemoryProfiler += Profiler.GetRuntimeMemorySizeLong(tex2D);
#endif
                currentCountValue.textureMemoryScore += MetricsScoreUtils.ComputeTextureScore(tex2D);
            }
        }

        void OnTextureRemoved(Texture texture)
        {
            if (texture == null)
                return;

            MarkDirty();

            if (texture is Texture2D tex2D)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                currentCountValue.textureMemoryProfiler -= Profiler.GetRuntimeMemorySizeLong(tex2D);
#endif
                currentCountValue.textureMemoryScore -= MetricsScoreUtils.ComputeTextureScore(tex2D);
            }
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
        
        void OnDataChanged(long obj1)
        {
            MarkDirty();
        }

        private void UpdateMetrics()
        {
            if (sceneNumber <= 0 || data == null || !data.sceneData.ContainsKey(sceneNumber))
                return;

            if (data != null && data.sceneData.ContainsKey(sceneNumber))
            {
                var sceneData = data.sceneData[sceneNumber];

                if (sceneData != null)
                {
                    currentCountValue.materials = sceneData.materials.Count();
                    currentCountValue.textures = sceneData.textures.Count();
                    currentCountValue.meshes = sceneData.meshes.Count();
                    currentCountValue.entities = sceneData.owners.Count();
                    currentCountValue.bodies = sceneData.renderers.Count();
                    currentCountValue.triangles = sceneData.triangles.Get() / 3;
                }
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

                if (!metricsData.worstMetricOffenses.ContainsKey(sceneNumber))
                {
                    firstOffense = true;
                    metricsData.worstMetricOffenses[sceneNumber] = currentCountValue.Clone();
                }

                SceneMetricsModel worstOffense = metricsData.worstMetricOffenses[sceneNumber];
                SceneMetricsModel currentOffense = maxCountValue - currentCountValue;

                if ( firstOffense )
                    logger.Verbose($"New offending scene with scene number {sceneNumber} ({scenePosition})!\n{currentCountValue}");

                if ( currentOffense < worstOffense )
                    return;

                metricsData.worstMetricOffenses[sceneNumber] = currentOffense;
                logger.Verbose($"New offending scene with scene number {sceneNumber} {scenePosition}!\nmetrics: {currentCountValue}\nlimits: {maxCountValue}\ndelta:{currentOffense}");
            }
        }
        
        private void RaiseMetricsUpdate()
        {
            UpdateWorstMetricsOffense();
            if (!currentCountValue.Equals(lastCountValue))
            {
                lastCountValue = currentCountValue;
                OnMetricsUpdated?.Invoke(this);
            }
        }
    }
}