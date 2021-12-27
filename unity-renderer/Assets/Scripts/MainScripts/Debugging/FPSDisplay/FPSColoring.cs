using UnityEngine;

namespace DCL.FPSDisplay
{
    public class FPSColoring
    {
        public static FPSColor[] coloring =
        {
            new FPSColor(Color.green, FPSEvaluation.GREAT),
            new FPSColor(Color.white, FPSEvaluation.GOOD),
            new FPSColor(Color.yellow, FPSEvaluation.BAD),
            new FPSColor(new Color(0.9764706f, 0.4117647f, 0.05490196f), FPSEvaluation.WORSE),
            new FPSColor(Color.red, FPSEvaluation.UNBEARABLE),
        };

        public static Color GetDisplayColor(float fps)
        {
            for (int i = 0; i < coloring.Length; i++)
            {
                if (fps >= coloring[i].fps)
                {
                    return coloring[i].color;
                }
            }

            return coloring[coloring.Length - 1].color;
        }
    }
}