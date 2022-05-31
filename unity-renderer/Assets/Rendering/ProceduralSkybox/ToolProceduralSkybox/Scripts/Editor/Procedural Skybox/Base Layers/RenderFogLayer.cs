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
            config.useFog = EditorGUILayout.Toggle(SkyboxEditorLiterals.Labels.useFog, config.useFog);
            if (config.useFog)
            {
                RenderSimpleValues.RenderColorGradientField(config.fogColor, SkyboxEditorLiterals.LayerProperties.fogColor, 0, 24);
                config.fogMode = (FogMode)EditorGUILayout.EnumPopup(SkyboxEditorLiterals.LayerProperties.fogMode, config.fogMode);

                switch (config.fogMode)
                {
                    case FogMode.Linear:
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.startDistance, ref config.fogStartDistance);
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.endDistance, ref config.fogEndDistance);
                        break;
                    default:
                        RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.density, ref config.fogDensity);
                        break;
                }
            }

        }
    }
}