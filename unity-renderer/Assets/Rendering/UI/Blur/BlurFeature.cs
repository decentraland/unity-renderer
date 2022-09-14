using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurFeature : IPlugin
{
    public BlurFeature()
    {
        (GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset).ToggleRenderFeature<GaussianBlurHandler>(true);
    }

    public void Dispose()
    {
        (GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset).ToggleRenderFeature<GaussianBlurHandler>(false);
    }
}