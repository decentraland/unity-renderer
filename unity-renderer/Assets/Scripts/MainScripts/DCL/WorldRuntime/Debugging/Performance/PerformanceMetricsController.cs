using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.Interface;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using Newtonsoft.Json;
using Unity.Profiling;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController
    {
        private const int SAMPLES_SIZE = 1000; // Send performance report every 1000 samples
        private const string PROFILER_METRICS_FEATURE_FLAG = "profiler_metrics";

        private readonly LinealBufferHiccupCounter tracker = new LinealBufferHiccupCounter();
        private readonly char[] encodedSamples = new char[SAMPLES_SIZE];
        private readonly PerformanceMetricsDataVariable performanceMetricsDataVariable;
        private IWorldState worldState => Environment.i.world.state;
        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private GeneralSettings generalSettings => Settings.i.generalSettings.Data;
        private ProfilerRecorder drawCallsRecorder;
        private ProfilerRecorder reservedMemoryRecorder;
        private ProfilerRecorder usedMemoryRecorder;
        private ProfilerRecorder gcAllocatedInFrameRecorder;
        private bool trackProfileRecords;
        private int currentIndex = 0;
        private long totalAllocSample;
        private bool isTrackingProfileRecords = false;
        private readonly Dictionary<string, long> scenesMemoryScore = new Dictionary<string, long>();

        public PerformanceMetricsController()
        {
            performanceMetricsDataVariable = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData");

            featureFlags.OnChange += OnFeatureFlagChange;
            OnFeatureFlagChange(featureFlags.Get(), null);
        }
        private void OnFeatureFlagChange(FeatureFlag current, FeatureFlag previous)
        {
            trackProfileRecords = current.IsFeatureEnabled(PROFILER_METRICS_FEATURE_FLAG);

            if (trackProfileRecords && !isTrackingProfileRecords)
            {
                drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
                reservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
                usedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
                gcAllocatedInFrameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
                isTrackingProfileRecords = true;
            }
        }

        public void Update()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (!CommonScriptableObjects.focusState.Get())
                return;
#endif
            if (!CommonScriptableObjects.rendererState.Get() && !CommonScriptableObjects.forcePerformanceMeter.Get())
                return;

            var deltaInMs = Time.deltaTime * 1000;

            tracker.AddDeltaTime(Time.deltaTime);

            performanceMetricsDataVariable.Set(tracker.CurrentFPSCount(),
                tracker.CurrentHiccupCount(),
                tracker.HiccupsSum,
                tracker.GetTotalSeconds());

            encodedSamples[currentIndex++] = (char) deltaInMs;

            if (trackProfileRecords)
            {
                totalAllocSample += gcAllocatedInFrameRecorder.LastValue;
            }

            if (currentIndex == SAMPLES_SIZE)
            {
                currentIndex = 0;
                Report(new string(encodedSamples));
                totalAllocSample = 0;
            }

        }

        private void Report(string encodedSamples)
        {

            var loadedScenesValues = worldState.GetLoadedScenes();
            scenesMemoryScore.Clear();
            foreach (var parcelScene in loadedScenesValues)
            {
                // we ignore global scene
                IParcelScene parcelSceneValue = parcelScene.Value;

                if (parcelSceneValue.isPersistent)
                    continue; 

                scenesMemoryScore.Add(parcelSceneValue.sceneData.id, parcelSceneValue.metricsCounter.currentCount.totalMemoryScore);
            }

            object drawCalls = null;
            object totalMemoryReserved = null;
            object totalMemoryUsage = null;
            object totalGCAlloc = null;

            if (trackProfileRecords)
            {
                drawCalls = (int)drawCallsRecorder.LastValue;
                totalMemoryReserved = reservedMemoryRecorder.LastValue;
                totalMemoryUsage = usedMemoryRecorder.LastValue;
                totalGCAlloc = totalAllocSample;
            }

            var playerCount = otherPlayers.Count();
            var loadRadius = generalSettings.scenesLoadRadius;

            (int gltfloading, int gltffailed, int gltfcancelled, int gltfloaded) = PerformanceAnalytics.GLTFTracker.GetData();
            (int abloading, int abfailed, int abcancelled, int abloaded) = PerformanceAnalytics.ABTracker.GetData();
            var gltfTextures = PerformanceAnalytics.GLTFTextureTracker.Get();
            var abTextures = PerformanceAnalytics.ABTextureTracker.Get();
            var promiseTextures = PerformanceAnalytics.PromiseTextureTracker.Get();
            var queuedMessages = PerformanceAnalytics.MessagesEnqueuedTracker.Get();
            var processedMessages = PerformanceAnalytics.MessagesProcessedTracker.Get();

            bool usingFPSCap = Settings.i.qualitySettings.Data.fpsCap;

            int hiccupsInThousandFrames = tracker.CurrentHiccupCount();

            float hiccupsTime = tracker.GetHiccupSum();

            float totalTime = tracker.GetTotalSeconds();
            
            WebInterface.PerformanceReportPayload performanceReportPayload = new WebInterface.PerformanceReportPayload
            {
                samples = encodedSamples,
                fpsIsCapped = usingFPSCap,
                hiccupsInThousandFrames = hiccupsInThousandFrames,
                hiccupsTime = hiccupsTime,
                totalTime = totalTime,
                gltfInProgress = gltfloading,
                gltfFailed = gltffailed,
                gltfCancelled = gltfcancelled,
                gltfLoaded = gltfloaded,
                abInProgress = abloading,
                abFailed = abfailed,
                abCancelled = abcancelled,
                abLoaded = abloaded,
                gltfTexturesLoaded = gltfTextures,
                abTexturesLoaded = abTextures,
                promiseTexturesLoaded = promiseTextures,
                enqueuedMessages = queuedMessages,
                processedMessages = processedMessages,
                playerCount = playerCount,
                loadRadius = (int)loadRadius,
                sceneScores = scenesMemoryScore,
                drawCalls = drawCalls,
                memoryReserved = totalMemoryReserved,
                memoryUsage = totalMemoryUsage,
                totalGCAlloc = totalGCAlloc
            };

            var result = JsonConvert.SerializeObject(performanceReportPayload);
            WebInterface.SendPerformanceReport(result);
            PerformanceAnalytics.ResetAll();
        }
    }
}