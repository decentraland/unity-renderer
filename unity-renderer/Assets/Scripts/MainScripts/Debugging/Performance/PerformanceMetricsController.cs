using System.Text;
using DCL.Interface;
using DCL.FPSDisplay;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController
    {
        private LinealBufferHiccupCounter tracker = new LinealBufferHiccupCounter();
        private const int SAMPLES_SIZE = 1000;
        private char[] encodedSamples = new char[SAMPLES_SIZE];
        private int currentIndex = 0;

        [SerializeField] private PerformanceMetricsDataVariable performanceMetricsDataVariable;

        public PerformanceMetricsController()
        {
            performanceMetricsDataVariable = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData");
        }

        public void Update()
        {
#if !UNITY_EDITOR
            if (!CommonScriptableObjects.focusState.Get())
                return;
#endif
            if (!CommonScriptableObjects.rendererState.Get())
                return;

            var deltaInMs = Time.deltaTime * 1000;

            tracker.AddDeltaTime(Time.deltaTime);

            performanceMetricsDataVariable?.Set(tracker.CurrentFPSCount(), tracker.CurrentHiccupCount(), tracker.HiccupsSum, tracker.GetTotalSeconds());

            encodedSamples[currentIndex++] = (char)deltaInMs;

            if (currentIndex == SAMPLES_SIZE)
            {
                currentIndex = 0;
                Report(new string(encodedSamples));
                GenerateHiccupReport();
            }
        }

        private void Report(string encodedSamples)
        {
            WebInterface.SendPerformanceReport(encodedSamples, Settings.i.currentQualitySettings.fpsCap);
        }

        private void GenerateHiccupReport()
        {
            WebInterface.SendPerformanceHiccupReport(tracker.CurrentHiccupCount(), tracker.GetHiccupSum(), tracker.GetTotalSeconds());
        }
    }
}