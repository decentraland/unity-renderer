using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.FPSDisplay
{

    public class FPSDisplay : MonoBehaviour
    {
        private const float REFRESH_SECONDS = 0.1f;

        [SerializeField] private Text label;
        [SerializeField] private PerformanceMetricsDataVariable performanceData;

        private void OnEnable()
        {
            if (label == null || performanceData == null)
                return;

            StartCoroutine(UpdateLabelLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator UpdateLabelLoop()
        {
            while (true)
            {
                UpdateLabel();

                yield return new WaitForSeconds(REFRESH_SECONDS);
            }
        }

        private void UpdateLabel()
        {
            float dt = Time.unscaledDeltaTime;

            float fps = performanceData.Get().fpsCount;

            string fpsFormatted = fps.ToString("##");
            string msFormatted = (dt * 1000).ToString("##");

            string targetText = string.Empty;


            string NO_DECIMALS = "##";
            string TWO_DECIMALS = "##.00";
            targetText += $"Hiccups in the last 1000 frames: {performanceData.Get().hiccupCount}\n";
            targetText += $"Hiccup loss: {(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% ({performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs)\n";
            targetText += $"Bad Frames Percentile: {((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%\n";
            targetText += $"Current {msFormatted} ms (fps: {fpsFormatted})";

            if (label.text != targetText)
            {
                label.text = targetText;
            }

            FPSColoring.DisplayColor(label, fps);
        }
    }
}