using DCL;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public interface IProfilerRecordsService: IService
    {
        float LastFrameTimeInMS { get; }

        float LastFPS { get; }
        (float FrameTime, float FPS) AverageData { get; }

        long GcAllocatedInFrame { get; }
        long UsedMemory { get; }
        long ReservedMemory { get; }
        long DrawCalls { get; }

        void RecordAdditionalProfilerMetrics();

        void StartRecordGCAllocatedInFrame();

        void StopRecordGCAllocatedInFrame();
    }
}
