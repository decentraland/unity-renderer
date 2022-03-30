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

            config.useDirectionalLight = EditorGUILayout.Toggle(SkyboxEditorLiterals.useDirecnLight, config.useDirectionalLight);

            if (!config.useDirectionalLight)
            {
                return;
            }
            RenderSimpleValues.RenderColorGradientField(config.directionalLightLayer.lightColor, SkyboxEditorLiterals.lightColor, 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.directionalLightLayer.tintColor, SkyboxEditorLiterals.tint, 0, 24, true);

            GUILayout.Space(10);

            // Light Intesity
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, config.directionalLightLayer.intensity, SkyboxEditorLiterals.lightIntensity, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.intensity);

            GUILayout.Space(30);

            RenderTransitioningVariables.RenderTransitioningQuaternionAsVector3(ref timeOfTheDay, config.directionalLightLayer.lightDirection, SkyboxEditorLiterals.lightDirection, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.direction, GetDLDirection, 0, 24);
        }

        private static Quaternion GetDLDirection() { return directionalLight.transform.rotation; }
    }
}