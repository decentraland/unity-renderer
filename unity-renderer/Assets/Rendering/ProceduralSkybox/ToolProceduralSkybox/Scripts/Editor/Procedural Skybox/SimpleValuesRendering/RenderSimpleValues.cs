using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderSimpleValues
    {
        public static void RenderSepratedFloatFields(string label, string label1, ref float value1, string label2, ref float value2)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label1, GUILayout.Width(90), GUILayout.ExpandWidth(false));
            value1 = EditorGUILayout.FloatField("", value1, GUILayout.Width(90));
            EditorGUILayout.LabelField(label2, GUILayout.Width(90), GUILayout.ExpandWidth(false));
            value2 = EditorGUILayout.FloatField("", value2, GUILayout.Width(90));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderFloatField(string label, ref float value)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.FloatField(value, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderFloatFieldAsSlider(string label, ref float value, float min, float max)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Slider(value, min, max, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderVector3Field(string label, ref Vector3 value)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Vector3Field("", value, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderVector2Field(string label, ref Vector2 value)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Vector2Field("", value, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderTexture(string label, ref Texture2D tex)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            tex = (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderCubemapTexture(string label, ref Cubemap tex)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            tex = (Cubemap)EditorGUILayout.ObjectField(tex, typeof(Cubemap), false, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        public static void RenderColorGradientField(Gradient color, string label = "color", float startTime = -1, float endTime = -1, bool hdr = false)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));

            if (startTime != -1)
            {
                EditorGUILayout.LabelField(startTime + "Hr", GUILayout.Width(65), GUILayout.ExpandWidth(false));
            }

            color = EditorGUILayout.GradientField(new GUIContent(""), color, hdr, GUILayout.Width(250), GUILayout.ExpandWidth(false));

            if (endTime != 1)
            {
                EditorGUILayout.LabelField(endTime + "Hr", GUILayout.Width(65), GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }
    }
}