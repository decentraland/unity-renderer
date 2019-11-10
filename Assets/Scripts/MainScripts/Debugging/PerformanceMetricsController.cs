using System.Text;
using DCL.Interface;
using UnityEngine;

namespace DCL
{
    public class PerformanceMetricsController : MonoBehaviour
    {
        private const int SAMPLES_SIZE = 1000;
        private char[] encodedSamples = new char[SAMPLES_SIZE];
        private int currentIndex = 0;

        private void Update()
        {
            var deltaInMs = Time.deltaTime * 1000;
            encodedSamples[currentIndex++] = (char)deltaInMs;
            if (currentIndex == SAMPLES_SIZE)
            {
                currentIndex = 0;
                Report(new string(encodedSamples));
            }
        }

        private void Report(string encodedSamples)
        {
            WebInterface.SendPerformanceReport(encodedSamples);
        }
    }
}