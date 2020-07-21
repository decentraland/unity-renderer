using UnityEngine;

namespace DCL.FPSDisplay
{
    public class LinealBufferHiccupCounter
    {
        private readonly LinealBufferFPSCounter counter = new LinealBufferFPSCounter();
        public const float HICCUP_THRESHOLD_IN_SECONDS = 1 / (float)FPSEvaluation.BAD;
        private int hiccupsCountInBuffer = 0;
        private float hiccupsSum = 0.0f;
        private float bufferCount = 0.0f;

        public int HiccupsCountInBuffer { get => hiccupsCountInBuffer; private set => hiccupsCountInBuffer = value; }
        public float HiccupsSum { get => hiccupsSum; set => hiccupsSum = value; }

        public float CurrentFPSCount()
        {
            return counter.CurrentFPSCount();
        }

        public float GetTotalSeconds()
        {
            return bufferCount;
        }

        public void AddDeltaTime(float valueInSeconds)
        {
            if (IsHiccup(counter.values[counter.end]))
            {
                hiccupsCountInBuffer -= 1;
                hiccupsSum -= counter.values[counter.end];
            }
            if (IsHiccup(valueInSeconds))
            {
                hiccupsCountInBuffer += 1;
                hiccupsSum += valueInSeconds;
            }
            bufferCount -= counter.values[counter.end];
            bufferCount += valueInSeconds;

            counter.AddDeltaTime(valueInSeconds);
        }

        public int CurrentHiccupCount()
        {
            return hiccupsCountInBuffer;
        }

        private bool IsHiccup(float value)
        {
            return value > HICCUP_THRESHOLD_IN_SECONDS;
        }

        public float GetHiccupSum()
        {
            return hiccupsSum;
        }
    }
}