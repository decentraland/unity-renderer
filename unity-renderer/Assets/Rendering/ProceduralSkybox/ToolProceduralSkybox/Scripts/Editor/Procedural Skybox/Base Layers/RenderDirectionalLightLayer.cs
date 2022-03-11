using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class DirectionalLightLayer
    {
        private static Light directionalLight;
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Light directionalLightRef)
        {
            directionalLight = directionalLightRef;

            config.useDirectionalLight = EditorGUILayout.Toggle("Use Directional Light", config.useDirectionalLight);

            if (!config.useDirectionalLight)
            {
                return;
            }
            RenderSimpleValues.RenderColorGradientField(config.directionalLightLayer.lightColor, "Light Color", 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.directionalLightLayer.tintColor, "Tint Color", 0, 24, true);

            GUILayout.Space(10);

            // Light Intesity
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, config.directionalLightLayer.intensity, "Light Intensity", "%", "Intensity");

            GUILayout.Space(30);

            RenderTransitioningVariables.RenderTransitioningQuaternionAsVector3(ref timeOfTheDay, config.directionalLightLayer.lightDirection, "Light Direction", "%", "Direction", GetDLDirection, 0, 24);
        }

        private static Quaternion GetDLDirection() { return directionalLight.transform.rotation; }
    }
}