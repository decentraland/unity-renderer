using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class RenderHorizonPlane
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, SkyboxConfiguration config)
        {
            // Horizon Plane
            RenderSimpleValues.RenderTexture("Texture", ref config.horizonPlaneTexture);

            // Horizon Plane values
            EditorGUILayout.LabelField("Horizon Plane Values", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            // Tiling
            RenderSimpleValues.RenderVector2Field("Tiling", ref config.horizonPlaneTiling);
            // Offset
            RenderSimpleValues.RenderVector2Field("Offset", ref config.horizonPlaneOffset);
            EditorGUI.indentLevel--;

            // Horizon Plane color
            RenderSimpleValues.RenderColorGradientField(config.horizonPlaneColor, "Horizon Plane Color", 0, 24);

            // Horizon light intensity
            RenderSimpleValues.RenderFloatFieldAsSlider("Light Intensity", ref config.horizonLightIntensity, 0, 1);

            // Horizon Height
            RenderTransitioningVariables.RenderTransitioningFloat(toolSize, ref timeOfTheDay, config.horizonPlaneHeight, "Horizon Plane Height", "%", "value", true, -1, 0);

            EditorGUILayout.Space(10);

            // Plane smooth range
            RenderSimpleValues.RenderMinMaxSlider("Plane Smoothness", ref config.horizonPlaneSmoothRange.x, ref config.horizonPlaneSmoothRange.y, 0, 1);
        }
    }
}