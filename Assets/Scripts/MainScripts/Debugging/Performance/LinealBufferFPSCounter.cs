namespace DCL.FPSDisplay
{
    public class LinealBufferFPSCounter
    {
        public const int MAX_BUFFER_SIZE = 1000;
        public readonly float[] values = new float[MAX_BUFFER_SIZE];

        public int begin { get; private set; } = 0;
        public int end { get; private set; } = 0;
        public float secondsBetweenBeginAndEnd { get; private set; } = 0;
        public float secondsInBuffer { get; private set; } = 0;
        public int countBetweenBeginAndEnd { get; private set; } = 0;

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
        public float CurrentFPSCount()
        {
            return countBetweenBeginAndEnd;
        }
        public float GetTotalSeconds()
        {
            return secondsInBuffer;
        }
    }
}