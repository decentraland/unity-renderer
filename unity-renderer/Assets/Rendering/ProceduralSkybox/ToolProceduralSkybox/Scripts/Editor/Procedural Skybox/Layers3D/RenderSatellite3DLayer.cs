using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderSatellite3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, Satellite3DLayer config)
        {

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            config.nameInEditor = EditorGUILayout.TextField(config.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.timeSpan, SkyboxEditorLiterals.start, ref config.timeSpan_start, SkyboxEditorLiterals.end, ref config.timeSpan_End);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_start);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpan_End);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.fade, SkyboxEditorLiterals.inStr, ref config.fadeInTime, SkyboxEditorLiterals.outStr, ref config.fadeOutTime);

            //// Tint
            //RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.tint, ref layer.tintPercentage, 0, 100);

            RenderSimpleValues.RenderPrefabInput("Prefab", ref config.satellite);
            RenderSimpleValues.RenderFloatField("Size", ref config.satelliteSize);
            RenderSimpleValues.RenderFloatField("Radius", ref config.radius);
            RenderSimpleValues.RenderFloatFieldAsSlider("Initial Pos", ref config.initialAngle, 0, 360);
            RenderSimpleValues.RenderFloatFieldAsSlider("Horizon Plane", ref config.horizonPlaneRotation, 0, 180);
            RenderSimpleValues.RenderFloatFieldAsSlider("Inclination", ref config.inclination, 0, 180);
            RenderSimpleValues.RenderFloatField("Speed", ref config.movementSpeed);
            RenderSimpleValues.RenderEnumPopup<RotationType>("Rotation Type", ref config.satelliteRotation);

            // If fixed rotation
            if (config.satelliteRotation == RotationType.Fixed)
            {
                RenderSimpleValues.RenderVector3Field("Rotation", ref config.fixedRotation);
            }
            else if (config.satelliteRotation == RotationType.Rotate)
            {
                RenderSimpleValues.RenderVector3Field("Axis", ref config.rotateAroundAxis);
                RenderSimpleValues.RenderFloatField("Rotation Speed", ref config.rotateSpeed);
            }
        }
    }
}