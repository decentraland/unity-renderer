using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    [RequireComponent(typeof(FrameTimeCounter))]
    public class FPSDisplay : MonoBehaviour
    {
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

        string[] labelText =
        {
            "Current: ",
            "50th: ",
            "75th: ",
            "90th: ",
            "95th: ",
            "99th: ",
        };

        public Text[] labels;

        FPSColor[] coloring =
        {
            new FPSColor(Color.white, 40),
            new FPSColor(Color.yellow, 30),
            new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.3f), 20),
            new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.7f), 10),
            new FPSColor(Color.red, 0),
        };

        public FrameTimeCounter fpsCounter;

        private void Start()
        {
            InvokeRepeating("LazyUpdate", 5.0f, 0.15f);
        }

        void LazyUpdate()
        {
            if (!fpsCounter)
            {
                fpsCounter = GetComponent<FrameTimeCounter>();

                if (!fpsCounter)
                {
                    return;
                }
            }


            if (labels.Length > 0)
            {
                float fps = 1.0f / Time.deltaTime;
                string targetText = labelText[0] + (Time.deltaTime * 1000).ToString("##.000") + "ms (fps: " + fps + ")";

                if (labels[0].text != targetText)
                {
                    labels[0].text = targetText;
                }

                DisplayColor(labels[0], fps);

                for (int i = 1; i < labels.Length; i++)
                {
                    DisplayText(labels[i], labelText[i], fpsCounter.stats[i]);
                    DisplayColor(labels[i], 1 / fpsCounter.stats[i]);
                }
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

        void DisplayText(Text label, string text, float value)
        {
            if (value == 0)
            {
                return;
            }

            string targetText = text + (value * 1000).ToString("##.000") + "ms";

            if (targetText != label.text)
            {
                label.text = targetText;
            }
        }
    }
}