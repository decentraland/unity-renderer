using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderLeftPanelSatelliteLayers
    {
        public static void Render(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config, Action<RightPanelPins> AddToRightPanel, CopyFunctionality copyPasteObj)
        {
            // Loop through satellite list and print the name of all layers
            for (int i = 0; i < config.satelliteLayers.Count; i++)
            {

                EditorGUILayout.BeginHorizontal(toolSize.leftPanelHorizontal);

                config.satelliteLayers[i].enabled = EditorGUILayout.Toggle(config.satelliteLayers[i].enabled, GUILayout.Width(toolSize.layerActiveCheckboxSize), GUILayout.Height(toolSize.layerActiveCheckboxSize));

                if (GUILayout.Button(config.satelliteLayers[i].nameInEditor, GUILayout.Width(toolSize.layerButtonWidth)))
                {
                    AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Elements3D_Satellite, name = config.satelliteLayers[i].nameInEditor, targetSatelliteElement = config.satelliteLayers[i] });
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(SkyboxEditorLiterals.Characters.upArrow.ToString()))
                {
                    Config3DSatellite temp = null;

                    if (i >= 1)
                    {
                        temp = config.satelliteLayers[i - 1];
                        config.satelliteLayers[i - 1] = config.satelliteLayers[i];
                        config.satelliteLayers[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == config.satelliteLayers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.downArrow.ToString()))
                {
                    Config3DSatellite temp = null;
                    if (i < (config.satelliteLayers.Count - 1))
                    {
                        temp = config.satelliteLayers[i + 1];
                        config.satelliteLayers[i + 1] = config.satelliteLayers[i];
                        config.satelliteLayers[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                //if (GUILayout.Button(SkyboxEditorLiterals.sign_remove))
                //{
                //    config.satelliteLayers.RemoveAt(i);
                //    break;
                //}

                Color circleColor = Color.green;
                switch (config.satelliteLayers[i].renderType)
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
                    list.Add(config.satelliteLayers);
                    list.Add(i);

                    // Anonymous method for delete operation
                    GenericMenu.MenuFunction2 deleteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DSatellite> satellites = list[0] as List<Config3DSatellite>;
                        int index = (int)list[1];
                        satellites.RemoveAt(index);
                    };

                    // Anonymous method for Add operation
                    GenericMenu.MenuFunction2 addBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DSatellite> satellites = list[0] as List<Config3DSatellite>;
                        int index = (int)list[1];
                        satellites.Insert(index + 1, new Config3DSatellite("Satellite " + (satellites.Count + 1)));
                    };

                    // Anonymous method for copy layer
                    GenericMenu.MenuFunction2 copyBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DSatellite> satellites = list[0] as List<Config3DSatellite>;
                        int index = (int)list[1];
                        copyPasteObj.SetSatellite(satellites[index]);
                    };

                    // Anonymous method for Paste layer
                    GenericMenu.MenuFunction2 pasteBtnClicked = (object obj) =>
                    {
                        ArrayList list = obj as ArrayList;
                        List<Config3DSatellite> satellites = list[0] as List<Config3DSatellite>;
                        int index = (int)list[1];
                        satellites[index] = copyPasteObj.GetCopiedSatellite().DeepCopy();
                    };
                    ShowSatelliteContextMenu(copyPasteObj, deleteBtnClicked, addBtnClicked, copyBtnClicked, pasteBtnClicked, list);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            }
            Rect r = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(r.width - 35, r.y, 25, 25), SkyboxEditorLiterals.Characters.sign_add))
            {
                config.satelliteLayers.Add(new Config3DSatellite("Satellite " + (config.satelliteLayers.Count + 1)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(25);
        }

        private static void ShowSatelliteContextMenu(CopyFunctionality copyPasteObj, GenericMenu.MenuFunction2 OnDeleteBtnClicked, GenericMenu.MenuFunction2 OnAddBtnClicked, GenericMenu.MenuFunction2 OnCopyBtnClicked, GenericMenu.MenuFunction2 OnPasteBtnClicked, object list)
        {
            // Create menu
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add"), false, OnAddBtnClicked, list);

            // Copy option
            menu.AddItem(new GUIContent("Copy"), false, OnCopyBtnClicked, list);

            // Paste option
            if (copyPasteObj.IsSatelliteAvailable())
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