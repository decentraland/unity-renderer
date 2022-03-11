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
            RenderSimpleValues.RenderTexture("Texture", ref layer.texture);

            // Row and Coloumns
            RenderSimpleValues.RenderVector2Field("Rows and Columns", ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderSimpleValues.RenderFloatField("Anim Speed", ref layer.flipbookAnimSpeed);

            // Normal Texture
            RenderSimpleValues.RenderTexture("Normal Map", ref layer.textureNormal);

            // Normal Intensity
            RenderSimpleValues.RenderFloatFieldAsSlider("Normal Intensity", ref layer.normalIntensity, 0, 1);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderSimpleValues.RenderVector2Field("Tiling", ref layer.tiling);

            // Movement Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypePlanar_Radial = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypePlanar_Radial, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            if (layer.movementTypePlanar_Radial == MovementType.Speed)
            {
                // Speed
                RenderSimpleValues.RenderVector2Field("Speed", ref layer.speed_Vector2);
            }
            else
            {
                // Offset
                RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.offset, "Position", "%", "", layer.timeSpan_start, layer.timeSpan_End);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(15);

            // Render Distance
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.renderDistance, "Render Distance", "", "", true, 0, 20, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(15);

            // Rotation
            if (!isRadial)
            {
                RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.rotations_float, "Rotation", "", "", true, 0, 360, layer.timeSpan_start, layer.timeSpan_End);
                EditorGUILayout.Separator();
            }

            RenderDistortionVariables(ref timeOfTheDay, toolSize, layer);

            EditorGUILayout.Space(10);
        }

        public static void RenderDistortionVariables(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
        {
            layer.distortionExpanded = EditorGUILayout.Foldout(layer.distortionExpanded, "Distortion Values", true, EditorStyles.foldoutHeader);

            if (!layer.distortionExpanded)
            {
                return;
            }

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel++;

            // Distortion Intensity
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.distortIntensity, "Intensity", "%", "Value", false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Size
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.distortSize, "Size", "%", "Value", false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Speed
            RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.distortSpeed, "Speed", "%", "Value", layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Sharpness
            RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.distortSharpness, "Sharpness", "%", "Value", layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel--;
        }
    }
}