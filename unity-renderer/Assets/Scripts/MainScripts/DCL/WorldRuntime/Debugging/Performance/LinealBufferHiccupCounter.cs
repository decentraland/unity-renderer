namespace DCL.FPSDisplay
{
    public class LinealBufferHiccupCounter
    {
        private readonly LinealBufferFPSCounter counter;

        public int HiccupsCountInBuffer { get; private set; }
        public float HiccupsSum { get; private set; }
        public float TotalSeconds { get; private set; }

        public LinealBufferHiccupCounter(int bufferSize)
        {
            counter = new LinealBufferFPSCounter(bufferSize);
        }

        public void AddDeltaTime(float valueInSeconds)
        {
            if (IsHiccup(counter.Values[counter.Tail]))
            {
                HiccupsCountInBuffer -= 1;
                HiccupsSum -= counter.Values[counter.Tail];
            }

            if (IsHiccup(valueInSeconds))
            {
                HiccupsCountInBuffer += 1;
                HiccupsSum += valueInSeconds;
            }

            TotalSeconds -= counter.Values[counter.Tail];
            TotalSeconds += valueInSeconds;

            counter.AddDeltaTime(valueInSeconds);
        }

        public static bool IsHiccup(float value) =>
            value > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS;

        private class LinealBufferFPSCounter
        {
            public readonly float[] Values;
            private readonly int maxBufferSize;

            private int head;
            private float secondsBetweenHeadAndTail;

            public int Tail { get; private set; }

            public LinealBufferFPSCounter(int bufferSize)
            {
                Values = new float[bufferSize];
                maxBufferSize = bufferSize;
            }

            public void AddDeltaTime(float valueInSeconds)
            {
                Values[Tail] = valueInSeconds;

                Tail = CircularIncrement(Tail);

                AdjustHeadPosition(valueInSeconds);
            }

            private void AdjustHeadPosition(float valueInSeconds)
            {
                secondsBetweenHeadAndTail += valueInSeconds;
                while (secondsBetweenHeadAndTail > 1)
                {
                    secondsBetweenHeadAndTail -= Values[head];
                    head = CircularIncrement(head);
                }
            }

            private int CircularIncrement(int id) =>
                id == maxBufferSize - 1 ? 0 : id + 1;
        }
    }
}
