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
            config.ambientTrilight = EditorGUILayout.Toggle("Use Gradient", config.ambientTrilight);

            if (config.ambientTrilight)
            {
                RenderSimpleValues.RenderColorGradientField(config.ambientSkyColor, "Ambient Sky Color", 0, 24, true);
                RenderSimpleValues.RenderColorGradientField(config.ambientEquatorColor, "Ambient Equator Color", 0, 24, true);
                RenderSimpleValues.RenderColorGradientField(config.ambientGroundColor, "Ambient Ground Color", 0, 24, true);
            }

        }
    }
}