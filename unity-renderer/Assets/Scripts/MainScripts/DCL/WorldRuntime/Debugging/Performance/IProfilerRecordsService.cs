using DCL;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public interface IProfilerRecordsService: IService
    {
        float LastFrameTimeInSec { get; }
        float LastFrameTimeInMS { get; }
        float LastFPS { get; }
        public (float FrameTime, float FPS) AverageData { get; }

        long TotalAllocSample { get; }
        long TotalMemoryUsage { get; }
        long TotalMemoryReserved { get; }
        long DrawCalls { get; }

        void RecordAdditionalProfilerMetrics();
    }
}
