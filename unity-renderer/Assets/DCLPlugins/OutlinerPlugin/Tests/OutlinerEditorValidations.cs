using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlinerEditorValidations
{
    [Test]
    public void OutlinerRenderFeaturesAreDisabled()
    {
        // Arrange
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
        var maskFeature = scriptableRendererData?.rendererFeatures.OfType<OutlineMaskFeature>().First();
        var screenFeature = scriptableRendererData?.rendererFeatures.OfType<OutlineScreenEffectFeature>().First();

        // Assert
        Assert.NotNull(maskFeature, "The Outliner Mask Feature is missing in the ForwardRender asset");
        Assert.NotNull(screenFeature, "The Outliner Screen Effect Feature is missing in the ForwardRender asset");
        Assert.IsFalse(maskFeature.isActive, "Outliner Mask Feature is enabled in the ForwardRender asset, this should be done in runtime by the feature flag");
        Assert.IsFalse(screenFeature.isActive, "Outliner Screen Effect Feature is enabled in the ForwardRender asset, this should be done in runtime by the feature flag");
    }
}
