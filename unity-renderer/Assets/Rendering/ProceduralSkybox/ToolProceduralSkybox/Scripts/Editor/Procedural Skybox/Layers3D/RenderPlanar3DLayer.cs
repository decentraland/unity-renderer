using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderPlanar3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, Config3DPlanar config)
        {

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            config.nameInEditor = EditorGUILayout.TextField(config.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.timeSpan, SkyboxEditorLiterals.LayerProperties.start, ref config.timeSpan_start, SkyboxEditorLiterals.LayerProperties.end, ref config.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_End);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.fade, SkyboxEditorLiterals.LayerProperties.inStr, ref config.fadeInTime, SkyboxEditorLiterals.LayerProperties.outStr, ref config.fadeOutTime);

            GameObject tempPrefab = config.prefab;
            RenderSimpleValues.RenderPrefabInput("Prefab", ref tempPrefab);
            config.AssignNewPrefab(tempPrefab);


            if (!config.validPrefab)
            {
                Color color = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField(config.inValidStr, GUILayout.Width(400), GUILayout.ExpandWidth(false));
                GUI.color = color;
                config.prefab = null;
            }

            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Planar3D.SCENE, ref config.radius);
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Planar3D.Y_POS, ref config.yPos);
            RenderSimpleValues.RenderBoolField(SkyboxEditorLiterals.Planar3D.FOLLOW_CAMERA, ref config.followCamera);
            RenderSimpleValues.RenderBoolField(SkyboxEditorLiterals.Planar3D.RENDER_IN_MAIN_CAMERA, ref config.renderWithMainCamera);
        }
    }
}