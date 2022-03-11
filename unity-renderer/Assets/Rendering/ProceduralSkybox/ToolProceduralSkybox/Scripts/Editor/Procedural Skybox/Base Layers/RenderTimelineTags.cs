using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderTimelineTags
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {
            if (config.timelineTags == null)
            {
                config.timelineTags = new List<TimelineTagsDuration>();
            }

            for (int i = 0; i < config.timelineTags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                // Text field for name of event
                EditorGUILayout.LabelField("Name", GUILayout.Width(50));
                config.timelineTags[i].tag = EditorGUILayout.TextField(config.timelineTags[i].tag, GUILayout.Width(70));

                // Start time
                EditorGUILayout.LabelField("Start", GUILayout.Width(45));
                GUILayout.Space(0);
                config.timelineTags[i].startTime = EditorGUILayout.FloatField(config.timelineTags[i].startTime, GUILayout.Width(50));
                SkyboxEditorUtils.ClampToDayTime(ref config.timelineTags[i].startTime);

                // End time
                if (!config.timelineTags[i].isTrigger)
                {
                    EditorGUILayout.LabelField("End", GUILayout.Width(40));
                    GUILayout.Space(0);
                    config.timelineTags[i].endTime = EditorGUILayout.FloatField(config.timelineTags[i].endTime, GUILayout.Width(50));
                    SkyboxEditorUtils.ClampToDayTime(ref config.timelineTags[i].endTime);
                }
                else
                {
                    GUILayout.Space(97);
                }

                // no end time
                config.timelineTags[i].isTrigger = EditorGUILayout.ToggleLeft("Trigger", config.timelineTags[i].isTrigger, GUILayout.Width(80));

                // Remove Button
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    config.timelineTags.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                config.timelineTags.Add(new TimelineTagsDuration(timeOfTheDay));
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}