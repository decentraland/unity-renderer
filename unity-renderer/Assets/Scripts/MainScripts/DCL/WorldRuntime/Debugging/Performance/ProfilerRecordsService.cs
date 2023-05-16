using Unity.Profiling;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public class ProfilerRecordsService : IProfilerRecordsService
    {
        private const int CAPACITY = 15; // Amounts of frames used for calculating average FPS

        private ProfilerRecorder mainThreadTimeRecorder;

        private ProfilerRecorder drawCallsRecorder;
        private ProfilerRecorder reservedMemoryRecorder;
        private ProfilerRecorder usedMemoryRecorder;
        private ProfilerRecorder gcAllocatedInFrameRecorder;

        private bool isRecordingAdditionalProfilers;

        public float LastFrameTimeInMS => mainThreadTimeRecorder.LastValue * 1e-6f; // [sec]
        public float LastFPS => 1 / (mainThreadTimeRecorder.LastValue * 1e-9f);

        /// <summary>
        /// Average frame time and FPS. In-game use only due to overhead (not for external analytics)
        /// </summary>
        public (float FrameTime, float FPS) AverageData
        {
            get
            {
                float frameTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * 1e-6f; // [ms]
                return (FrameTime: frameTime, FPS: 1000 / frameTime);
            }
        }

        // Additional recordings
        public long GcAllocatedInFrame => gcAllocatedInFrameRecorder.LastValue;
        public long UsedMemory => usedMemoryRecorder.LastValue;
        public long ReservedMemory => reservedMemoryRecorder.LastValue;
        public long DrawCalls => drawCallsRecorder.LastValue;

        public ProfilerRecordsService()
        {
            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", CAPACITY);
        }

        public void Initialize() { }

        public void Dispose()
        {
            mainThreadTimeRecorder.Dispose();

            drawCallsRecorder.Dispose();
            reservedMemoryRecorder.Dispose();
            usedMemoryRecorder.Dispose();
            gcAllocatedInFrameRecorder.Dispose();
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

        private static float GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            int samplesCount = recorder.Capacity;

            if (samplesCount == 0)
                return 0;

            float r = 0;

            unsafe
            {
                ProfilerRecorderSample* samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);

                for (var i = 0; i < samplesCount; ++i)
                    r += samples[i].Value;
            }

            return r / samplesCount;
        }
    }
}
