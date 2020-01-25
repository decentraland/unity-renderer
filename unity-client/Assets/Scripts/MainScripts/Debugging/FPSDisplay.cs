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

        void Update()
        {
            if (label != null)
            {
                deltas[end++] = Time.deltaTime;
                if (end == 1000) end = 0;
                countBetween += Time.deltaTime;
                while (countBetween > 1.0f)
                {
                    countBetween -= deltas[begin++];
                    if (begin == 1000) begin = 0;
                }
                float fps = end > begin ? end - begin : 1000 + end - begin;
                string fpsFormatted = fps.ToString("##");
                string msFormatted = (Time.deltaTime * 1000).ToString("##");
                string targetText = $"Current {msFormatted} ms (fps: {fpsFormatted})";

                if (label.text != targetText)
                {
                    label.text = targetText;
                }

                DisplayColor(label, fps);
            }
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
