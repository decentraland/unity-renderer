using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderLeftPanel3DLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel)
        {
            // Loop through texture layer and print the name of all layers
            for (int i = 0; i < config.additional3Dconfig.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                config.additional3Dconfig[i].enabled = EditorGUILayout.Toggle(config.additional3Dconfig[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(config.additional3Dconfig[i].layers.nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Elements3D_Dome, name = config.additional3Dconfig[i].layers.nameInEditor, dome3DElementsIndex = i });
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.upArrow.ToString()))
                {
                    Config3DDome temp = null;

                    if (i >= 1)
                    {
                        temp = config.additional3Dconfig[i - 1];
                        config.additional3Dconfig[i - 1] = config.additional3Dconfig[i];
                        config.additional3Dconfig[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == config.layers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.downArrow.ToString()))
                {
                    Config3DDome temp = null;
                    if (i < (config.additional3Dconfig.Count - 1))
                    {
                        temp = config.additional3Dconfig[i + 1];
                        config.additional3Dconfig[i + 1] = config.additional3Dconfig[i];
                        config.additional3Dconfig[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                if (GUILayout.Button(SkyboxEditorLiterals.sign_remove))
                {
                    config.additional3Dconfig.RemoveAt(i);
                    break;
                }

                Color circleColor = Color.green;
                switch (config.additional3Dconfig[i].layers.renderType)
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
                config.additional3Dconfig.Add(new Config3DDome("Dome " + (config.additional3Dconfig.Count + 1)));
            }

            EditorGUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
        }

        //void Render3DLayers(List<Config3DDome> configs3D)
        //{
        //    for (int i = 0; i < configs3D.Count; i++)
        //    {
        //        // Name and buttons
        //        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
        //        configs3D[i].enabled = EditorGUILayout.Toggle(configs3D[i].enabled, GUILayout.Width(20), GUILayout.Height(10));
        //        GUILayout.Space(10);
        //        configs3D[i].expandedInEditor = EditorGUILayout.Foldout(configs3D[i].expandedInEditor, GUIContent.none, true, foldoutStyle);
        //        configs3D[i].nameInEditor = EditorGUILayout.TextField(configs3D[i].nameInEditor, GUILayout.Width(100), GUILayout.ExpandWidth(false));

        //        if (i == 0)
        //        {
        //            GUI.enabled = false;
        //        }
        //        if (GUILayout.Button(('\u25B2').ToString(), GUILayout.Width(50), GUILayout.ExpandWidth(false)))
        //        {
        //            Config3DDome temp = null;

        //            if (i >= 1)
        //            {
        //                temp = configs3D[i - 1];
        //                configs3D[i - 1] = configs3D[i];
        //                configs3D[i] = temp;
        //            }
        //        }

        //        GUI.enabled = true;

        //        if (i == configs3D.Count - 1)
        //        {
        //            GUI.enabled = false;
        //        }

        //        if (GUILayout.Button(('\u25BC').ToString(), GUILayout.Width(50), GUILayout.ExpandWidth(false)))
        //        {
        //            Config3DDome temp = null;
        //            if (i < (configs3D.Count - 1))
        //            {
        //                temp = configs3D[i + 1];
        //                configs3D[i + 1] = configs3D[i];
        //                configs3D[i] = temp;
        //            }
        //            break;
        //        }

        //        GUI.enabled = true;

        //        Color circleColor = Color.green;
        //        switch (configs3D[i].layers.renderType)
        //        {
        //            case LayerRenderType.Rendering:
        //                circleColor = Color.green;
        //                break;
        //            case LayerRenderType.NotRendering:
        //                circleColor = Color.gray;
        //                break;
        //            case LayerRenderType.Conflict_Playing:
        //                circleColor = Color.yellow;
        //                break;
        //            case LayerRenderType.Conflict_NotPlaying:
        //                circleColor = Color.red;
        //                break;
        //            default:
        //                break;
        //        }

        //        Color normalContentColor = GUI.color;
        //        GUI.color = circleColor;

        //        EditorGUILayout.LabelField(('\u29BF').ToString(), renderingMarkerStyle, GUILayout.Width(60), GUILayout.Height(20));

        //        GUI.color = normalContentColor;

        //        // Dome context menu
        //        if (GUILayout.Button(":", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
        //        {
        //            ArrayList list = new ArrayList();
        //            list.Add(configs3D);
        //            list.Add(i);

        //            // Anonymous method for delete operation
        //            GenericMenu.MenuFunction2 deleteBtnClicked = (object obj) =>
        //            {
        //                ArrayList list = obj as ArrayList;
        //                List<Config3DDome> domeList = list[0] as List<Config3DDome>;
        //                int index = (int)list[1];
        //                domeList.RemoveAt(index);
        //                Repaint();
        //            };

        //            // Anonymous method for Add operation
        //            GenericMenu.MenuFunction2 addBtnClicked = (object obj) =>
        //            {
        //                ArrayList list = obj as ArrayList;
        //                List<Config3DDome> domeList = list[0] as List<Config3DDome>;
        //                int index = (int)list[1];
        //                domeList.Insert(index + 1, new Config3DDome("Dome " + (domeList.Count + 1)));
        //                Repaint();
        //            };

        //            // Anonymous method for Paste layer
        //            GenericMenu.MenuFunction2 pasteBtnClicked = (object obj) =>
        //            {
        //                ArrayList list = obj as ArrayList;
        //                List<Config3DDome> domeList = list[0] as List<Config3DDome>;
        //                int index = (int)list[1];
        //                copiedLayer.DeepCopy(domeList[index].layers);
        //                Repaint();
        //            };
        //            ShowDomeContextMenu(deleteBtnClicked, addBtnClicked, pasteBtnClicked, list);
        //        }
        //        EditorGUILayout.EndHorizontal();

        //        if (configs3D[i].expandedInEditor)
        //        {
        //            EditorGUILayout.Separator();
        //            EditorGUI.indentLevel++;
        //            RenderDomeBackgroundLayer(configs3D[i]);
        //            EditorGUILayout.Separator();

        //            EditorGUILayout.BeginVertical("box");
        //            GUILayout.Space(10);
        //            RenderTextureLayer(configs3D[i].layers);
        //            GUILayout.Space(10);
        //            EditorGUILayout.EndVertical();
        //            EditorGUI.indentLevel--;
        //        }

        //        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //        GUILayout.Space(32);
        //    }

        //    GUI.enabled = true;

        //    if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
        //    {
        //        configs3D.Add(new Config3DDome("Dome " + (configs3D.Count + 1)));
        //    }
        //}

        //void ShowDomeContextMenu(GenericMenu.MenuFunction2 OnDeleteBtnClicked, GenericMenu.MenuFunction2 OnAddBtnClicked, GenericMenu.MenuFunction2 OnPasteBtnClicked, object list)
        //{
        //    // Create menu
        //    GenericMenu menu = new GenericMenu();

        //    // Add option
        //    menu.AddItem(new GUIContent("Add"), false, OnAddBtnClicked, list);
        //    // Paste option
        //    menu.AddItem(new GUIContent("Paste"), false, OnPasteBtnClicked, list);

        //    menu.AddSeparator("");
        //    // Delete option
        //    menu.AddItem(new GUIContent("Delete"), false, OnDeleteBtnClicked, list);

        //    menu.ShowAsContext();
        //}

        //void RenderDomeBackgroundLayer(Config3DDome domeObj)
        //{
        //    RenderColorGradientField(domeObj.backgroundLayer.skyColor, "Sky Color", 0, 24);
        //    RenderColorGradientField(domeObj.backgroundLayer.horizonColor, "Horizon Color", 0, 24);
        //    RenderColorGradientField(domeObj.backgroundLayer.groundColor, "Ground Color", 0, 24);

        //    EditorGUILayout.Separator();
        //    RenderTransitioningFloat(domeObj.backgroundLayer.horizonHeight, "Horizon Height", "%", "value", true, -1, 1);

        //    EditorGUILayout.Space(10);
        //    RenderTransitioningFloat(domeObj.backgroundLayer.horizonWidth, "Horizon Width", "%", "value", true, -1, 1);

        //    EditorGUILayout.Separator();

        //    // Horizon Mask
        //    RenderTexture("Texture", ref domeObj.backgroundLayer.horizonMask);

        //    // Horizon mask values
        //    RenderVector3Field("Horizon Mask Values", ref domeObj.backgroundLayer.horizonMaskValues);
        //}
    }
}