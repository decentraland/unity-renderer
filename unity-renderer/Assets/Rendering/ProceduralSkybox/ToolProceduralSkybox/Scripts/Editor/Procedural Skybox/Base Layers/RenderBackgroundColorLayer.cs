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
            RenderSimpleValues.RenderColorGradientField(config.skyColor, SkyboxEditorLiterals.LayerProperties.skyColor, 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.horizonColor, SkyboxEditorLiterals.LayerProperties.horizonColor, 0, 24);
            RenderSimpleValues.RenderColorGradientField(config.groundColor, SkyboxEditorLiterals.LayerProperties.groundColor, 0, 24);
            RenderHorizonLayer(ref timeOfTheDay, toolSize, config);
        }

        static void RenderHorizonLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration selectedConfiguration)
        {
            EditorGUILayout.Separator();
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonHeight, SkyboxEditorLiterals.LayerProperties.horizonHeight, SkyboxEditorLiterals.LayerProperties.percentage, SkyboxEditorLiterals.LayerProperties.value, true, -1, 1);

            EditorGUILayout.Space(10);
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, selectedConfiguration.horizonWidth, SkyboxEditorLiterals.LayerProperties.horizonWidth, SkyboxEditorLiterals.LayerProperties.percentage, SkyboxEditorLiterals.LayerProperties.value, true, -1, 1);

            EditorGUILayout.Separator();

            // Horizon Mask
            EditorGUILayout.LabelField(SkyboxEditorLiterals.LayerProperties.horizonMask, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            RenderSimpleValues.RenderTexture(SkyboxEditorLiterals.LayerProperties.texture, ref selectedConfiguration.horizonMask);

            // Horizon Mask values
            EditorGUILayout.LabelField(SkyboxEditorLiterals.LayerProperties.horizonMaskValues, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            // Tiling
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.LayerProperties.tiling, ref selectedConfiguration.horizonMaskTiling);
            // Offset
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.LayerProperties.offset, ref selectedConfiguration.horizonMaskOffset);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }
    }
}