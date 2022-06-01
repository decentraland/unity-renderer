using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderLeftPanelBaseSkyboxLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel, List<string> renderingOrderList, CopyFunctionality copyPasteObj)
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
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.upArrow.ToString()))
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

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.downArrow.ToString()))
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

                EditorGUILayout.LabelField(SkyboxEditorLiterals.Characters.renderMarker.ToString(), SkyboxEditorStyles.Instance.renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

                GUI.color = normalContentColor;

                // Dome context menu
                if (GUILayout.Button(":", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    ArrayList list = new ArrayList();
                    list.Add(config.layers);
                    list.Add(i);

                    // Anonymous method for delete operation
                    GenericMenu.MenuFunction2 deleteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<TextureLayer> layerList = list[0] as List<TextureLayer>;
                        int index = (int)list[1];
                        layerList.RemoveAt(index);
                    };

                    // Anonymous method for Add operation
                    GenericMenu.MenuFunction2 addBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<TextureLayer> layerList = list[0] as List<TextureLayer>;
                        int index = (int)list[1];
                        layerList.Insert(index + 1, new TextureLayer("New Layer"));
                    };

                    // Anonymous method for copy layer
                    GenericMenu.MenuFunction2 copyBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<TextureLayer> layerList = list[0] as List<TextureLayer>;
                        int index = (int)list[1];
                        copyPasteObj.SetTextureLayer(layerList[index]);
                    };

                    // Anonymous method for Paste layer
                    GenericMenu.MenuFunction2 pasteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<TextureLayer> layerList = list[0] as List<TextureLayer>;
                        int index = (int)list[1];
                        layerList[index] = copyPasteObj.GetCopiedTextureLayer().DeepCopy();
                    };
                    ShowBaseLayersContextMenu(copyPasteObj, deleteBtnClicked, addBtnClicked, copyBtnClicked, pasteBtnClicked, list);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            }
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.Characters.sign_add))
            {
                config.layers.Add(new TextureLayer("New Layer"));
            }

            EditorGUILayout.Space(25);
            EditorGUILayout.EndHorizontal();

        }

        private static void ShowBaseLayersContextMenu(CopyFunctionality copyPasteObj, GenericMenu.MenuFunction2 OnDeleteBtnClicked, GenericMenu.MenuFunction2 OnAddBtnClicked, GenericMenu.MenuFunction2 OnCopyBtnClicked, GenericMenu.MenuFunction2 OnPasteBtnClicked, object list)
        {
            // Create menu
            GenericMenu menu = new GenericMenu();

            // Add option
            menu.AddItem(new GUIContent("Add"), false, OnAddBtnClicked, list);

            // Copy option
            menu.AddItem(new GUIContent("Copy"), false, OnCopyBtnClicked, list);

            // Paste option
            if (copyPasteObj.IsTextureLayerAvailable())
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