using DCL.FPSDisplay;
using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace FPSDisplayTests
{
    public class HiccupTests
    {
        [Test]
        public void DetectsOneHiccup()
        {
            LinealBufferHiccupCounter counter = new LinealBufferHiccupCounter(new ProfilerRecordsService());

            const float tenMillis = 0.01f;

            for (int i = 0; i < 100; i++)
            {
                counter.AddDeltaTime(tenMillis);
            }

            counter.AddDeltaTime(FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS + 0.01f);

            Assert.AreEqual(counter.HiccupsCountInBuffer, 1);
        }

        [Test]
        public void CorrectlyAccountsTimeInHiccups()
        {
            LinealBufferHiccupCounter counter = new LinealBufferHiccupCounter(new ProfilerRecordsService());

            float hiccups = 0.0f;
            int hiccupCount = 0;

            for (int i = 0; i < 1000; i++)
            {
                float value = Random.Range(0.001f, 1f);
                counter.AddDeltaTime(value);
                if (value > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS)
                {
                    hiccups += value;
                    hiccupCount += 1;
                }
            }

            Assert.AreEqual(counter.HiccupsSum, hiccups);
            Assert.AreEqual(counter.HiccupsCountInBuffer, hiccupCount);
        }

        [Test]
        public void GetSumOfHiccupsAndTime()
        {
            LinealBufferHiccupCounter counter = new LinealBufferHiccupCounter(new ProfilerRecordsService());

            float hiccups = 0.0f;
            float totalTime = 0.0f;
            int hiccupCount = 0;

            for (int i = 0; i < 1000; i++)
            {
                float value = Random.Range(0.001f, 1f);

                if (value > FPSEvaluation.HICCUP_THRESHOLD_IN_SECONDS)
                {
                    hiccupCount += 1;
                    hiccups += value;
                }

                totalTime += value;
                counter.AddDeltaTime(value);
            }

            const float eps = 0.002f;
            Assert.LessOrEqual(Mathf.Abs(counter.HiccupsSum - hiccups), eps);
            Assert.AreEqual(counter.HiccupsCountInBuffer, hiccupCount);
            Assert.LessOrEqual(Mathf.Abs(counter.TotalSeconds - totalTime), eps);
        }
    }
}
