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
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.domeName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.nameInEditor = EditorGUILayout.TextField(layer.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Dome Size
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Layers.domeSize, ref dome.domeRadius);
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

            switch (layer.layerType)
            {
                case LayerType.Cubemap:
                    RenderCubemapLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
                    break;
                case LayerType.Planar:
                    RenderPlanarLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
                    break;
                case LayerType.Radial:
                    RenderPlanarLayer.RenderLayer(ref timeOfTheDay, toolSize, layer, true);
                    break;
                case LayerType.Satellite:
                    RenderSatelliteLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
                    break;
                case LayerType.Particles:
                    RenderParticleLayer.RenderLayer(ref timeOfTheDay, toolSize, layer);
                    break;
            }
        }
    }
}