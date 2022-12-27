using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class OutlinerEditorValidations
{
    [Test]
    public void OutlinerRenderFeaturesAreDisabled()
    {
        // Arrange
        var maskFeature = Resources.FindObjectsOfTypeAll<OutlineMaskFeature>().FirstOrDefault();
        var screenFeature = Resources.FindObjectsOfTypeAll<OutlineScreenEffectFeature>().FirstOrDefault();

        // Assert
        Assert.NotNull(maskFeature, "The Outliner Mask Feature is missing in the ForwardRender asset");
        Assert.NotNull(screenFeature, "The Outliner Screen Effect Feature is missing in the ForwardRender asset");
        Assert.IsFalse(maskFeature.isActive, "Outliner Mask Feature is enabled in the ForwardRender asset, this should be done in runtime by the feature flag");
        Assert.IsFalse(screenFeature.isActive, "Outliner Screen Effect Feature is enabled in the ForwardRender asset, this should be done in runtime by the feature flag");
    }
}
