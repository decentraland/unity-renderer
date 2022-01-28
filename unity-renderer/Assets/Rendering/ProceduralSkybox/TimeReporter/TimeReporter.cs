using DCL;
using DCL.Interface;

public class TimeReporter
{
    private float timeNormalizationFactor;
    private float cycleTime;
    private bool wasPaused = true;

    public void Configure(float normalizationFactor, float cycle)
    {
        timeNormalizationFactor = normalizationFactor;
        cycleTime = cycle;
    }

    public void ReportTime(float time)
    {
        bool isPaused = !DataStore.i.skyboxConfig.useDynamicSkybox.Get();

        // NOTE: if not paused and pause state didn't change there is no need to report
        // current time since it's being calculated on kernel side
        if (!isPaused && !wasPaused)
        {
            return;
        }

        wasPaused = isPaused;
        DoReport(time, isPaused);
    }

    internal virtual void DoReport(float time, bool isPaused)
    {
        WebInterface.ReportTime(time, isPaused, timeNormalizationFactor, cycleTime);
    }
}