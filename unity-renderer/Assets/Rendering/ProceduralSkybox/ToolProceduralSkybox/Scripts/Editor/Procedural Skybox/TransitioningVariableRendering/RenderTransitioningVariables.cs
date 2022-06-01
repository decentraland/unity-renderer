using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderTransitioningVariables
    {
        public static void RenderTransitioningVector3(ref float timeOfTheDay, List<TransitioningVector3> list, string label, string percentTxt, string valueText, float layerStartTime = 0, float layerEndTime = 24)
        {
            EditorGUILayout.BeginHorizontal(SkyboxEditorStyles.Instance.transitioningBoxStyle, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector3 tLastPos = Vector3.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningVector3(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLastPos));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_goto, GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = SkyboxEditorUtils.GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                // Percentage
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Space(10);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                list[i].value = EditorGUILayout.Vector3Field("", list[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_remove, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector3 tLastPos = Vector3.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[i].value;
                    }
                    list.Insert(i + 1, new TransitioningVector3(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLastPos));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void RenderTransitioningVector2(ref float timeOfTheDay, List<TransitioningVector2> list, string label, string percentTxt, string valueText, float layerStartTime = 0, float layerEndTime = 24)
        {
            EditorGUILayout.BeginHorizontal(SkyboxEditorStyles.Instance.transitioningBoxStyle, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector2 tLastPos = Vector2.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningVector2(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLastPos));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_goto, GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = SkyboxEditorUtils.GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                // Percentage
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                list[i].value = EditorGUILayout.Vector2Field("", list[i].value, GUILayout.Width(120), GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_remove, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector2 tLastPos = Vector2.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[i].value;
                    }
                    list.Insert(i + 1, new TransitioningVector2(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLastPos));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void RenderTransitioningFloat(EditorToolMeasurements toolSize, ref float timeOfTheDay, List<TransitioningFloat> list, string label, string percentTxt, string valueText, bool slider = false, float min = 0, float max = 1, float layerStartTime = 0, float layerEndTime = 24)
        {
            SkyboxEditorStyles.Instance.transitioningBoxStyle.normal.background = toolSize.transitioningVariableBG.backgroundTex;

            GUILayout.BeginHorizontal(SkyboxEditorStyles.Instance.transitioningBoxStyle, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    float tLast = 0;
                    if (list.Count != 0)
                    {
                        tLast = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningFloat(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLast));
                }
                GUILayout.EndHorizontal();
            }

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_goto, GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = SkyboxEditorUtils.GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                if (slider)
                {
                    list[i].value = EditorGUILayout.Slider(list[i].value, min, max, GUILayout.Width(120), GUILayout.ExpandWidth(false));
                }
                else
                {
                    list[i].value = EditorGUILayout.FloatField("", list[i].value, GUILayout.Width(70), GUILayout.ExpandWidth(false));
                }


                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_remove, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                GUILayout.Space(10);
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    float tLast = 0;
                    if (list.Count != 0)
                    {
                        tLast = list[i].value;
                    }
                    list.Insert(i + 1, new TransitioningFloat(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, tLast));
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public static void RenderTransitioningQuaternionAsVector3(ref float timeOfTheDay, List<TransitioningQuaternion> list, string label, string percentTxt, string valueText, Func<Quaternion> GetCurrentRotation, float layerStartTime = 0, float layerEndTime = 24)
        {
            EditorGUILayout.BeginHorizontal(SkyboxEditorStyles.Instance.transitioningBoxStyle, GUILayout.ExpandWidth(false));

            GUILayout.Label(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));

            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.Add(new TransitioningQuaternion(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, GetCurrentRotation()));
                }
                EditorGUILayout.EndHorizontal();
            }

            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_goto, GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = SkyboxEditorUtils.GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Space(10);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                // Convert Quaternion to Vector3
                list[i].value = Quaternion.Euler(EditorGUILayout.Vector3Field("", list[i].value.eulerAngles, GUILayout.ExpandWidth(false)));

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_remove, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                    break;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.Insert(i + 1, new TransitioningQuaternion(SkyboxEditorUtils.GetNormalizedLayerCurrentTime(timeOfTheDay, layerStartTime, layerEndTime) * 100, GetCurrentRotation()));
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void RenderPercentagePart(float layerStartTime, float layerEndTime, ref float percentage)
        {
            GUILayout.Label(layerStartTime + SkyboxEditorLiterals.short_Hour, GUILayout.Width(35), GUILayout.ExpandWidth(false));

            GUILayout.BeginVertical(SkyboxEditorStyles.Instance.percentagePartStyle, GUILayout.ExpandWidth(false), GUILayout.Width(150));
            float time = SkyboxEditorUtils.GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, percentage / 100);
            GUILayout.Label(time.ToString("f2") + SkyboxEditorLiterals.short_Hour, GUILayout.ExpandWidth(false));
            percentage = EditorGUILayout.Slider(percentage, 0, 100, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            GUILayout.EndVertical();

            GUILayout.Label(layerEndTime + SkyboxEditorLiterals.short_Hour, GUILayout.Width(35), GUILayout.ExpandWidth(false));
        }
    }
}