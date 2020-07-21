using UnityEngine;
using UnityEngine.UI;

namespace DCL.FPSDisplay
{

    public class FPSDisplay : MonoBehaviour
    {
        public Text label;
        private readonly LinealBufferHiccupCounter counter = new LinealBufferHiccupCounter();

        void Update()
        {
            if (label == null)
                return;

            float dt = Time.unscaledDeltaTime;

            counter.AddDeltaTime(dt);
            float fps = counter.CurrentFPSCount();

            string fpsFormatted = fps.ToString("##");
            string msFormatted = (dt * 1000).ToString("##");

            string targetText = string.Empty;

            string NO_DECIMALS = "##";
            string TWO_DECIMALS = "##.00";
            targetText += $"Hiccups in the last 1000 frames: {counter.CurrentHiccupCount()}\n";
            targetText += $"Hiccup loss: {(100.0f * counter.GetHiccupSum() / counter.GetTotalSeconds()).ToString(TWO_DECIMALS)}% ({counter.GetHiccupSum().ToString(TWO_DECIMALS)} in {counter.GetTotalSeconds().ToString(TWO_DECIMALS)} secs)\n";
            targetText += $"Bad Frames Percentile: {((counter.CurrentHiccupCount()) / 10.0f).ToString(NO_DECIMALS)}%\n";
            targetText += $"Current {msFormatted} ms (fps: {fpsFormatted})";

            if (label.text != targetText)
            {
                label.text = targetText;
            }

            FPSColoring.DisplayColor(label, fps);
        }
    }
}