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
                {
                    if (sampleB == null)
                        return 0;

                    return -1;
                }

                if (sampleB == null)
                {
                    return 1;
                }

                if (sampleA == sampleB)
                    return 0;

                return sampleA.fpsCount > sampleB.fpsCount ? 1 : -1;
            }
        }

        private const bool VERBOSE = true;

        private PerformanceMetricsDataVariable metricsData;
        private SamplesFPSComparer samplesFPSComparer = new SamplesFPSComparer();
        private float currentDuration = 0f;
        private float targetDuration = 0f;
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
        private float totalFramesTime;

        public PerformanceMeterController() { metricsData = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData"); }

        private void ResetDataValues()
        {
            samples.Clear();
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
            totalFramesTime = 0;
        }

        public void StartSampling(float durationInMilliseconds)
        {
            if (VERBOSE)
                Debug.Log("PerformanceMeterController - Start running... target duration: " + (durationInMilliseconds / 1000) + " seconds");

            metricsData.OnChange += OnMetricsChange;

            targetDuration = durationInMilliseconds;

            ResetDataValues();
        }

        public void StopSampling()
        {
            if (VERBOSE)
                Debug.Log("PerformanceMeterController - Stopped running.");

            metricsData.OnChange -= OnMetricsChange;

            if (samples.Count == 0)
            {
                Debug.Log("PerformanceMeterController - No samples were gathered, the duration time in milliseconds set is probably too small");
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
                Debug.Log("PerformanceMeterController - PerformanceMetricsDataVariable changed more than once in the same frame!");
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

            currentDuration += Time.deltaTime * 1000;
            if (currentDuration > targetDuration)
            {
                totalFramesTime = currentDuration;
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

            // print processed data
            Debug.Log("PerformanceMeterController - Data report step 2: "
                      + "\n * average FPS -> " + averageFPS
                      + "\n * highest FPS -> " + highestFPS
                      + "\n * lowest FPS -> " + lowestFPS
                      + "\n * 50 percentile (median) FPS -> " + percentile50FPS
                      + "\n * 95 percentile FPS -> " + percentile95FPS
                      + "\n * total hiccups -> " + totalHiccups
                      + "\n * total hiccups time in seconds -> " + totalHiccupsTimeInSeconds
                      + "\n * total frames -> " + totalFrames
                      + "\n * total frames time -> " + totalFramesTime
            );

            // print/dump all samples data
            Debug.Log("PerformanceMeterController - Data report step 3...");
            foreach (SampleData sample in samples)
            {
                Debug.Log(sample);
            }
        }
    }
}