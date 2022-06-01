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
            RenderSimpleValues.RenderTexture(SkyboxEditorLiterals.LayerProperties.texture, ref layer.texture);

            // Row and Coloumns
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.LayerProperties.rowsColumns, ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.animSpeed, ref layer.flipbookAnimSpeed);

            // Gradient
            RenderSimpleValues.RenderColorGradientField(layer.color, SkyboxEditorLiterals.LayerProperties.color, layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.LayerProperties.tiling, ref layer.particleTiling);

            // Offset
            RenderSimpleValues.RenderVector2Field(SkyboxEditorLiterals.LayerProperties.offset, ref layer.particlesOffset);

            // Amount
            RenderSimpleValues.RenderFloatField(SkyboxEditorLiterals.LayerProperties.amount, ref layer.particlesAmount);

            // Size
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.size, SkyboxEditorLiterals.LayerProperties.min, ref layer.particleMinSize, SkyboxEditorLiterals.LayerProperties.max, ref layer.particleMaxSize);

            // Spread
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.spread, SkyboxEditorLiterals.LayerProperties.horizontal, ref layer.particlesHorizontalSpread, SkyboxEditorLiterals.LayerProperties.vertical, ref layer.particlesVerticalSpread);

            // Fade
            RenderSimpleValues.RenderSepratedFloatFields(SkyboxEditorLiterals.LayerProperties.fade, SkyboxEditorLiterals.LayerProperties.min, ref layer.particleMinFade, SkyboxEditorLiterals.LayerProperties.max, ref layer.particleMaxFade);

            // Particle Rotation
            RenderTransitioningVariables.RenderTransitioningVector3(ref timeOfTheDay, layer.particleRotation, SkyboxEditorLiterals.LayerProperties.rotation, SkyboxEditorLiterals.LayerProperties.percentage, SkyboxEditorLiterals.LayerProperties.value, layer.timeSpan_start, layer.timeSpan_End);
        }
    }
}