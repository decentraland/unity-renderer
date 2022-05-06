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
            EditorGUILayout.LabelField("Horizon Mask", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            RenderSimpleValues.RenderTexture("Texture", ref selectedConfiguration.horizonMask);

            // Horizon Mask values
            EditorGUILayout.LabelField("Horizon Mask Values", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            // Tiling
            RenderSimpleValues.RenderVector2Field("Tiling", ref selectedConfiguration.horizonMaskTiling);
            // Offset
            RenderSimpleValues.RenderVector2Field("Offset", ref selectedConfiguration.horizonMaskOffset);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }
    }
}