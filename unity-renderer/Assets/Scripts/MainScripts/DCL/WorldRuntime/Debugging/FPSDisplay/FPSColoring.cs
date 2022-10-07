﻿using UnityEngine;

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

        public static Color GetPercentageColoring(float value, float limit)
        {
            float currentPercentage = value / limit;
            if (currentPercentage > 90)
            {
                return BAD_COLOR;
            }
            if (currentPercentage > 100)
            {
                return UNBEARABLE_COLOR;
            }
            return GOOD_COLOR;
        }
        
    }
}