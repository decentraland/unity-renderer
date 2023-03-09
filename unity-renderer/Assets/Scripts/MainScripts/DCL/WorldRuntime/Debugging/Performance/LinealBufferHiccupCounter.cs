using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using System;

namespace DCL.FPSDisplay
{
    public class LinealBufferHiccupCounter : IDisposable
    {
        private readonly IProfilerRecordsService profilerRecordsService;

        public int HiccupsCountInBuffer { get; private set; }

        public float HiccupsSum { get; private set; }

        public float TotalSeconds { get; private set; }

        public float CurrentFPSCount => profilerRecordsService.LastFPS;

        public LinealBufferHiccupCounter(IProfilerRecordsService profilerRecordsService)
        {
            this.profilerRecordsService = profilerRecordsService;
        }

        public void Dispose() { }

        public void AddDeltaTime(float valueInSeconds)
        {
            float lastFrameTimeInSec = profilerRecordsService.LastFrameTimeInSec;

            if (IsHiccup(lastFrameTimeInSec))
            {
                HiccupsCountInBuffer -= 1;
                HiccupsSum -= lastFrameTimeInSec;
            }

            if (IsHiccup(valueInSeconds))
            {
                HiccupsCountInBuffer += 1;
                HiccupsSum += valueInSeconds;
            }

            TotalSeconds -= lastFrameTimeInSec;
            TotalSeconds += valueInSeconds;
        }

        private static bool IsHiccup(float value) =>
            value > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;
    }
}
