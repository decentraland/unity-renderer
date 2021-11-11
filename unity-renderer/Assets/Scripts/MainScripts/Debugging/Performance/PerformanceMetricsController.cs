using System.Collections.Generic;
using DCL.Interface;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController
    {
        private const float REPORT_TIME_IN_SECONDS = 20f;

        private readonly LinealBufferHiccupCounter tracker = new LinealBufferHiccupCounter();
        private readonly PerformanceMetricsDataVariable performanceMetricsDataVariable;

        private readonly List<char> encodedSamples = new List<char>();
        private float lastReportTime;

        public PerformanceMetricsController()
        {
            performanceMetricsDataVariable = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData");
            lastReportTime = 0;
        }

        public void Update()
        {
#if !UNITY_EDITOR
            if (!CommonScriptableObjects.focusState.Get())
                return;
#endif
            if (!CommonScriptableObjects.rendererState.Get())
                return;

            TrackFrame();

            if (IsTimeToReport())
            {
                ReportFrame();
            }
        }
        private void TrackFrame()
        {
            var deltaInMs = Time.deltaTime * 1000;
            tracker.AddDeltaTime(Time.deltaTime);
            performanceMetricsDataVariable.Set(tracker.CurrentFPSCount(), tracker.CurrentHiccupCount(), tracker.HiccupsSum, tracker.GetTotalSeconds());
            encodedSamples.Add((char)deltaInMs);
        }
        private void ReportFrame()
        {
            lastReportTime = Time.unscaledTime;
            
            string samples = new string(encodedSamples.ToArray());
            WebInterface.SendPerformanceReport(samples, 
                Settings.i.qualitySettings.Data.fpsCap, 
                tracker.CurrentHiccupCount(),
                tracker.GetHiccupSum(),
                tracker.GetTotalSeconds());
            
            encodedSamples.Clear();
        }
        private bool IsTimeToReport() { return Time.unscaledTime - lastReportTime > REPORT_TIME_IN_SECONDS; }

    }
}