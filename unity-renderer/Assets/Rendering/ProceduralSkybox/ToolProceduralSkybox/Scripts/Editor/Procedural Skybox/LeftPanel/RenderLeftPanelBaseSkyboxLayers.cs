using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderLeftPanelBaseSkyboxLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel, List<string> renderingOrderList)
        {
            // Loop through texture layer and print the name of all layers
            for (int i = 0; i < config.layers.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                config.layers[i].enabled = EditorGUILayout.Toggle(config.layers[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(config.layers[i].nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Base_Skybox, name = config.layers[i].nameInEditor, baseSkyboxTargetLayer = config.layers[i] });
                }

                config.layers[i].slotID = EditorGUILayout.Popup(config.layers[i].slotID, renderingOrderList.ToArray(), GUILayout.Width(toolSize.layerRenderWidth));

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.upArrow.ToString()))
                {
                    TextureLayer temp = null;

                    if (i >= 1)
                    {
                        temp = config.layers[i - 1];
                        config.layers[i - 1] = config.layers[i];
                        config.layers[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == config.layers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.downArrow.ToString()))
                {
                    TextureLayer temp = null;
                    if (i < (config.layers.Count - 1))
                    {
                        temp = config.layers[i + 1];
                        config.layers[i + 1] = config.layers[i];
                        config.layers[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                if (GUILayout.Button(SkyboxEditorLiterals.sign_remove))
                {
                    config.layers.RemoveAt(i);
                    break;
                }

                Color circleColor = Color.green;
                switch (config.layers[i].renderType)
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
                config.layers.Add(new TextureLayer("Tex Layer " + (config.layers.Count + 1)));
            }

            EditorGUILayout.Space(25);
            EditorGUILayout.EndHorizontal();

        }
    }
}