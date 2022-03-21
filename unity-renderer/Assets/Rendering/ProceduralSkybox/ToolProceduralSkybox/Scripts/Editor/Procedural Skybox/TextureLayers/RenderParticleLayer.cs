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
            RenderSimpleValues.RenderTexture(SkyboxEditorLiterals.texture, ref layer.texture);

            // Row and Coloumns
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.rowsColumns, ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.animSpeed, ref layer.flipbookAnimSpeed);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, SkyboxEditorLiterals.color, layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.tiling, ref layer.particleTiling);

            // Offset
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.offset, ref layer.particlesOffset);

            // Amount
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.amount, ref layer.particlesAmount);

            // Size
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.size, SkyboxEditorLiterals.min, ref layer.particleMinSize, SkyboxEditorLiterals.max, ref layer.particleMaxSize);

            // Spread
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.spread, SkyboxEditorLiterals.horizontal, ref layer.particlesHorizontalSpread, SkyboxEditorLiterals.vertical, ref layer.particlesVerticalSpread);

            // Fade
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.fade, SkyboxEditorLiterals.min, ref layer.particleMinFade, SkyboxEditorLiterals.max, ref layer.particleMaxFade);

            // Particle Rotation
            RenderTransitioningVariables.RenderTransitioningVector3(ref timeOfTheDay, layer.particleRotation, SkyboxEditorLiterals.rotation, SkyboxEditorLiterals.percentage, SkyboxEditorLiterals.value, layer.timeSpan_start, layer.timeSpan_End);
        }
    }
}