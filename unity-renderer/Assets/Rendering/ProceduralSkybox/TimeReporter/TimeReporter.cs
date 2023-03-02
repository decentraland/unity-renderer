using System;
using DCL;
using DCL.Interface;

public interface ITimeReporter : IDisposable
{
    void Configure(float normalizationFactor, float cycle);
    void ReportTime(float time);
}

public class TimeReporter : ITimeReporter
{
    private float timeNormalizationFactor;
    private float cycleTime;
    private bool wasTimeRunning;

    internal delegate void OnReportEventHandler(float time, bool isPaused);
    internal event OnReportEventHandler OnReport;

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
        bool isTimeRunning = DataStore.i.skyboxConfig.mode.Get() == SkyboxMode.Dynamic;

        // NOTE: if not paused and pause state didn't change there is no need to report
        // current time since it's being calculated on kernel side
        if (isTimeRunning && wasTimeRunning)
            return;

        wasTimeRunning = isTimeRunning;
        OnReport?.Invoke(time, !isTimeRunning);
    }

    private void DoReport(float time, bool isPaused)
    {
        WebInterface.ReportTime(time, isPaused, timeNormalizationFactor, cycleTime);
    }
}
