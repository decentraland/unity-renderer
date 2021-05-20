using System.Collections.Generic;
using UnityEngine;
using DCL.FPSDisplay;

namespace DCL
{
    public class PerformanceMeterController
    {
        private class SampleData
        {
            public int frameNumber;
            public float fpsCount;
            public float totalMilliseconds;
            public float currentTime;
            public bool isHiccup = false;

            public override string ToString()
            {
                return "frame number: " + frameNumber
                                        + "\n frame consumed milliseconds: " + totalMilliseconds
                                        + "\n is hiccup: " + isHiccup
                                        + "\n fps until this frame: " + fpsCount;
            }
        }

        private class SamplesFPSComparer : IComparer<SampleData>
        {
            public int Compare (SampleData sampleA, SampleData sampleB)
            {
                // 0    -> sampleA and sampleB are equal
                // 1    -> sampleA is greater
                // -1   -> sampleB is greater

                if (sampleA == null)
                    return -1;

                if (sampleB == null)
                    return 1;

                if (sampleA.fpsCount == sampleB.fpsCount)
                    return 0;

                return sampleA.fpsCount > sampleB.fpsCount ? 1 : -1;
            }
        }

        private PerformanceMetricsDataVariable metricsData;
        private SamplesFPSComparer samplesFPSComparer = new SamplesFPSComparer();
        private float currentDurationInMilliseconds = 0f;
        private float targetDurationInMilliseconds = 0f;
        private List<SampleData> samples = new List<SampleData>();

        // auxiliar data
        private SampleData lastSavedSample;
        private float fpsSum = 0;

        // reported data
        private float highestFPS;
        private float lowestFPS;
        private float averageFPS;
        private float percentile50FPS;
        private float percentile95FPS;
        private int totalHiccups;
        private float totalHiccupsTimeInSeconds;
        private int totalFrames;
        private float totalFramesTimeInSeconds;

