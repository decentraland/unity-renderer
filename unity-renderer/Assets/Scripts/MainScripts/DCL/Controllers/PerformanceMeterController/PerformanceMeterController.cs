using System.Collections.Generic;
using UnityEngine;
using DCL.FPSDisplay;
using Newtonsoft.Json;

namespace DCL
{
    /// <summary>
    /// Performance Meter Tool
    ///
    /// It samples frames performance data for the target duration and prints a complete report when finished.
    ///
    /// There are 2 ways to trigger this tool usage:
    /// A: While the client is running in the browser, open the browser console and run "clientDebug.RunPerformanceMeterTool(10000);" to run for 10 seconds.
    /// B: In Unity Editor select the "Main" gameobject and right-click on its DebugBridge Monobehaviour, from there a debug method can be selected to run the tool for 10 seconds.
    /// </summary>
    public class PerformanceMeterController
    {
        private class SampleData
        {
            public int frameNumber;
            public float millisecondsConsumed;
            public bool isHiccup = false;
            public float currentTime;
            public float fpsAtThisFrameInTime;

            public override string ToString()
            {
                return "frame number: " + frameNumber
                                        + "\n frame consumed milliseconds: " + millisecondsConsumed
                                        + "\n is hiccup: " + isHiccup
                                        + "\n fps at this frame: " + fpsAtThisFrameInTime;
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

                if (sampleA.fpsAtThisFrameInTime == sampleB.fpsAtThisFrameInTime)
                    return 0;

                return sampleA.fpsAtThisFrameInTime > sampleB.fpsAtThisFrameInTime ? 1 : -1;
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
        private int totalHiccupFrames;
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
            totalHiccupFrames = 0;
            totalHiccupsTimeInSeconds = 0;
            totalFrames = 0;
            totalFramesTimeInSeconds = 0;
        }

        /// <summary>
        /// Starts the Performance Meter Tool sampling.
        /// </summary>
        /// <param name="durationInMilliseconds">The target duration for the running of the tool, after which a report will be printed in the console</param>
        public void StartSampling(float durationInMilliseconds)
        {
            Log("Start running... target duration: " + (durationInMilliseconds / 1000) + " seconds");

            ResetDataValues();

            targetDurationInMilliseconds = durationInMilliseconds;

            metricsData.OnChange += OnMetricsChange;
        }

        /// <summary>
        /// Stops the Performance Meter Tool sampling, processes the data gathered and prints a full report in the console.
        /// </summary>
        public void StopSampling()
        {
            Log("Stopped running.");

            metricsData.OnChange -= OnMetricsChange;

            if (samples.Count == 0)
            {
                Log("No samples were gathered, the duration time in milliseconds set is probably too small");
                return;
            }

            ProcessSamples();

            ReportData();
        }

