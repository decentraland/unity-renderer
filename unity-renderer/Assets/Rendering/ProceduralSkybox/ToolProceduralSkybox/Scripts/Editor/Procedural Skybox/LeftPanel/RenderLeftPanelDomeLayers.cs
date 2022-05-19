using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderLeftPanelDomeLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel, CopyFunctionality copyPasteObj)
        {
            // Loop through texture layer and print the name of all layers
            for (int i = 0; i < config.additional3Dconfig.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                config.additional3Dconfig[i].enabled = EditorGUILayout.Toggle(config.additional3Dconfig[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(config.additional3Dconfig[i].layers.nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Elements3D_Dome, name = config.additional3Dconfig[i].layers.nameInEditor, targetDomeElement = config.additional3Dconfig[i] });
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.upArrow.ToString()))
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

                if (i == config.additional3Dconfig.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.downArrow.ToString()))
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

                // Rendering marker
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

                EditorGUILayout.LabelField(SkyboxEditorLiterals.Characters.renderMarker.ToString(), SkyboxEditorStyles.Instance.renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

                GUI.color = normalContentColor;

                // Dome context menu
                if (GUILayout.Button(":", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    ArrayList list = new ArrayList();
                    list.Add(config.additional3Dconfig);
                    list.Add(i);

                    // Anonymous method for delete operation
                    GenericMenu.MenuFunction2 deleteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DDome> domeList = list[0] as List<Config3DDome>;
                        int index = (int)list[1];
                        domeList.RemoveAt(index);
                    };

                    // Anonymous method for Add operation
                    GenericMenu.MenuFunction2 addBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DDome> domeList = list[0] as List<Config3DDome>;
                        int index = (int)list[1];
                        domeList.Insert(index + 1, new Config3DDome("Dome " + (domeList.Count + 1)));
                    };

                    // Anonymous method for copy layer
                    GenericMenu.MenuFunction2 copyBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DDome> layerList = list[0] as List<Config3DDome>;
                        int index = (int)list[1];
                        copyPasteObj.SetDome(layerList[index]);
                    };

                    // Anonymous method for Paste layer
                    GenericMenu.MenuFunction2 pasteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DDome> layerList = list[0] as List<Config3DDome>;
                        int index = (int)list[1];
                        layerList[index] = copyPasteObj.GetCopiedDome().DeepCopy();
                    };
                    ShowDomeContextMenu(copyPasteObj, deleteBtnClicked, addBtnClicked, copyBtnClicked, pasteBtnClicked, list);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            }
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.Characters.sign_add))
            {
                config.additional3Dconfig.Add(new Config3DDome("Dome " + (config.additional3Dconfig.Count + 1)));
            }

            EditorGUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowDomeContextMenu(CopyFunctionality copyPasteObj, GenericMenu.MenuFunction2 OnDeleteBtnClicked, GenericMenu.MenuFunction2 OnAddBtnClicked, GenericMenu.MenuFunction2 OnCopyBtnClicked, GenericMenu.MenuFunction2 OnPasteBtnClicked, object list)
        {
            // Create menu
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add"), false, OnAddBtnClicked, list);

            // Copy option
            menu.AddItem(new GUIContent("Copy"), false, OnCopyBtnClicked, list);

            // Paste option
            if (copyPasteObj.IsDomeAvailable())
            {
                menu.AddItem(new GUIContent("Paste"), false, OnPasteBtnClicked, list);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
            }

            menu.AddSeparator("");
            // Delete option
            menu.AddItem(new GUIContent("Delete"), false, OnDeleteBtnClicked, list);

            menu.ShowAsContext();
        }
    }
}