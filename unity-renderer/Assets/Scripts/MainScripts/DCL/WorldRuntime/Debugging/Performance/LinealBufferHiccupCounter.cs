namespace DCL.FPSDisplay
{
    public class LinealBufferHiccupCounter
    {

        private readonly LinealBufferFPSCounter counter = new ();

        public int HiccupsCountInBuffer { get; private set; }
        public float HiccupsSum { get; private set; }
        public float TotalSeconds { get; private set; }

        public float CurrentFPSCount => counter.CurrentFPSCount();

        public void AddDeltaTime(float valueInSeconds)
        {
            if (IsHiccup(counter.LastValue))
            {
                HiccupsCountInBuffer -= 1;
                HiccupsSum -= counter.LastValue;
            }

            if (IsHiccup(valueInSeconds))
            {
                HiccupsCountInBuffer += 1;
                HiccupsSum += valueInSeconds;
            }

            TotalSeconds -= counter.LastValue;
            TotalSeconds += valueInSeconds;

            counter.AddDeltaTime(valueInSeconds);
        }

        private static bool IsHiccup(float value) =>
            value > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;
    }
}
