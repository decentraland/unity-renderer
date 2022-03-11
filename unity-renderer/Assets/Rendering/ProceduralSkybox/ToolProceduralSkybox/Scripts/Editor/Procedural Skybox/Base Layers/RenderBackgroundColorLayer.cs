using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderBackgroundColorLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {
            RenderSimpleValues.RenderColorGradientField(config.skyColor, "Sky Color", 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.horizonColor, "Horizon Color", 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.groundColor, "Ground Color", 0, 24);
            RenderHorizonLayer(ref timeOfTheDay, toolSize, config);
        }

        static void RenderHorizonLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration selectedConfiguration)
        {
            EditorGUILayout.Separator();
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonHeight, "Horizon Height", "%", "value", true, -1, 1);

            EditorGUILayout.Space(10);
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonWidth, "Horizon Width", "%", "value", true, -1, 1);

            EditorGUILayout.Separator();

            // Horizon Mask
            RenderSimpleValues.RenderTexture("Texture", ref selectedConfiguration.horizonMask);

            // Horizon mask values
            RenderSimpleValues.RenderVector3Field("Horizon Mask Values", ref selectedConfiguration.horizonMaskValues);

            // Horizon Plane color
            RenderSimpleValues.RenderColorGradientField(selectedConfiguration.horizonPlaneColor, "Horizon Plane Color", 0, 24);

            // Horizon Height
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonPlaneHeight, "Horizon Plane Height", "%", "value", true, -1, 0);
        }
    }
}