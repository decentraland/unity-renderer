using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public static class RenderParticleLayer
    {
        public static void RenderLayer(ref float timeOfTheDay, EditorToolMeasurements toolSize, TextureLayer layer)
        {
            // Texture
            RenderSimpleValues.RenderTexture("Texture", ref layer.texture);

            // Row and Coloumns
            RenderSimpleValues.RenderVector2Field("Rows and Columns", ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderSimpleValues.RenderFloatField("Anim Speed", ref layer.flipbookAnimSpeed);

            // Normal Map
            RenderSimpleValues.RenderTexture("Normal Map", ref layer.textureNormal);

            // Normal Intensity
            RenderSimpleValues.RenderFloatFieldAsSlider("Normal Intensity", ref layer.normalIntensity, 0, 1);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderSimpleValues.RenderVector2Field("Tiling", ref layer.particleTiling);

            // Offset
            RenderSimpleValues.RenderVector2Field("Offset", ref layer.particlesOffset);

            // Amount
            RenderSimpleValues.RenderFloatField("Amount", ref layer.particlesAmount);

            // Size
            RenderSimpleValues.RenderSepratedFloatFields("Size", "Min", ref layer.particleMinSize, "Max", ref layer.particleMaxSize);

            // Spread
            RenderSimpleValues.RenderSepratedFloatFields("Spread", "Horizontal", ref layer.particlesHorizontalSpread, "Vertical", ref layer.particlesVerticalSpread);

            // Fade
            RenderSimpleValues.RenderSepratedFloatFields("Fade", "Min", ref layer.particleMinFade, "Max", ref layer.particleMaxFade);

            // Particle Rotation
            RenderTransitioningVariables.RenderTransitioningVector3(ref timeOfTheDay, layer.particleRotation, "Rotation", "%", "value", layer.timeSpan_start, layer.timeSpan_End);
        }
    }
}