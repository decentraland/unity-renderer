using System;
using DCL;
using DCL.Interface;

public class TimeReporter : IDisposable
{
    private float timeNormalizationFactor;
    private float cycleTime;
    private bool wasPaused = true;

    internal event Action<float, bool> OnReport;

    public TimeReporter()
    {
        OnReport += DoReport;
    }

    public void Dispose()
    {
        OnReport -= DoReport;
    }

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
        OnReport?.Invoke(time, isPaused);
    }
    
    private void DoReport(float time, bool isPaused)
    {
        WebInterface.ReportTime(time, isPaused, timeNormalizationFactor, cycleTime);
    }
}