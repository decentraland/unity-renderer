using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderLeftPanelPlanarLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel, CopyFunctionality copyPasteObj)
        {
            // Loop through planar list and print the name of all layers
            for (int i = 0; i < config.planarLayers.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                config.planarLayers[i].enabled = EditorGUILayout.Toggle(config.planarLayers[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(config.planarLayers[i].nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Elements3D_Planar, name = config.planarLayers[i].nameInEditor, targetPlanarElement = config.planarLayers[i] });
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.upArrow.ToString()))
                {
                    Config3DPlanar temp = null;

                    if (i >= 1)
                    {
                        temp = config.planarLayers[i - 1];
                        config.planarLayers[i - 1] = config.planarLayers[i];
                        config.planarLayers[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == config.planarLayers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.downArrow.ToString()))
                {
                    Config3DPlanar temp = null;
                    if (i < (config.planarLayers.Count - 1))
                    {
                        temp = config.planarLayers[i + 1];
                        config.planarLayers[i + 1] = config.planarLayers[i];
                        config.planarLayers[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_remove))
                {
                    config.planarLayers.RemoveAt(i);
                    break;
                }

                Color circleColor = Color.green;
                switch (config.planarLayers[i].renderType)
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

                EditorGUILayout.LabelField(SkyboxEditorLiterals.Characters.renderMarker.ToString(), SkyboxEditorStyles.Instance.renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

                GUI.color = normalContentColor;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            }
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.Characters.sign_add))
            {
                config.planarLayers.Add(new Config3DPlanar("Plane " + (config.planarLayers.Count + 1)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(25);
        }
    }
}