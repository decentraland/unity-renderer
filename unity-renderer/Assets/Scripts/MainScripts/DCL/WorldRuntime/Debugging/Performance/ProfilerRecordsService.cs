using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public class ProfilerRecordsService : IProfilerRecordsService
    {
        private const int CAPACITY = 15;

        private readonly List<ProfilerRecorderSample> samples;

        private ProfilerRecorder mainThreadTimeRecorder;

        private ProfilerRecorder drawCallsRecorder;
        private ProfilerRecorder reservedMemoryRecorder;
        private ProfilerRecorder usedMemoryRecorder;
        private ProfilerRecorder gcAllocatedInFrameRecorder;

        private bool isRecordingAdditionalProfilers;

        public float LastFrameTimeInSec => mainThreadTimeRecorder.LastValue * 1e-9f; // [sec]
        public float LastFrameTimeInMS => mainThreadTimeRecorder.LastValue * 1e-6f; // [sec]
        public float LastFPS => 1 / LastFrameTimeInSec;

        public (float FrameTime, float FPS) AverageData
        {
            get
            {
                float frameTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * 1e-6f; // [ms]
                return (FrameTime: frameTime, FPS: 1000 / frameTime);
            }
        }

        // Additional recordings
        public long TotalAllocSample => gcAllocatedInFrameRecorder.LastValue;
        public long TotalMemoryUsage => usedMemoryRecorder.LastValue;
        public long TotalMemoryReserved => reservedMemoryRecorder.LastValue;
        public long DrawCalls => drawCallsRecorder.LastValue;

        public ProfilerRecordsService()
        {
            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", CAPACITY);
            samples = new List<ProfilerRecorderSample>(CAPACITY);
        }

        public void Initialize() { }

        public void Dispose()
        {
            mainThreadTimeRecorder.Dispose();

            if (isRecordingAdditionalProfilers)
            {
                drawCallsRecorder.Dispose();
                reservedMemoryRecorder.Dispose();
                usedMemoryRecorder.Dispose();
                gcAllocatedInFrameRecorder.Dispose();
            }
        }

        public void RecordAdditionalProfilerMetrics()
        {
            if (isRecordingAdditionalProfilers) return;

            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            reservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            usedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            gcAllocatedInFrameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

            isRecordingAdditionalProfilers = true;
        }

        public void StartRecordGCAllocatedInFrame()
        {
            if (isRecordingAdditionalProfilers)
                return;

            gcAllocatedInFrameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        }

        public void StopRecordGCAllocatedInFrame()
        {
            if (isRecordingAdditionalProfilers)
                return;

            gcAllocatedInFrameRecorder.Dispose();
        }

        private float GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            int samplesCount = recorder.Capacity;

            if (samplesCount == 0)
                return 0;

            samples.Clear();
            recorder.CopyTo(samples);

            float r = 0;

            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;

            return r / samplesCount;
        }
    }
}
