using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderCubemapLayer
    {

        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
        {
            // Cubemap
            RenderSimpleValues.RenderCubemapTexture("Cubemap", ref layer.cubemap);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            EditorGUILayout.Separator();

            // Movement Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypeCubemap = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypeCubemap, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Rotation
            if (layer.movementTypeCubemap == MovementType.PointBased)
            {
                RenderTransitioningVariables.RenderTransitioningVector3(ref timeOfTheDay, layer.rotations_Vector3, "Rotation", "%", "Rot:", layer.timeSpan_start, layer.timeSpan_End);

            }
            else
            {
                RenderSimpleValues.RenderVector3Field("Speed", ref layer.speed_Vector3);
            }
        }
    }
}