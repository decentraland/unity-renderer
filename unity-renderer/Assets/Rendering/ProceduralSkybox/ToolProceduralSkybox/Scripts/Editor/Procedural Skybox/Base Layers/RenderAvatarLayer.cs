using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderAvatarLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration selectedConfiguration)
        {
            EditorGUILayout.LabelField("In World", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            // Avatar Color
            selectedConfiguration.useAvatarGradient = EditorGUILayout.Toggle("Color Gradient", selectedConfiguration.useAvatarGradient, GUILayout.Width(500));

            if (selectedConfiguration.useAvatarGradient)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarTintGradient, "Tint Gradient", 0, 24, true);
            }
            else
            {
                selectedConfiguration.avatarTintColor = EditorGUILayout.ColorField("Tint Color", selectedConfiguration.avatarTintColor, GUILayout.Width(400));
                EditorGUILayout.Separator();
            }

            // Avatar Light Direction
            selectedConfiguration.useAvatarRealtimeDLDirection = EditorGUILayout.Toggle("Realtime DL Direction", selectedConfiguration.useAvatarRealtimeDLDirection);

            if (!selectedConfiguration.useAvatarRealtimeDLDirection)
            {
                RenderSimpleValues.RenderVector3Field("Light Direction", ref selectedConfiguration.avatarLightConstantDir);
            }

            EditorGUILayout.Separator();

            // Avatar Light Color
            selectedConfiguration.useAvatarRealtimeLightColor = EditorGUILayout.Toggle("Realtime Light Color", selectedConfiguration.useAvatarRealtimeLightColor);

            if (!selectedConfiguration.useAvatarRealtimeLightColor)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarLightColorGradient, "Light Color", 0, 24);
                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("In Editor (Backpack)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            selectedConfiguration.avatarEditorTintColor = EditorGUILayout.ColorField("Tint Color", selectedConfiguration.avatarEditorTintColor, GUILayout.Width(400));
            RenderSimpleValues.RenderVector3Field("Light Direction", ref selectedConfiguration.avatarEditorLightDir);
            selectedConfiguration.avatarEditorLightColor = EditorGUILayout.ColorField("Light Color", selectedConfiguration.avatarEditorLightColor, GUILayout.Width(400));
            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }
    }
}