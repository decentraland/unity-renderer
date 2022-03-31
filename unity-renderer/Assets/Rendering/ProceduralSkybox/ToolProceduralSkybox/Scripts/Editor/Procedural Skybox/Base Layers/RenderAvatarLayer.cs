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
            EditorGUILayout.LabelField(SkyboxEditorLiterals.inWorld, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            // Avatar Color
            selectedConfiguration.useAvatarGradient = EditorGUILayout.Toggle(SkyboxEditorLiterals.colorGradient, selectedConfiguration.useAvatarGradient, GUILayout.Width(500));

            if (selectedConfiguration.useAvatarGradient)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarTintGradient, SkyboxEditorLiterals.tintGradient, 0, 24, true);
            }
            else
            {
                selectedConfiguration.avatarTintColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.tint, selectedConfiguration.avatarTintColor, GUILayout.Width(400));
                EditorGUILayout.Separator();
            }

            // Avatar Light Direction
            selectedConfiguration.useAvatarRealtimeDLDirection = EditorGUILayout.Toggle(SkyboxEditorLiterals.realtimeDLDirecn, selectedConfiguration.useAvatarRealtimeDLDirection);

            if (!selectedConfiguration.useAvatarRealtimeDLDirection)
            {
                RenderSimpleValues.RenderVector3Field(SkyboxEditorLiterals.lightDirection, ref selectedConfiguration.avatarLightConstantDir);
            }

            EditorGUILayout.Separator();

            // Avatar Light Color
            selectedConfiguration.useAvatarRealtimeLightColor = EditorGUILayout.Toggle(SkyboxEditorLiterals.realtimeLightColor, selectedConfiguration.useAvatarRealtimeLightColor);

            if (!selectedConfiguration.useAvatarRealtimeLightColor)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarLightColorGradient, SkyboxEditorLiterals.lightColor, 0, 24);
                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField(SkyboxEditorLiterals.inEditorBackpack, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            selectedConfiguration.avatarEditorTintColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.tint, selectedConfiguration.avatarEditorTintColor, GUILayout.Width(400));
            RenderSimpleValues.RenderVector3Field(SkyboxEditorLiterals.lightDirection, ref selectedConfiguration.avatarEditorLightDir);
            selectedConfiguration.avatarEditorLightColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.lightColor, selectedConfiguration.avatarEditorLightColor, GUILayout.Width(400));
            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }
    }
}