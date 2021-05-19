using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class PerformanceMeterController
    {
        private class SampleData
        {
            public int frameNumber;
            public float fpsCount;
            public float totalSeconds;

            public override string ToString()
            {
                return "frame number: " + frameNumber
                                        + "\n frame consumed seconds: " + totalSeconds
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
        private float totalHiccupsTime;
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
            totalHiccupsTime = 0;
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
                totalSeconds = newData.totalSeconds
            };
            samples.Add(newSample);
            lastSavedSample = newSample;

            fpsSum += newData.fpsCount;
            // totalHiccups += newData.hiccupCount; // TODO: confirm this is per frame and not since level load
            // totalHiccupsTime += newData.hiccupSum; // TODO: confirm this is per frame and not since level load
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
            samples.Sort(samplesFPSComparer);
            int samplesCount = samples.Count;

            highestFPS = samples[samplesCount - 1].fpsCount;
            lowestFPS = samples[0].fpsCount;

            averageFPS = fpsSum / samples.Count;

            percentile50FPS = samples[Mathf.CeilToInt(samplesCount * 0.5f)].fpsCount;
            percentile95FPS = samples[Mathf.CeilToInt(samplesCount * 0.95f)].fpsCount;
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
                      // + "\n * total hiccups -> " + totalHiccups
                      // + "\n * total hiccups time -> " + totalHiccupsTime
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