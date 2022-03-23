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
            RenderSimpleValues.RenderColorGradientField(config.skyColor, SkyboxEditorLiterals.skyColor, 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.horizonColor, SkyboxEditorLiterals.horizonColor, 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.groundColor, SkyboxEditorLiterals.groundColor, 0, 24);
            RenderHorizonLayer(ref timeOfTheDay, toolSize, config);
        }

        static void RenderHorizonLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration selectedConfiguration)
        {
            EditorGUILayout.Separator();
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonHeight, SkyboxEditorLiterals.horizonHeight, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, true, -1, 1);

            EditorGUILayout.Space(10);
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonWidth, SkyboxEditorLiterals.horizonWidth, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, true, -1, 1);

            EditorGUILayout.Separator();

            // Horizon Mask
            RenderSimpleValues.RenderTexture(SkyboxEditorLiterals.texture, ref selectedConfiguration.horizonMask);

            // Horizon mask values
            RenderSimpleValues.RenderVector3Field(SkyboxEditorLiterals.horizonMaskValues, ref selectedConfiguration.horizonMaskValues);

            // Horizon Plane color
            RenderSimpleValues.RenderColorGradientField(selectedConfiguration.horizonPlaneColor, SkyboxEditorLiterals.horizonPlaneColor, 0, 24);

            // Horizon Height
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonPlaneHeight, SkyboxEditorLiterals.horizonPlaneHeight, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, true, -1, 0);
        }
    }
}