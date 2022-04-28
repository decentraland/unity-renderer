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

            RenderSimpleValues.RenderPrefabInput("Prefab", ref config.prefab);
            RenderSimpleValues.RenderFloatField("Width", ref config.satelliteSize);
            RenderSimpleValues.RenderFloatField("Length", ref config.radius);
            RenderSimpleValues.RenderFloatField("Y-Pos", ref config.radius);
        }
    }
}