using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderSatellite3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            config.satelliteLayer.nameInEditor = EditorGUILayout.TextField(config.satelliteLayer.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();


            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.timeSpan, SkyboxEditorLiterals.start, ref config.satelliteLayer.timeSpan_start, SkyboxEditorLiterals.end, ref config.satelliteLayer.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref config.satelliteLayer.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref config.satelliteLayer.timeSpan_End);

            // Fading
            //RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.fade, SkyboxEditorLiterals.inStr, ref layer.fadingInTime, SkyboxEditorLiterals.outStr, ref layer.fadingOutTime);

            //// Tint
            //RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.tint, ref layer.tintPercentage, 0, 100);

            RenderSimpleValues.RenderPrefabInput("Prefab", ref config.satelliteLayer.satellite);
            RenderSimpleValues.RenderFloatField("Size", ref config.satelliteLayer.satelliteSize);
            RenderSimpleValues.RenderFloatField("Radius", ref config.satelliteLayer.radius);
            RenderSimpleValues.RenderFloatFieldAsSlider("Initial Pos", ref config.satelliteLayer.initialAngle, 0, 360);
            RenderSimpleValues.RenderFloatFieldAsSlider("Horizon Plane", ref config.satelliteLayer.horizonPlaneRotation, 0, 180);
            RenderSimpleValues.RenderFloatFieldAsSlider("Inclination", ref config.satelliteLayer.inclination, 0, 180);
            RenderSimpleValues.RenderFloatField("Speed", ref config.satelliteLayer.movementSpeed);
            RenderSimpleValues.RenderEnumPopup<RotationType>("Rotation Type", ref config.satelliteLayer.satelliteRotation);

            // If fixed rotation
            if (config.satelliteLayer.satelliteRotation == RotationType.Fixed)
            {
                RenderSimpleValues.RenderVector3Field("Rotation", ref config.satelliteLayer.fixedRotation);
            }
            else if (config.satelliteLayer.satelliteRotation == RotationType.Rotate)
            {
                RenderSimpleValues.RenderVector3Field("Axis", ref config.satelliteLayer.rotateAroundAxis);
                RenderSimpleValues.RenderFloatField("Rotation Speed", ref config.satelliteLayer.rotateSpeed);
            }
        }
    }
}