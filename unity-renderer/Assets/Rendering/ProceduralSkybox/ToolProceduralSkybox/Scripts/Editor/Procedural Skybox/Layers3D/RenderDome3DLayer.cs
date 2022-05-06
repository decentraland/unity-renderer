using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderDome3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, Config3DDome dome)
        {
            TextureLayer layer = dome.layers;
            EditorGUILayout.Separator();

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.domeName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.nameInEditor = EditorGUILayout.TextField(layer.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Dome Size
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.domeSize, ref dome.domeRadius);
            EditorGUILayout.Separator();

            // Layer Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(SkyboxEditorLiterals.layerType, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.layerType = (LayerType)EditorGUILayout.EnumPopup(layer.layerType, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.timeSpan, SkyboxEditorLiterals.start, ref layer.timeSpan_start, SkyboxEditorLiterals.end, ref layer.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref layer.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref layer.timeSpan_End);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.fade, SkyboxEditorLiterals.inStr, ref layer.fadingInTime, SkyboxEditorLiterals.outStr, ref layer.fadingOutTime);

            // Tint
            RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.tint, ref layer.tintPercentage, 0, 100);

            if (layer.layerType == LayerType.Cubemap)
            {
                RenderCubemapLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);

            }
            else if (layer.layerType == LayerType.Planar)
            {
                RenderPlanarLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);

            }
            else if (layer.layerType == LayerType.Radial)
            {
                RenderPlanarLayer.RenderLayer(ref timeOfTheDay, toolSize, layer, true);
            }
            else if (layer.layerType == LayerType.Satellite)
            {
                RenderSatelliteLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
            }
            else if (layer.layerType == LayerType.Particles)
            {
                RenderParticleLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
            }
        }
    }
}