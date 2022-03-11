using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderFogLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {
            config.useFog = EditorGUILayout.Toggle("Use Fog", config.useFog);
            if (config.useFog)
            {
                RenderSimpleValues.RenderColorGradientField(config.fogColor, "Fog Color", 0, 24);
                config.fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", config.fogMode);

                switch (config.fogMode)
                {
                    case FogMode.Linear:
                        RenderSimpleValues.RenderFloatField("Start Distance", ref config.fogStartDistance);
                        RenderSimpleValues.RenderFloatField("End Distance", ref config.fogEndDistance);
                        break;
                    default:
                        RenderSimpleValues.RenderFloatField("Density", ref config.fogDensity);
                        break;
                }
            }

        }
    }
}