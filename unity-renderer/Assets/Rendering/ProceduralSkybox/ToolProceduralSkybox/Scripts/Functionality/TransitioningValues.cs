using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public static class TransitioningValues
    {
        public static float GetTransitionValue(List<TransitioningFloat> list, float percentage, float defaultVal = 0)
        {
            if (list == null || list.Count < 1)
            {
                return defaultVal;
            }

            if (list.Count == 1)
            {
                return list[0].value;
            }

            TransitioningFloat min = list[0], max = list[0];


            for (int i = 0; i < list.Count; i++)
            {
                if (percentage <= list[i].percentage)
                {
                    max = list[i];

                    if ((i - 1) > 0)
                    {
                        min = list[i - 1];
                    }

                    break;
                }
            }

            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            return Mathf.Lerp(min.value, max.value, t);
        }

        public static Vector2 GetTransitionValue(List<TransitioningVector2> list, float percentage, Vector2 defaultVal = default(Vector2))
        {
            Vector2 offset = defaultVal;

            if (list == null || list.Count == 0)
            {
                return offset;
            }

            if (list.Count == 1)
            {
                offset = list[0].value;
                return offset;
            }


            TransitioningVector2 min = list[0], max = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                if (percentage <= list[i].percentage)
                {
                    max = list[i];

                    if ((i - 1) > 0)
                    {
                        min = list[i - 1];
                    }

                    break;
                }
            }
            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            offset = Vector2.Lerp(min.value, max.value, t);

            return offset;
        }

        public static Vector3 GetTransitionValue(List<TransitioningVector3> list, float percentage)
        {
            Vector3 offset = new Vector3(0, 0, 0);

            if (list == null || list.Count == 0)
            {
                return offset;
            }

            if (list.Count == 1)
            {
                offset = list[0].value;
                return offset;
            }


            TransitioningVector3 min = list[0], max = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                if (percentage <= list[i].percentage)
                {
                    max = list[i];

                    if ((i - 1) > 0)
                    {
                        min = list[i - 1];
                    }

                    break;
                }
            }

            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            offset = Vector3.Lerp(min.value, max.value, t);

            return offset;
        }
    }
}