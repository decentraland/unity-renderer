using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderSatellite3DLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, Config3DSatellite config)
        {

            // name In Editor
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.layerName, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            config.nameInEditor = EditorGUILayout.TextField(config.nameInEditor, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Time Span
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.timeSpan, SkyboxEditorLiterals.LayerProperties.start, ref config.timeSpanStart, SkyboxEditorLiterals.LayerProperties.end, ref config.timeSpanEnd);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpanStart);
            SkyboxEditorUtils.ClampToDayTime(ref config.timeSpanEnd);

            // Fading
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.fade, SkyboxEditorLiterals.LayerProperties.inStr, ref config.fadeInTime, SkyboxEditorLiterals.LayerProperties.outStr, ref config.fadeOutTime);

            RenderSimpleValues.RenderPrefabInput(SkyboxEditorLiterals.Satellite3D.PREFAB, ref config.satellite);
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Satellite3D.SIZE, ref config.satelliteSize);
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Satellite3D.RADIUS, ref config.radius);
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.Satellite3D.Y_OFFSET, ref config.orbitYOffset);
            RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.Satellite3D.INITIAL_POS, ref config.initialAngle, 0, 360);
            RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.Satellite3D.HORIZON_PLANE, ref config.horizonPlaneRotation, 0, 180);
            RenderSimpleValues.RenderFloatFieldAsSlider(SkyboxEditorLiterals.Satellite3D.INCLINATION, ref config.inclination, 0, 180);
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.speed, ref config.movementSpeed);
            RenderSimpleValues.RenderEnumPopup(SkyboxEditorLiterals.Satellite3D.ROTATION_TYPE, ref config.satelliteRotation);

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