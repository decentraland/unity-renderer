using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController
    {
        private const int SAMPLES_SIZE = 1000; // Send performance report every 1000 samples

        private LinealBufferHiccupCounter tracker = new LinealBufferHiccupCounter();
        private char[] encodedSamples = new char[SAMPLES_SIZE];
        private PerformanceMetricsDataVariable performanceMetricsDataVariable;
        private int currentIndex = 0;

        public PerformanceMetricsController()
        {
            performanceMetricsDataVariable = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData");
        }

        public void Update()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (!CommonScriptableObjects.focusState.Get())
                return;
#endif
            if (!CommonScriptableObjects.rendererState.Get())
                return;

            var deltaInMs = Time.deltaTime * 1000;

            tracker.AddDeltaTime(Time.deltaTime);

            performanceMetricsDataVariable.Set(tracker.CurrentFPSCount(),
                tracker.CurrentHiccupCount(),
                tracker.HiccupsSum,
                tracker.GetTotalSeconds());

            encodedSamples[currentIndex++] = (char) deltaInMs;

            if (currentIndex == SAMPLES_SIZE)
            {
                currentIndex = 0;
                Report(new string(encodedSamples));
            }
        }

        private void Report(string encodedSamples)
        {
            Dictionary<string,IParcelScene>.ValueCollection loadedScenesValues = Environment.i.world.state.loadedScenes.Values;

            var totalMemoryScore = 0L;
            foreach (IParcelScene parcelScene in loadedScenesValues)
            {
                var coords = parcelScene.sceneData.basePosition;
                long parcelMemoryScore = parcelScene.metricsCounter.currentCount.totalMemoryScore;
                totalMemoryScore += parcelMemoryScore;
                
                Debug.Log($"Memory score: ({coords.x},{coords.y}) {parcelMemoryScore}");
            }
            
            // Get all PerformanceAnalytics data here
            // TODO: Add above data to the performance report
            WebInterface.SendPerformanceReport(encodedSamples, 
                Settings.i.qualitySettings.Data.fpsCap,
                tracker.CurrentHiccupCount(),
                tracker.GetHiccupSum(),
                tracker.GetTotalSeconds());
        }
    }
}