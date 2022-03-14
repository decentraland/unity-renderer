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
            config.useFog = EditorGUILayout.Toggle(SkyboxEditorLiterals.useFog, config.useFog);
            if (config.useFog)
            {
                RenderSimpleValues.RenderColorGradientField(config.fogColor, SkyboxEditorLiterals.fogColor, 0, 24);
                config.fogMode = (FogMode)EditorGUILayout.EnumPopup(SkyboxEditorLiterals.fogMode, config.fogMode);

                switch (config.fogMode)
                {
                    case FogMode.Linear:
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.startDistance, ref config.fogStartDistance);
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.endDistance, ref config.fogEndDistance);
                        break;
                    default:
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.density, ref config.fogDensity);
                        break;
                }
            }

        }
    }
}