using UnityEngine;

namespace DCL.FPSDisplay
{
    public class FPSColoring
    {
        private static readonly Color GREAT_COLOR = Color.green;
        private static readonly Color GOOD_COLOR = Color.white;
        private static readonly Color BAD_COLOR = Color.yellow;
        private static readonly Color WORSE_COLOR = new Color(0.9764706f, 0.4117647f, 0.05490196f);
        private static readonly Color UNBEARABLE_COLOR = Color.red;

        public static FPSColor[] coloring =
        {
            new FPSColor(GREAT_COLOR, FPSEvaluation.GREAT),
            new FPSColor(GOOD_COLOR, FPSEvaluation.GOOD),
            new FPSColor(BAD_COLOR, FPSEvaluation.BAD),
            new FPSColor(WORSE_COLOR, FPSEvaluation.WORSE),
            new FPSColor(UNBEARABLE_COLOR, FPSEvaluation.UNBEARABLE),
        };

        private static Color GeFPSDisplayColor(float fps)
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

        private static Color GetPercentageColoring(float value, float limit)
        {
            float currentPercentage = value / limit;
            if (currentPercentage > 0.99)
            {
                return UNBEARABLE_COLOR;
            }
            if (currentPercentage > 0.9)
            {
                return BAD_COLOR;
            }
            return GOOD_COLOR;
        }

        private static Color GetMemoryColoring(float memoryValue)
        {
            if (memoryValue > 1800)
            {
                return UNBEARABLE_COLOR;
            }
            if (memoryValue > 1500)
            {
                return BAD_COLOR;
            }
            return GOOD_COLOR;
        }

        public static string GetDisplayColorString(float fps) { return GetColor(GetHexColor(GeFPSDisplayColor(fps))); }

        public static string GetPercentageColoringString(int value, int limit) { return GetColor(GetHexColor(GetPercentageColoring(value, limit))); }

        private static string GetHexColor(Color color) { return $"#{ColorUtility.ToHtmlStringRGB(color)}"; }

        private static string GetColor(string color) { return $"<color={color}>"; }

        public static object GetMemoryColoringString(float memoryValue) { return GetColor(GetHexColor(GetMemoryColoring(memoryValue))); }
    }
}