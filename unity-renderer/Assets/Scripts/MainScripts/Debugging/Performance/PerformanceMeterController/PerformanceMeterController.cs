using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using Newtonsoft.Json;

namespace DCL
{
    /// <summary>
    /// Performance Meter Tool
    ///
    /// It samples frames performance data for the target duration and prints a complete report when finished.
    ///
    /// There are 2 ways to trigger this tool usage:
    /// A: While the client is running in the browser, open the browser console and run "clientDebug.RunPerformanceMeterTool(10);" to run for 10 seconds.
    /// B: In Unity Editor select the "Main" gameobject and right-click on its DebugBridge Monobehaviour, from there a debug method can be selected to run the tool for 10 seconds.
    /// </summary>
    public class PerformanceMeterController
    {
        private class SampleData : IComparable
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

            public int CompareTo(object obj)
            {
                // 0    -> this and otherSample are equal
                // 1    -> this is greater
                // -1   -> otherSample is greater

                SampleData otherSample = obj as SampleData;

                if (otherSample == null)
                    return 1;

                if (this.fpsAtThisFrameInTime == otherSample.fpsAtThisFrameInTime)
                    return 0;

                return this.fpsAtThisFrameInTime > otherSample.fpsAtThisFrameInTime ? 1 : -1;
            }
        }

