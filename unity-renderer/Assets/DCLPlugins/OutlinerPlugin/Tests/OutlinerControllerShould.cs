// unset:none
using DCL;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlinerControllerShould
{
    [Test]
    public void PrepareTheRenderFeatures()
    {
        // Arrange
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
        var maskFeature = scriptableRendererData?.rendererFeatures.OfType<OutlineMaskFeature>().First();
        var screenFeature = scriptableRendererData?.rendererFeatures.OfType<OutlineScreenEffectFeature>().First();
        maskFeature.SetActive(false);
        screenFeature.SetActive(false);

        // Act
        var outlinerController = new OutlinerController(new DataStore_Outliner(), ScriptableObject.CreateInstance<OutlineRenderersSO>());

        // Assert
        Assert.IsTrue(maskFeature.isActive);
        Assert.IsTrue(screenFeature.isActive);
        outlinerController.Dispose();
    }
}
