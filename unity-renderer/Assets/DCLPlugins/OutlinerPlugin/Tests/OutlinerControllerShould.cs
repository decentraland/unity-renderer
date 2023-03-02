using DCL;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class OutlinerControllerShould
{
    [Test]
    public void PrepareTheRenderFeatures()
    {
        // Arrange
        var maskFeature = Resources.FindObjectsOfTypeAll<OutlineMaskFeature>().FirstOrDefault();
        maskFeature.SetActive(false);
        var screenFeature = Resources.FindObjectsOfTypeAll<OutlineScreenEffectFeature>().FirstOrDefault();
        screenFeature.SetActive(false);

        // Act
        var outlinerController = new OutlinerController(new DataStore_Outliner(), ScriptableObject.CreateInstance<OutlineRenderersSO>());

        // Assert
        Assert.IsTrue(maskFeature.isActive);
        Assert.IsTrue(screenFeature.isActive);
        outlinerController.Dispose();
    }
}