        private PerformanceMetricsDataVariable metricsData;
        private float currentDurationInSeconds = 0f;
        private float targetDurationInSeconds = 0f;
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
            currentDurationInSeconds = 0f;
            targetDurationInSeconds = 0f;

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
        /// <param name="durationInSeconds">The target duration for the running of the tool, after which a report will be printed in the console</param>
        public void StartSampling(float durationInSeconds)
        {
            Log("Start running... target duration: " + durationInSeconds + " seconds");

            ResetDataValues();

            targetDurationInSeconds = durationInSeconds;

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
                Log("No samples were gathered, the duration time in seconds set is probably too small");
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
            float secondsConsumed = 0;

            if (lastSavedSample != null)
            {
                if (lastSavedSample.frameNumber == Time.frameCount)
                {
                    Log("PerformanceMetricsDataVariable changed more than once in the same frame!");
                    return;
                }

                secondsConsumed = Time.timeSinceLevelLoad - lastSavedSample.currentTime;
            }

            SampleData newSample = new SampleData()
            {
                frameNumber = Time.frameCount,
                fpsAtThisFrameInTime = newData.fpsCount,
                millisecondsConsumed = secondsConsumed * 1000,
                currentTime = Time.timeSinceLevelLoad
            };
            newSample.isHiccup = secondsConsumed > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;
            samples.Add(newSample);
            lastSavedSample = newSample;

            if (newSample.isHiccup)
            {
                totalHiccupFrames++;
                totalHiccupsTimeInSeconds += secondsConsumed;
            }

            fpsSum += newData.fpsCount;

            totalFrames++;

            currentDurationInSeconds += Time.deltaTime;
            if (currentDurationInSeconds > targetDurationInSeconds)
            {
                totalFramesTimeInSeconds = currentDurationInSeconds;
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
            sortedSamples.Sort();

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
                + "\n * Sampling duration in seconds -> " + targetDurationInSeconds
                + "\n * System Info -> Operating System -> " + SystemInfo.operatingSystem
                + "\n * System Info -> Device Name -> " + SystemInfo.deviceName
                + "\n * System Info -> Graphics Device Name -> " + SystemInfo.graphicsDeviceName
                + "\n * System Info -> System RAM Size -> " + SystemInfo.systemMemorySize
                + "\n * General Settings -> Scenes Load Radius -> " + Settings.i.generalSettings.Data.scenesLoadRadius
                + "\n * Quality Settings -> FPSCap -> " + Settings.i.qualitySettings.Data.fpsCap
                + "\n * Quality Settings -> Bloom -> " + Settings.i.qualitySettings.Data.bloom
                + "\n * Quality Settings -> Shadow -> " + Settings.i.qualitySettings.Data.shadows
                + "\n * Quality Settings -> Antialising -> " + Settings.i.qualitySettings.Data.antiAliasing
                + "\n * Quality Settings -> Base Resolution -> " + Settings.i.qualitySettings.Data.baseResolution
                + "\n * Quality Settings -> Display Name -> " + Settings.i.qualitySettings.Data.displayName
                + "\n * Quality Settings -> Render Scale -> " + Settings.i.qualitySettings.Data.renderScale
                + "\n * Quality Settings -> Shadow Distance -> " + Settings.i.qualitySettings.Data.shadowDistance
                + "\n * Quality Settings -> Shadow Resolution -> " + Settings.i.qualitySettings.Data.shadowResolution
                + "\n * Quality Settings -> Soft Shadows -> " + Settings.i.qualitySettings.Data.softShadows
                + "\n * Quality Settings -> SSAO Quality -> " + Settings.i.qualitySettings.Data.ssaoQuality
                + "\n * Quality Settings -> Camera Draw Distance -> " + Settings.i.qualitySettings.Data.cameraDrawDistance
                + "\n * Quality Settings -> Detail Object Culling Enabled -> " + Settings.i.qualitySettings.Data.enableDetailObjectCulling
                + "\n * Quality Settings -> Detail Object Culling Limit -> " + Settings.i.qualitySettings.Data.detailObjectCullingLimit
                + "\n * Quality Settings -> Reflection Quality -> " + Settings.i.qualitySettings.Data.reflectionResolution
            );

            // Step 2 - report processed data
            Log("Data report step 2 - Processed values:"
                + "\n * PERFORMANCE SCORE (0-100) -> " + CalculatePerformanceScore()
                + "\n * average FPS -> " + averageFPS
                + "\n * highest FPS -> " + highestFPS
                + "\n * lowest FPS -> " + lowestFPS
                + "\n * 50 percentile (median) FPS -> " + percentile50FPS
                + "\n * 95 percentile FPS -> " + percentile95FPS
                + $"\n * total hiccups (>{FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS}ms frames) -> {totalHiccupFrames} ({CalculateHiccupsPercentage()}% of frames were hiccups)"
                + "\n * total hiccups time (seconds) -> " + totalHiccupsTimeInSeconds
                + "\n * total frames -> " + totalFrames
                + "\n * total frames time (seconds) -> " + totalFramesTimeInSeconds
            );

            // Step 3 - report all samples data
            string rawSamplesJSON = "Data report step 3 - Raw samples:"
                                    + "\n "
                                    + "{\"frame-samples\": " + JsonConvert.SerializeObject(samples) + "}";

#if !UNITY_WEBGL && UNITY_EDITOR
            string targetFilePath = Application.persistentDataPath + "/PerformanceMeterRawFrames.txt";
            Log("Data report step 3 - Trying to dump raw samples JSON at: " + targetFilePath);
            System.IO.File.WriteAllText(targetFilePath, rawSamplesJSON);
#endif

            Log(rawSamplesJSON);
        }

        /// <summary>
        /// Calculates a performance score from 0 to 100 based on the average FPS (compared to the max possible FPS) and the amount of hiccup frames (compared to the total amount of frames).
        /// </summary>
        private float CalculatePerformanceScore()
        {
            float topFPS = Settings.i.qualitySettings.Data.fpsCap ? 30f : 60f;
            float fpsScore = Mathf.Min(averageFPS / topFPS, 1); // from 0 to 1
            float hiccupsScore = 1 - ((float)totalHiccupFrames / samples.Count); // from 0 to 1
            float performanceScore = (fpsScore + hiccupsScore) / 2 * 100; // scores sum / amount of scores * 100 to have a 0-100 scale
            performanceScore = Mathf.Round(performanceScore * 100f) / 100f; // to save only 2 decimals

            return performanceScore;
        }

        private float CalculateHiccupsPercentage()
        {
            float percentage = ((float)totalHiccupFrames / totalFrames) * 100;
            percentage = Mathf.Round(percentage * 100f) / 100f; // to have 2 decimals
            return percentage;
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