        /// <summary>
        /// Callback triggered on every update made to the PerformanceMetricsDataVariable ScriptableObject, done every frame by the PerformanceMetricsController
        /// </summary>
        /// /// <param name="newData">NEW version of the PerformanceMetricsDataVariable ScriptableObject</param>
        /// /// <param name="oldData">OLD version of the PerformanceMetricsDataVariable ScriptableObject</param>
        private void OnMetricsChange(PerformanceMetricsData newData, PerformanceMetricsData oldData)
        {
            if (lastSavedSample != null && lastSavedSample.frameNumber == Time.frameCount)
            {
                Log("PerformanceMetricsDataVariable changed more than once in the same frame!");
                return;
            }

            SampleData newSample = new SampleData()
            {
                frameNumber = Time.frameCount,
                fpsAtThisFrameInTime = newData.fpsCount,
                millisecondsConsumed = lastSavedSample != null ? (Time.timeSinceLevelLoad - lastSavedSample.currentTime) * 1000 : -1,
                currentTime = Time.timeSinceLevelLoad
            };
            newSample.isHiccup = newSample.millisecondsConsumed / 1000 > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;
            samples.Add(newSample);
            lastSavedSample = newSample;

            if (newSample.isHiccup)
            {
                totalHiccupFrames++;
                totalHiccupsTimeInSeconds += newSample.millisecondsConsumed / 1000;
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

        /// <summary>
        /// Process the data gathered from every frame sample to calculate the final highestFPS, lowestFPS, averageFPS, percentile50FPS, percentile95FPS
        /// </summary>
        private void ProcessSamples()
        {
            // Sort the samples based on FPS count of each one, to be able to calculate the percentiles later
            var sortedSamples = new List<SampleData>(samples);
            sortedSamples.Sort(samplesFPSComparer);
            int samplesCount = sortedSamples.Count;

            highestFPS = sortedSamples[samplesCount - 1].fpsAtThisFrameInTime;
            lowestFPS = sortedSamples[0].fpsAtThisFrameInTime;

            averageFPS = fpsSum / sortedSamples.Count;

            percentile50FPS = sortedSamples[Mathf.CeilToInt(samplesCount * 0.5f)].fpsAtThisFrameInTime;
            percentile95FPS = sortedSamples[Mathf.CeilToInt(samplesCount * 0.95f)].fpsAtThisFrameInTime;
        }

        /// <summary>
        /// Formats and prints the final data report following the 3 steps: system info, processed values and frame samples raw data
        /// </summary>
        private void ReportData()
        {
			// TODO: We could build a text file (or html) template with replaceable tags like #OPERATING_SYSTEM, #GRAPHICS_DEVICE, etc. and just replace those values in that output file, instead of printing them in the console.

            // Step 1 - report relevant system info: hardware, cappedFPS, OS, sampling duration, etc.
            Log("Data report step 1 - System and Graphics info:"
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

            // Step 2 - report processed data
            Log("Data report step 2 - Processed values:"
                + "\n * PERFORMANCE SCORE (0-10) -> " + CalculatePerformanceScore()
                + "\n * average FPS -> " + averageFPS
                + "\n * highest FPS -> " + highestFPS
                + "\n * lowest FPS -> " + lowestFPS
                + "\n * 50 percentile (median) FPS -> " + percentile50FPS
                + "\n * 95 percentile FPS -> " + percentile95FPS
                + "\n * total hiccups -> " + totalHiccupFrames
                + "\n * total hiccups time (seconds) -> " + totalHiccupsTimeInSeconds
                + "\n * total frames -> " + totalFrames
                + "\n * total frames time (seconds) -> " + totalFramesTimeInSeconds
            );

            // Step 3 - report all samples data
            string rawSamplesJSON = "Data report step 3 - Raw samples:"
                                    + "\n "
                                    + "{\"frame-samples\": " + JsonConvert.SerializeObject(samples) + "}";

#if !UNITY_WEBGL
            string targetFilePath = Application.persistentDataPath + "/PerformanceMeterRawFrames.txt";
            Log("Data report step 3 - Trying to dump raw samples JSON at: " + targetFilePath);
            System.IO.File.WriteAllText (targetFilePath, rawSamplesJSON);
#endif

            Log(rawSamplesJSON);
        }

        /// <summary>
        /// Calculates a performance score from 0 to 10 based on the average FPS (compared to the max possible FPS) and the amount of hiccup frames (compared to the total amount of frames).
        /// </summary>
        private float CalculatePerformanceScore()
        {
            float topFPS = Settings.i.currentQualitySettings.fpsCap ? 30f : 60f;
            float fpsScore = Mathf.Min(averageFPS / topFPS, 1); // from 0 to 1
            float hiccupsScore = 1 - totalHiccupFrames / samples.Count; // from 0 to 1
            float performanceScore = (fpsScore + hiccupsScore) / 2 * 10; // scores sum / amount of scores * 10 to have a 0-10 scale
            performanceScore = Mathf.Round(performanceScore * 100f) / 100f; // to save only 2 decimals

            return performanceScore;
        }

        /// <summary>
        /// Logs the tool messages in console regardless of the "Debug.unityLogger.logEnabled" value. 
        /// </summary>
        private void Log(string message)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            Debug.Log("PerformanceMeter - " + message);

            Debug.unityLogger.logEnabled = originalLogEnabled;
        }
    }
}