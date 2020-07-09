using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class FPSDisplay : MonoBehaviour
    {
        private readonly float[] deltas = new float[1000];
        private int begin = 0;
        private int end = 0;
        private float countBetween = 0;

        [System.Serializable]
        public struct FPSColor
        {
            public Color color;
            public int fps;

            public FPSColor(Color col, int fps) : this()
            {
                this.color = col;
                this.fps = fps;
            }
        }

        public Text label;

        FPSColor[] coloring =
        {
            new FPSColor(Color.white, 40),
            new FPSColor(Color.yellow, 30),
            new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.3f), 20),
            new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.7f), 10),
            new FPSColor(Color.red, 0),
        };

        private double totalHiccupCount;
        private double hiccupsPerMinute;
        private double hiccupsPerMinuteMax;

        private double fastFramesCount;
        private double totalFramesCount;

        private double avgHiccupPeak;

        private const double HICCUP_THRESHOLD = 0.034; //NOTE(Brian): A bit over 30 FPS

        void Update()
        {
            if (label == null)
                return;

            float dt = Time.unscaledDeltaTime;

            if (dt > HICCUP_THRESHOLD)
            {
                totalHiccupCount++;
                avgHiccupPeak += dt;
            }

            hiccupsPerMinute = totalHiccupCount / (Time.realtimeSinceStartup / 60.0f);
            hiccupsPerMinuteMax = Math.Max(hiccupsPerMinuteMax, hiccupsPerMinute);

            deltas[end++] = dt;
            if (end == 1000) end = 0;
            countBetween += dt;

            while (countBetween > 1.0f)
            {
                countBetween -= deltas[begin++];
                if (begin == 1000) begin = 0;
            }

            float fps = end > begin ? end - begin : 1000 + end - begin;

            if (dt < 0.033f)
            {
                fastFramesCount++;
            }

            totalFramesCount++;

            string fpsFormatted = fps.ToString("##");
            string msFormatted = (dt * 1000).ToString("##");

            string hpsMaxFormatted = hiccupsPerMinuteMax.ToString("##.00", NumberFormatInfo.InvariantInfo);
            string fps30percentileFormatted = ((fastFramesCount / totalFramesCount) * 100).ToString("##");
            string avgHiccupPeakFormatted = ((avgHiccupPeak / totalHiccupCount) * 1000).ToString("##");

            string targetText = string.Empty;

            targetText += $"Avg hiccup ms: {avgHiccupPeakFormatted} ms\n";
            targetText += $"Max hiccup count p/m: {hpsMaxFormatted}\n";
            targetText += $"30 fps percentile: {fps30percentileFormatted}%\n";
            targetText += $"Current {msFormatted} ms (fps: {fpsFormatted})";

            if (label.text != targetText)
            {
                label.text = targetText;
            }

            DisplayColor(label, fps);
        }

        void DisplayColor(Text label, float fps)
        {
            for (int i = 0; i < coloring.Length; i++)
            {
                if (fps >= coloring[i].fps)
                {
                    if (label.color != coloring[i].color)
                    {
                        label.color = coloring[i].color;
                    }

                    break;
                }
            }
        }
    }
}