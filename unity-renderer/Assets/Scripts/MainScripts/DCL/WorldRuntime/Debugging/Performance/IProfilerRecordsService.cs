using DCL;

namespace MainScripts.DCL.WorldRuntime.Debugging.Performance
{
    public interface IProfilerRecordsService: IService
    {
        float LastFrameTimeInSec { get; }
        float LastFrameTimeInMS { get; }
        float LastFPS { get; }
        float AverageFrameTime { get; }
        float AverageFPS { get; }
    }
}
