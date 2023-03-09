using System.Collections.Generic;
using Unity.Profiling;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public class ProfilerRecordsService : IProfilerRecordsService
    {
        private const int CAPACITY = 15;

        private readonly List<ProfilerRecorderSample> samples;

        private ProfilerRecorder mainThreadTimeRecorder;

        public float LastFrameTimeInSec => mainThreadTimeRecorder.LastValue * 1e-9f; // [nanoseconds] -> [sec]
        public float LastFrameTimeInMS => mainThreadTimeRecorder.LastValue * 1e-6f; // [nanoseconds] -> [ms]
        public float LastFPS => 1000 / LastFrameTimeInMS;

        public float AverageFrameTime => GetRecorderFrameAverage(mainThreadTimeRecorder) * 1e-6f;
        public float AverageFPS => 1000 / AverageFrameTime;

        public ProfilerRecordsService()
        {
            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", CAPACITY);
            samples = new List<ProfilerRecorderSample>(CAPACITY);
        }

        public void Initialize() { }

        public void Dispose()
        {
            mainThreadTimeRecorder.Dispose();
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
