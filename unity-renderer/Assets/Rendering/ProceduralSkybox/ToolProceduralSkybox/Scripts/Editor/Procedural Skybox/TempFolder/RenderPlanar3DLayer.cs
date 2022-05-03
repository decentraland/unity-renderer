using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderPlanar3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, Planar3DConfig config)
        {

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            config.nameInEditor = EditorGUILayout.TextField(config.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.timeSpan, SkyboxEditorLiterals.start, ref config.timeSpan_start, SkyboxEditorLiterals.end, ref config.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_End);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.fade, SkyboxEditorLiterals.inStr, ref config.fadeInTime, SkyboxEditorLiterals.outStr, ref config.fadeOutTime);

            //// Tint
            //RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.tint, ref layer.tintPercentage, 0, 100);

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

            RenderSimpleValues.RenderFloatField("Radius", ref config.radius);
            RenderSimpleValues.RenderFloatField("Y-Pos", ref config.yPos);
            RenderSimpleValues.RenderBoolField("Follow Camera", ref config.followCamera);
            RenderSimpleValues.RenderBoolField("Render In Main Camera", ref config.renderWithMainCamera);
        }
    }
}