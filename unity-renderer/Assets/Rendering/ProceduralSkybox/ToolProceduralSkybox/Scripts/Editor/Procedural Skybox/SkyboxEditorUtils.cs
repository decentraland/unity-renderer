using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public enum SkyboxEditorToolsParts
    {
        Timeline_Tags,
        BG_Layer,
        Ambient_Layer,
        Avatar_Layer,
        Fog_Layer,
        Directional_Light_Layer,
        Base_Skybox,
        Elements3D_Dome
    }

    public class RightPanelPins
    {
        public string name;
        public SkyboxEditorToolsParts part;
        public TextureLayer baseSkyboxTargetLayer = null;
        public Config3DDome targetDomeElement;
        public bool pinned;
        public Vector2 scroll;
    }

    public static class SkyboxEditorUtils
    {
        public static float GetNormalizedDayTime(float timeOfTheDay)
        {
            float tTime = timeOfTheDay / 24;
            tTime = Mathf.Clamp(tTime, 0, 1);
            return tTime;
        }

        public static float GetNormalizedLayerCurrentTime(float timeOfTheDay, float startTime, float endTime)
        {
            float editedEndTime = endTime;
            float editedDayTime = timeOfTheDay;
            if (endTime < startTime)
            {
                editedEndTime = 24 + endTime;
                if (timeOfTheDay < startTime)
                {
                    editedDayTime = 24 + timeOfTheDay;
                }
            }
            return Mathf.InverseLerp(startTime, editedEndTime, editedDayTime);
        }

        public static float GetDayTimeForLayerNormalizedTime(float startTime, float endTime, float normalizeTime)
        {
            float editedEndTime = endTime;
            if (endTime < startTime)
            {
                editedEndTime = 24 + endTime;
            }
            float time = Mathf.Lerp(startTime, editedEndTime, normalizeTime);

            if (time > 24)
            {
                time -= 24;
            }

            return time;
        }

        public static void ClampToDayTime(ref float value) { value = Mathf.Clamp(value, 0, 24); }
    }
}