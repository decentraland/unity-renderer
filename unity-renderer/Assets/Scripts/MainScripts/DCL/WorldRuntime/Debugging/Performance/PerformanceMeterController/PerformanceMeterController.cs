using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCL.FPSDisplay;
using DCL.SettingsCommon;
using Newtonsoft.Json;
using Unity.Profiling;

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
            public float frameTimeMs;

            public override string ToString()
            {
                return "frame number: " + frameNumber
                                        + "\n frame consumed milliseconds: " + millisecondsConsumed
                                        + "\n is hiccup: " + isHiccup
                                        + "\n frame time: " + frameTimeMs;
            }

            public int CompareTo(object obj)
            {
                // 0    -> this and otherSample are equal
                // 1    -> this is greater
                // -1   -> otherSample is greater

                SampleData otherSample = obj as SampleData;

                if (otherSample == null)
                    return 1;

                if (Math.Abs(this.frameTimeMs - otherSample.frameTimeMs) < float.Epsilon)
                    return 0;

                return this.frameTimeMs > otherSample.frameTimeMs ? 1 : -1;
            }
        }

        private PerformanceMetricsDataVariable metricsData;
        private float currentDurationInSeconds = 0f;
        private float targetDurationInSeconds = 0f;
        private List<SampleData> samples = new List<SampleData>();

        // auxiliar data
        private SampleData lastSavedSample;

        // reported data
        private double highestFrameTime;
        private double lowestFrameTime;
        private double averageFrameTime;
        private double marginOfError;

        private float percentile1FrameTime;
        private float percentile50FrameTime;
        private float percentile99FrameTime;
        private int totalHiccupFrames;
        private float totalHiccupsTimeInSeconds;
        private int totalFrames;
        private float totalFramesTimeInSeconds;
        private long lowestAllocation;
        private long highestAllocation;
        private long averageAllocation;
        private long totalAllocation;
        private ProfilerRecorder gcAllocatedInFrameRecorder;
        
        private bool justStarted = false;

        public PerformanceMeterController() { metricsData = Resources.Load<PerformanceMetricsDataVariable>("ScriptableObjects/PerformanceMetricsData"); }

        private void ResetDataValues()
        {
            samples.Clear();
            currentDurationInSeconds = 0f;
            targetDurationInSeconds = 0f;

            lastSavedSample = null;

            highestFrameTime = 0;
            lowestFrameTime = 0;
            averageFrameTime = 0;
            percentile50FrameTime = 0;
            percentile99FrameTime = 0;
            totalHiccupFrames = 0;
            totalHiccupsTimeInSeconds = 0;
            totalFrames = 0;
            totalFramesTimeInSeconds = 0;
            lowestAllocation = long.MaxValue;
            highestAllocation = 0;
            averageAllocation = 0;
            totalAllocation = 0;
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
            justStarted = true;
            gcAllocatedInFrameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
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
            // we avoid the first frame as when we are in editor, the context menu pauses everything and the next frame is chaotic
            if (justStarted)
            {
                justStarted = false;
                return;
            }
            
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

            float frameTimeMs = Time.deltaTime * 1000f;

            SampleData newSample = new SampleData
            {
                frameTimeMs = frameTimeMs,
                frameNumber = Time.frameCount,
                millisecondsConsumed = secondsConsumed * 1000,
                currentTime = Time.timeSinceLevelLoad,
                isHiccup = secondsConsumed > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS
            };

            samples.Add(newSample);
            lastSavedSample = newSample;

            if (newSample.isHiccup)
            {
                totalHiccupFrames++;
                totalHiccupsTimeInSeconds += secondsConsumed;
            }

            UpdateAllocations();

            totalFrames++;

            currentDurationInSeconds += Time.deltaTime;

            if (currentDurationInSeconds > targetDurationInSeconds)
            {
                totalFramesTimeInSeconds = currentDurationInSeconds;
                StopSampling();
            }
        }

        private void UpdateAllocations()
        {
            long lastAllocation = gcAllocatedInFrameRecorder.LastValue;

            if (highestAllocation < lastAllocation)
            {
                highestAllocation = lastAllocation;
            }

            if (lowestAllocation > lastAllocation)
            {
                lowestAllocation = lastAllocation;
            }

            totalAllocation += lastAllocation;
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

            var benchmark = new BenchmarkResult(sortedSamples.Select(sample => (double)sample.frameTimeMs).ToArray());

            highestFrameTime = benchmark.max;
            lowestFrameTime = benchmark.min;
            averageFrameTime = benchmark.mean;
            marginOfError = benchmark.rme;
            
            percentile1FrameTime = sortedSamples[Mathf.Min(Mathf.CeilToInt(samplesCount * 0.01f), sortedSamples.Count-1)].frameTimeMs;
            percentile50FrameTime = sortedSamples[Mathf.Min(Mathf.CeilToInt(samplesCount * 0.5f), sortedSamples.Count-1)].frameTimeMs;
            percentile99FrameTime = sortedSamples[Mathf.Min(Mathf.CeilToInt(samplesCount * 0.99f), sortedSamples.Count-1)].frameTimeMs;
            
            averageAllocation = totalAllocation / sortedSamples.Count;
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
                + "\n * Quality Settings -> Camera Draw Distance -> " +
                Settings.i.qualitySettings.Data.cameraDrawDistance
                + "\n * Quality Settings -> Detail Object Culling Enabled -> " +
                Settings.i.qualitySettings.Data.enableDetailObjectCulling
                + "\n * Quality Settings -> Detail Object Culling Limit -> " +
                Settings.i.qualitySettings.Data.detailObjectCullingLimit
                + "\n * Quality Settings -> Reflection Quality -> " +
                Settings.i.qualitySettings.Data.reflectionResolution
            );

            // Step 2 - report processed data
            string format = "F1";

            Log($"Data report step 2 - Processed values:" +
                $"\n * PERFORMANCE SCORE (0-100) -> {CalculatePerformanceScore()}" +
                $"\n * lowest frame time -> {lowestFrameTime.ToString(format)}ms" +
                $"\n * average frame time -> {averageFrameTime.ToString(format)}ms" +
                $"\n * highest frame time -> {highestFrameTime.ToString(format)}ms" +
                $"\n * 1 percentile frame time -> {percentile1FrameTime.ToString(format)}ms" +
                $"\n * 50 percentile frame time -> {percentile50FrameTime.ToString(format)}ms" +
                $"\n * 99 percentile frame time -> {percentile99FrameTime.ToString(format)}ms" +
                $"\n * error percentage -> ±{marginOfError.ToString(format)}%" +
                $"\n * total hiccups (>{FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS}ms frames) -> {totalHiccupFrames} ({CalculateHiccupsPercentage()}% of frames were hiccups)" +
                $"\n * total hiccups time (seconds) -> {totalHiccupsTimeInSeconds}" +
                $"\n * total frames -> {totalFrames}" +
                $"\n * total frames time (seconds) -> {totalFramesTimeInSeconds}" +
                $"\n * lowest allocations (kb) -> {lowestAllocation / 1000.0}" +
                $"\n * highest allocations (kb) -> {highestAllocation / 1000.0}" +
                $"\n * average allocations (kb) -> {averageAllocation / 1000.0}" +
                $"\n * total allocations (kb) -> {totalAllocation / 1000.0}"
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
        private int CalculatePerformanceScore()
        {
            double desiredFrameTime = Settings.i.qualitySettings.Data.fpsCap ? 1000/30.0 : 1000/60.0;
            double frameScore = Mathf.Min((float)(desiredFrameTime/ averageFrameTime), 1); // from 0 to 1
            double hiccupsScore = 1 - (float) totalHiccupFrames / samples.Count; // from 0 to 1
            double performanceScore = (frameScore + hiccupsScore) / 2 * 100; // scores sum / amount of scores * 100 to have a 0-100 scale

            return Mathf.RoundToInt((float)performanceScore * 100f) / 100;
        }

        private float CalculateHiccupsPercentage()
        {
            float percentage = ((float) totalHiccupFrames / totalFrames) * 100;
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