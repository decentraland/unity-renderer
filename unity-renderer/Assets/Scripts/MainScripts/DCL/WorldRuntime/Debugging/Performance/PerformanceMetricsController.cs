using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController
    {
        private const int SAMPLES_SIZE = 1000; // Send performance report every 1000 samples
        private const string PROFILER_METRICS_FEATURE_FLAG = "profiler_metrics";

        private readonly char[] encodedSamples = new char[SAMPLES_SIZE];
        private readonly Dictionary<int, long> scenesMemoryScore = new ();

        private readonly PerformanceMetricsDataVariable performanceMetricsDataVariable;
        private readonly LinealBufferHiccupCounter tracker;
        private readonly IProfilerRecordsService profilerRecordsService;

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;
        private IWorldState worldState => Environment.i.world.state;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private GeneralSettings generalSettings => Settings.i.generalSettings.Data;

        private bool trackProfileRecords;
        private int currentIndex;
        private long totalAllocSample;

        public PerformanceMetricsController()
        {
            performanceMetricsDataVariable = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData");

            profilerRecordsService = Environment.i.serviceLocator.Get<IProfilerRecordsService>();
            tracker = new LinealBufferHiccupCounter(SAMPLES_SIZE);

            featureFlags.OnChange += OnFeatureFlagChange;
            OnFeatureFlagChange(featureFlags.Get(), null);
        }

        private void OnFeatureFlagChange(FeatureFlag current, FeatureFlag previous)
        {
            trackProfileRecords = current.IsFeatureEnabled(PROFILER_METRICS_FEATURE_FLAG);

            if (trackProfileRecords)
                profilerRecordsService.RecordAdditionalProfilerMetrics();
        }

        public void Update()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (!CommonScriptableObjects.focusState.Get())
                return;
#endif
            if (!CommonScriptableObjects.rendererState.Get() && !CommonScriptableObjects.forcePerformanceMeter.Get())
                return;

            tracker.AddDeltaTime(Time.unscaledDeltaTime);
            performanceMetricsDataVariable.Set(profilerRecordsService.LastFPS, tracker.HiccupsCountInBuffer, tracker.HiccupsSum, tracker.TotalSeconds);

            encodedSamples[currentIndex++] = (char)profilerRecordsService.LastFrameTimeInMS;

            if (trackProfileRecords)
                totalAllocSample +=  profilerRecordsService.GcAllocatedInFrame;

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

                scenesMemoryScore.Add(parcelSceneValue.sceneData.sceneNumber, parcelSceneValue.metricsCounter.currentCount.totalMemoryScore);
            }

            object drawCalls = null;
            object totalMemoryReserved = null;
            object totalMemoryUsage = null;
            object totalGCAlloc = null;

            if (trackProfileRecords)
            {
                drawCalls = (int)profilerRecordsService.DrawCalls;
                totalMemoryReserved = profilerRecordsService.ReservedMemory;
                totalMemoryUsage = profilerRecordsService.UsedMemory;
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

            int hiccupsInThousandFrames = tracker.HiccupsCountInBuffer;
            float hiccupsTime = tracker.HiccupsSum;

            float totalTime = tracker.TotalSeconds;

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

            string result = JsonConvert.SerializeObject(performanceReportPayload);
            WebInterface.SendPerformanceReport(result);
            PerformanceAnalytics.ResetAll();
        }
    }
}
