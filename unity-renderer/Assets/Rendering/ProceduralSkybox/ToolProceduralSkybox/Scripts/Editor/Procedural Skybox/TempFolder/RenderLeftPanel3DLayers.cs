using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderLeftPanel3DLayers
    {
        public static void Render(SkyboxConfiguration selectedConfiguration, EditorToolMeasurements toolSize, Action<RightPanelPins> AddToRightPanel)
        {
            // Render dome objects

            // Render satellite objects
            RenderSatellites(selectedConfiguration, toolSize, AddToRightPanel);

            // Render planar objects
            RenderPlanar(selectedConfiguration, toolSize, AddToRightPanel);
        }

        private static void RenderPlanar(SkyboxConfiguration selectedConfiguration, EditorToolMeasurements toolSize, Action<RightPanelPins> AddToRightPanel)
        {
            // Loop through satellite list and print the name of all layers
            for (int i = 0; i < selectedConfiguration.planarLayers.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                selectedConfiguration.planarLayers[i].enabled = EditorGUILayout.Toggle(selectedConfiguration.planarLayers[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(selectedConfiguration.planarLayers[i].nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Planar_Layer, name = selectedConfiguration.planarLayers[i].nameInEditor, planarLayerIndex = i });
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.upArrow.ToString()))
                {
                    Planar3DConfig temp = null;

                    if (i >= 1)
                    {
                        temp = selectedConfiguration.planarLayers[i - 1];
                        selectedConfiguration.planarLayers[i - 1] = selectedConfiguration.planarLayers[i];
                        selectedConfiguration.planarLayers[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == selectedConfiguration.planarLayers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.downArrow.ToString()))
                {
                    Planar3DConfig temp = null;
                    if (i < (selectedConfiguration.planarLayers.Count - 1))
                    {
                        temp = selectedConfiguration.planarLayers[i + 1];
                        selectedConfiguration.planarLayers[i + 1] = selectedConfiguration.planarLayers[i];
                        selectedConfiguration.planarLayers[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                if (GUILayout.Button(SkyboxEditorLiterals.sign_remove))
                {
                    selectedConfiguration.planarLayers.RemoveAt(i);
                    break;
                }

                Color circleColor = Color.green;
                switch (selectedConfiguration.planarLayers[i].renderType)
                {
                    case LayerRenderType.Rendering:
                        circleColor = Color.green;
                        break;
                    case LayerRenderType.NotRendering:
                        circleColor = Color.gray;
                        break;
                    case LayerRenderType.Conflict_Playing:
                        circleColor = Color.yellow;
                        break;
                    case LayerRenderType.Conflict_NotPlaying:
                        circleColor = Color.red;
                        break;
                    default:
                        break;
                }

                Color normalContentColor = GUI.color;
                GUI.color = circleColor;

                EditorGUILayout.LabelField(SkyboxEditorLiterals.renderMarker.ToString(), SkyboxEditorStyles.Instance.renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

                GUI.color = normalContentColor;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            }
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.sign_add))
            {
                selectedConfiguration.planarLayers.Add(new Planar3DConfig("Plane " + (selectedConfiguration.planarLayers.Count + 1)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(25);
        }

        private static void RenderSatellites(SkyboxConfiguration selectedConfiguration, EditorToolMeasurements toolSize, Action<RightPanelPins> AddToRightPanel)
        {
            //// Loop through satellite list and print the name of all layers
            //for (int i = 0; i < selectedConfiguration.satelliteLayers.Count; i++)
            //{

            //    EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

            //    selectedConfiguration.satelliteLayers[i].enabled = EditorGUILayout.Toggle(selectedConfiguration.satelliteLayers[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

            //    if (GUILayout.Button(selectedConfiguration.satelliteLayers[i].nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
            //    {
            //        AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Satellite_Layer, name = selectedConfiguration.satelliteLayers[i].nameInEditor, satelliteLayerIndex = i });
            //    }

            //    if (i == 0)
            //    {
            //        GUI.enabled = false;
            //    }
            //    if (GUILayout.Button(SkyboxEditorLiterals.upArrow.ToString()))
            //    {
            //        Satellite3DLayer temp = null;

            //        if (i >= 1)
            //        {
            //            temp = selectedConfiguration.satelliteLayers[i - 1];
            //            selectedConfiguration.satelliteLayers[i - 1] = selectedConfiguration.satelliteLayers[i];
            //            selectedConfiguration.satelliteLayers[i] = temp;
            //        }
            //    }

            //    GUI.enabled = true;

            //    if (i == selectedConfiguration.satelliteLayers.Count - 1)
            //    {
            //        GUI.enabled = false;
            //    }

            //    if (GUILayout.Button(SkyboxEditorLiterals.downArrow.ToString()))
            //    {
            //        Satellite3DLayer temp = null;
            //        if (i < (selectedConfiguration.satelliteLayers.Count - 1))
            //        {
            //            temp = selectedConfiguration.satelliteLayers[i + 1];
            //            selectedConfiguration.satelliteLayers[i + 1] = selectedConfiguration.satelliteLayers[i];
            //            selectedConfiguration.satelliteLayers[i] = temp;
            //        }
            //        break;
            //    }

            //    GUI.enabled = true;

            //    if (GUILayout.Button(SkyboxEditorLiterals.sign_remove))
            //    {
            //        selectedConfiguration.satelliteLayers.RemoveAt(i);
            //        break;
            //    }

            //    Color circleColor = Color.green;
            //    switch (selectedConfiguration.satelliteLayers[i].renderType)
            //    {
            //        case LayerRenderType.Rendering:
            //            circleColor = Color.green;
            //            break;
            //        case LayerRenderType.NotRendering:
            //            circleColor = Color.gray;
            //            break;
            //        case LayerRenderType.Conflict_Playing:
            //            circleColor = Color.yellow;
            //            break;
            //        case LayerRenderType.Conflict_NotPlaying:
            //            circleColor = Color.red;
            //            break;
            //        default:
            //            break;
            //    }

            //    Color normalContentColor = GUI.color;
            //    GUI.color = circleColor;

            //    EditorGUILayout.LabelField(SkyboxEditorLiterals.renderMarker.ToString(), SkyboxEditorStyles.Instance.renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

            //    GUI.color = normalContentColor;
            //    EditorGUILayout.EndHorizontal();

            //    EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            //}
            //Rect r = EditorGUILayout.BeginHorizontal();
            //if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.sign_add))
            //{
            //    selectedConfiguration.satelliteLayers.Add(new Satellite3DLayer("Tex Layer " + (selectedConfiguration.satelliteLayers.Count + 1)));
            //}
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.Space(25);
        }
    }
}