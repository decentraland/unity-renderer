namespace DCL.FPSDisplay
{
    public class LinealBufferFPSCounter
    {
        private const int MAX_BUFFER_SIZE = 1000;

        private readonly float[] values = new float[MAX_BUFFER_SIZE];

        public float LastValue => values[end];

        private int begin;
        private int end;

        private float secondsBetweenBeginAndEnd;
        private float secondsInBuffer;
        private int countBetweenBeginAndEnd;

        public void AddDeltaTime(float valueInSeconds)
        {
            values[end++] = valueInSeconds;
            secondsInBuffer += valueInSeconds;
            secondsBetweenBeginAndEnd += valueInSeconds;
            countBetweenBeginAndEnd++;

            if (end == MAX_BUFFER_SIZE)
                end = 0;

            while (secondsBetweenBeginAndEnd > 1)
            {
                secondsInBuffer -= values[begin];
                secondsBetweenBeginAndEnd -= values[begin++];
                countBetweenBeginAndEnd--;
                if (begin == MAX_BUFFER_SIZE)
                    begin = 0;
            }
        }

        public float CurrentFPSCount() => countBetweenBeginAndEnd;
        public float GetTotalSeconds() => secondsInBuffer;
    }
}
