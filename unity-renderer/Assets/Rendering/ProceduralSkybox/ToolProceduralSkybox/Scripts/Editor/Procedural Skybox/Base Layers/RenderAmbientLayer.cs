using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderAmbientLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {
            config.ambientTrilight = EditorGUILayout.Toggle(SkyboxEditorLiterals.useGradient, config.ambientTrilight);

            if (config.ambientTrilight)
            {
                RenderSimpleValues.RenderColorGradientField(config.ambientSkyColor, SkyboxEditorLiterals.ambientSkyColor, 0, 24, true);
                RenderSimpleValues.RenderColorGradientField(config.ambientEquatorColor, SkyboxEditorLiterals.ambientEquatorColor, 0, 24, true);
                RenderSimpleValues.RenderColorGradientField(config.ambientGroundColor, SkyboxEditorLiterals.ambientGroundColor, 0, 24, true);
            }

        }
    }
}