        public PerformanceMeterController() { metricsData = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData"); }

        private void ResetDataValues()
        {
            samples.Clear();
            currentDurationInMilliseconds = 0f;
            targetDurationInMilliseconds = 0f;

            lastSavedSample = null;
            fpsSum = 0;

            highestFPS = 0;
            lowestFPS = 0;
            averageFPS = 0;
            percentile50FPS = 0;
            percentile95FPS = 0;
            totalHiccups = 0;
            totalHiccupsTimeInSeconds = 0;
            totalFrames = 0;
            totalFramesTimeInSeconds = 0;
        }

        public void StartSampling(float durationInMilliseconds)
        {
            Log("PerformanceMeterController - Start running... target duration: " + (durationInMilliseconds / 1000) + " seconds");

            ResetDataValues();

            targetDurationInMilliseconds = durationInMilliseconds;

            metricsData.OnChange += OnMetricsChange;
        }

        public void StopSampling()
        {
            Log("PerformanceMeterController - Stopped running.");

            metricsData.OnChange -= OnMetricsChange;

            if (samples.Count == 0)
            {
                Log("PerformanceMeterController - No samples were gathered, the duration time in milliseconds set is probably too small");
                return;
            }

            ProcessSamples();

            ReportData();
        }

        // PerformanceMetricsController updates the PerformanceMetricsDataVariable SO on every frame
        private void OnMetricsChange(PerformanceMetricsData newData, PerformanceMetricsData oldData)
        {
            if (lastSavedSample != null && lastSavedSample.frameNumber == Time.frameCount)
            {
                Log("PerformanceMeterController - PerformanceMetricsDataVariable changed more than once in the same frame!");
                return;
            }

            SampleData newSample = new SampleData()
            {
                frameNumber = Time.frameCount,
                fpsCount = newData.fpsCount,
                totalMilliseconds = lastSavedSample != null ? (Time.timeSinceLevelLoad - lastSavedSample.currentTime) * 1000 : -1,
                currentTime = Time.timeSinceLevelLoad
            };
            newSample.isHiccup = newSample.totalMilliseconds / 1000 > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;
            samples.Add(newSample);
            lastSavedSample = newSample;

            if (newSample.isHiccup)
            {
                totalHiccups++;
                totalHiccupsTimeInSeconds += newSample.totalMilliseconds / 1000;
            }

            fpsSum += newData.fpsCount;

            totalFrames++;

            currentDurationInMilliseconds += Time.deltaTime * 1000;
            if (currentDurationInMilliseconds > targetDurationInMilliseconds)
            {
                totalFramesTimeInSeconds = currentDurationInMilliseconds / 1000;
                StopSampling();
            }
        }

        private void ProcessSamples()
        {
            // Sort the samples based on FPS count of each one, to be able to calculate the percentiles later
            var sortedSamples = new List<SampleData>(samples);
            sortedSamples.Sort(samplesFPSComparer);
            int samplesCount = sortedSamples.Count;

            highestFPS = sortedSamples[samplesCount - 1].fpsCount;
            lowestFPS = sortedSamples[0].fpsCount;

            averageFPS = fpsSum / sortedSamples.Count;

            percentile50FPS = sortedSamples[Mathf.CeilToInt(samplesCount * 0.5f)].fpsCount;
            percentile95FPS = sortedSamples[Mathf.CeilToInt(samplesCount * 0.95f)].fpsCount;
        }

        private void ReportData()
        {
            // print relevant system info: hardware, cappedFPS, OS, sampling duration, etc.
            Log("PerformanceMeterController - Data report step 1 - System and Graphics info:"
                + "\n * Sampling duration in seconds -> " + (targetDurationInMilliseconds / 1000)
                + "\n * System Info -> Operating System -> " + SystemInfo.operatingSystem
                + "\n * System Info -> Device Name -> " + SystemInfo.deviceName
                + "\n * System Info -> Graphics Device Name -> " + SystemInfo.graphicsDeviceName
                + "\n * System Info -> System RAM Size -> " + SystemInfo.systemMemorySize
                + "\n * General Settings -> Auto Quality ON -> " + Settings.i.generalSettings.autoqualityOn
                + "\n * General Settings -> Scenes Load Radius -> " + Settings.i.generalSettings.scenesLoadRadius
                + "\n * Quality Settings -> FPSCap -> " + Settings.i.currentQualitySettings.fpsCap
                + "\n * Quality Settings -> Bloom -> " + Settings.i.currentQualitySettings.bloom
                + "\n * Quality Settings -> Shadow -> " + Settings.i.currentQualitySettings.shadows
                + "\n * Quality Settings -> Antialising -> " + Settings.i.currentQualitySettings.antiAliasing
                + "\n * Quality Settings -> Base Resolution -> " + Settings.i.currentQualitySettings.baseResolution
                + "\n * Quality Settings -> Color Grading -> " + Settings.i.currentQualitySettings.colorGrading
                + "\n * Quality Settings -> Display Name -> " + Settings.i.currentQualitySettings.displayName
                + "\n * Quality Settings -> Render Scale -> " + Settings.i.currentQualitySettings.renderScale
                + "\n * Quality Settings -> Shadow Distance -> " + Settings.i.currentQualitySettings.shadowDistance
                + "\n * Quality Settings -> Shadow Resolution -> " + Settings.i.currentQualitySettings.shadowResolution
                + "\n * Quality Settings -> Soft Shadows -> " + Settings.i.currentQualitySettings.softShadows
                + "\n * Quality Settings -> SSAO Quality -> " + Settings.i.currentQualitySettings.ssaoQuality
                + "\n * Quality Settings -> Camera Draw Distance -> " + Settings.i.currentQualitySettings.cameraDrawDistance
                + "\n * Quality Settings -> Detail Object Culling Enabled -> " + Settings.i.currentQualitySettings.enableDetailObjectCulling
                + "\n * Quality Settings -> Detail Object Culling Limit -> " + Settings.i.currentQualitySettings.detailObjectCullingLimit
            );

            // print processed data
            Log("PerformanceMeterController - Data report step 2 - Processed values:"
                + "\n * average FPS -> " + averageFPS
                + "\n * highest FPS -> " + highestFPS
                + "\n * lowest FPS -> " + lowestFPS
                + "\n * 50 percentile (median) FPS -> " + percentile50FPS
                + "\n * 95 percentile FPS -> " + percentile95FPS
                + "\n * total hiccups -> " + totalHiccups
                + "\n * total hiccups time (seconds) -> " + totalHiccupsTimeInSeconds
                + "\n * total frames -> " + totalFrames
                + "\n * total frames time (seconds) -> " + totalFramesTimeInSeconds
            );

            // print/dump all samples data
            // TODO: Serialize into a JSON
            // TODO: Try logging into a file as well (if it doesn't work in browser put a conditional for non-browser platforms)
            Log("PerformanceMeterController - Data report step 3 - Raw samples:");
            foreach (SampleData sample in samples)
            {
                Log(sample.ToString());
            }
        }

        private void Log(string message)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            Debug.Log(message);

            Debug.unityLogger.logEnabled = originalLogEnabled;
        }
    }
}