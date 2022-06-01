using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderTextureLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
        {
            EditorGUILayout.Separator();

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.nameInEditor = EditorGUILayout.TextField(layer.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Layer Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.layerType, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.layerType = (LayerType)EditorGUILayout.EnumPopup(layer.layerType, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.timeSpan, SkyboxEditorLiterals.LayerProperties.start, ref layer.timeSpan_start, SkyboxEditorLiterals.LayerProperties.end, ref layer.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref layer.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref layer.timeSpan_End);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.fade, SkyboxEditorLiterals.LayerProperties.inStr, ref layer.fadingInTime, SkyboxEditorLiterals.LayerProperties.outStr, ref layer.fadingOutTime);

            // Tint
            RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.LayerProperties.tint, ref layer.tintPercentage, 0, 100);

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