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
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Labels.inWorld, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            // Avatar Color
            selectedConfiguration.useAvatarGradient = EditorGUILayout.Toggle(SkyboxEditorLiterals.Labels.colorGradient, selectedConfiguration.useAvatarGradient, GUILayout.Width(500));

            if (selectedConfiguration.useAvatarGradient)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarTintGradient, SkyboxEditorLiterals.LayerProperties.tintGradient, 0, 24, true);
            }
            else
            {
                selectedConfiguration.avatarTintColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.LayerProperties.tint, selectedConfiguration.avatarTintColor, GUILayout.Width(400));
                EditorGUILayout.Separator();
            }

            // Avatar Light Direction
            selectedConfiguration.useAvatarRealtimeDLDirection = EditorGUILayout.Toggle(SkyboxEditorLiterals.LayerProperties.realtimeDLDirecn, selectedConfiguration.useAvatarRealtimeDLDirection);

            if (!selectedConfiguration.useAvatarRealtimeDLDirection)
            {
                RenderSimpleValues.RenderVector3Field(SkyboxEditorLiterals.LayerProperties.lightDirection, ref selectedConfiguration.avatarLightConstantDir);
            }

            EditorGUILayout.Separator();

            // Avatar Light Color
            selectedConfiguration.useAvatarRealtimeLightColor = EditorGUILayout.Toggle(SkyboxEditorLiterals.Labels.realtimeLightColor, selectedConfiguration.useAvatarRealtimeLightColor);

            if (!selectedConfiguration.useAvatarRealtimeLightColor)
            {
                RenderSimpleValues.RenderColorGradientField(selectedConfiguration.avatarLightColorGradient, SkyboxEditorLiterals.LayerProperties.lightColor, 0, 24);
                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField(SkyboxEditorLiterals.Labels.inEditorBackpack, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            selectedConfiguration.avatarEditorTintColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.LayerProperties.tint, selectedConfiguration.avatarEditorTintColor, GUILayout.Width(400));
            RenderSimpleValues.RenderVector3Field(SkyboxEditorLiterals.LayerProperties.lightDirection, ref selectedConfiguration.avatarEditorLightDir);
            selectedConfiguration.avatarEditorLightColor = EditorGUILayout.ColorField(SkyboxEditorLiterals.LayerProperties.lightColor, selectedConfiguration.avatarEditorLightColor, GUILayout.Width(400));
            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }
    }
}