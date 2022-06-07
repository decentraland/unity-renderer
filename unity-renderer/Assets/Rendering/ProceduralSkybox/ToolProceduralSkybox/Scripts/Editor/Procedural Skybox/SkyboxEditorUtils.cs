using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public enum SkyboxEditorToolsParts
    {
        Timeline_Tags,
        BG_Layer,
        Horizon_Plane,
        Ambient_Layer,
        Avatar_Layer,
        Fog_Layer,
        Directional_Light_Layer,
        Base_Skybox,
        Elements3D_Dome,
        Elements3D_Satellite,
        Elements3D_Planar
    }

    public class RightPanelPins
    {
        public string name;
        public SkyboxEditorToolsParts part;
        public TextureLayer baseSkyboxTargetLayer = null;
        public Config3DDome targetDomeElement;
        public Config3DSatellite targetSatelliteElement;
        public Config3DPlanar targetPlanarElement;
        public bool pinned;
        public Vector2 scroll;
    }

    public static class SkyboxEditorUtils
    {

        public static float GetNormalizedLayerCurrentTime(float timeOfTheDay, float startTime, float endTime)
        {
            float editedEndTime = endTime;
            float editedDayTime = timeOfTheDay;
            if (endTime < startTime)
            {
                editedEndTime = SkyboxUtils.CYCLE_TIME + endTime;
                if (timeOfTheDay < startTime)
                {
                    editedDayTime = SkyboxUtils.CYCLE_TIME + timeOfTheDay;
                }
            }
            return Mathf.InverseLerp(startTime, editedEndTime, editedDayTime);
        }

        public static float GetDayTimeForLayerNormalizedTime(float startTime, float endTime, float normalizeTime)
        {
            float editedEndTime = endTime;
            if (endTime < startTime)
            {
                editedEndTime = SkyboxUtils.CYCLE_TIME + endTime;
            }
            float time = Mathf.Lerp(startTime, editedEndTime, normalizeTime);

            if (time > SkyboxUtils.CYCLE_TIME)
            {
                time -= SkyboxUtils.CYCLE_TIME;
            }

            return time;
        }

        public static void ClampToDayTime(ref float value) { value = Mathf.Clamp(value, 0, SkyboxUtils.CYCLE_TIME); }
    }
}