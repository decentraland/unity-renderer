using UnityEngine;
using UnityEngine.UI;

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

    public string[] labelText =
    {
        "Worst:",
        "99th: ",
        "95th: ",
        "90th: ",
        "75th: ",
        "50th: "
    };

    public Text[] labels;

    public FPSColor[] coloring = {
        new FPSColor(Color.black, 40),
        new FPSColor(Color.yellow, 30),
        new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.3f), 20),
        new FPSColor(Color.Lerp(Color.yellow, Color.red, 0.7f), 10),
        new FPSColor(Color.red, 0)
    };

    public FrameTimeCounter fpsCounter;

    void Update()
    {
        if (!fpsCounter)
        {
            fpsCounter = GetComponent<FrameTimeCounter>();
            if (!fpsCounter)
            {
                return;
            }
        }
        for (int i = 0; i < labels.Length; i++)
        {
            Display(labels[i], labelText[i], fpsCounter.stats[i]);
        }
    }

    void Display(Text label, string text, float millis)
    {
        if (millis == 0)
        {
            return;
        }
        float fps = 1 / millis;
        string targetText = text + (millis * 1000).ToString("##.000") + " ms";
        if (targetText!= label.text)
        {
            label.text = targetText;
        }
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
