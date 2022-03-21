using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderPlanarLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer, bool isRadial = false)
        {
            // Texture
            RenderSimpleValues.RenderTexture(SkyboxEditorLiterals.texture, ref layer.texture);

            // Row and Coloumns
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.rowsColumns, ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.animSpeed, ref layer.flipbookAnimSpeed);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, SkyboxEditorLiterals.color, layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.tiling, ref layer.tiling);

            // Movement Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(SkyboxEditorLiterals.movementType, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypePlanar_Radial = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypePlanar_Radial, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            if (layer.movementTypePlanar_Radial == MovementType.Speed)
            {
                // Speed
                RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.speed, ref layer.speed_Vector2);
            }
            else
            {
                // Offset
                RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.offset, SkyboxEditorLiterals.position, SkyboxEditorLiterals.percentage, "", layer.timeSpan_start, layer.timeSpan_End);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(15);

            // Render Distance
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.renderDistance, SkyboxEditorLiterals.renderDistance, "", "", true, 0, 20, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(15);

            // Rotation
            if (!isRadial)
            {
                RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.rotations_float, SkyboxEditorLiterals.rotation, "", "", true, 0, 360, layer.timeSpan_start, layer.timeSpan_End);
                EditorGUILayout.Separator();
            }

            RenderDistortionVariables(ref timeOfTheDay, toolSize, layer);

            EditorGUILayout.Space(10);
        }

        public static void RenderDistortionVariables(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
        {
            layer.distortionExpanded = EditorGUILayout.Foldout(layer.distortionExpanded, SkyboxEditorLiterals.distortionValues, true, EditorStyles.foldoutHeader);

            if (!layer.distortionExpanded)
            {
                return;
            }

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel++;

            // Distortion Intensity
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.distortIntensity, SkyboxEditorLiterals.intensity, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Size
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.distortSize, SkyboxEditorLiterals.size, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Speed
            RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.distortSpeed, SkyboxEditorLiterals.speed, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Sharpness
            RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.distortSharpness, SkyboxEditorLiterals.sharpness, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel--;
        }
    }
}