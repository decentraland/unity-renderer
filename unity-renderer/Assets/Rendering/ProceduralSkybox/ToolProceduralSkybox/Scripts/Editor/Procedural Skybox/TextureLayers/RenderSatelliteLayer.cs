using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderSatelliteLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
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

            EditorGUILayout.Space(10);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypeSatellite = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypeSatellite, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            if (layer.movementTypeSatellite == MovementType.Speed)
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
            EditorGUILayout.Space(20);

            // Rotation
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, layer.rotations_float, "Rotation", "", "", true, 0, 360, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(12);

            // Size
            RenderTransitioningVariables.RenderTransitioningVector2(ref timeOfTheDay, layer.satelliteWidthHeight, "Width & Height", "%", "", layer.timeSpan_start, layer.timeSpan_End);
        }
    }
}