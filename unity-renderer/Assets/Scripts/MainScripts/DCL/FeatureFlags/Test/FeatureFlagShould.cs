using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FeatureFlagShould
{
    [SetUp]
    public void SetUp() { }

    [TearDown]
    public void TearDown() { }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AddFeaturesCorrectly(bool isEnabled)
    {
        //Arrange
        FeatureFlag featureFlag = new FeatureFlag();

        //Act
        featureFlag.flags.Add("Test", isEnabled);

        //Assert
        Assert.AreEqual(isEnabled, featureFlag.IsFeatureEnabled("Test"));
    }

    [Ignore("Bridge game object being null by design is bad")]
    [Test]
    public void AddBridgeWhenInitialized()
    {
        //Arrange
        GameObject bridgeGameObject = new GameObject("Bridges");
        FeatureFlagController controller = new FeatureFlagController(bridgeGameObject);
        GameObject newGameObject = new GameObject("Test");

        //Act
        controller.AddBridgeComponent(newGameObject);

        //Assert
        Assert.IsNotNull(controller.featureFlagBridgeComponent);
        Assert.IsNotNull(newGameObject.GetComponent<FeatureFlagBridge>());
        Object.Destroy(newGameObject);
        Object.Destroy(bridgeGameObject);
    }

    [Test]
    public void BeDisposedCorrectly()
    {
        //Arrange
        var bridges = new GameObject("DisposeBridge");
        FeatureFlagController controller = new FeatureFlagController(bridges);

        //Act
        controller.Dispose();

        //Assert
        Assert.IsTrue(controller.featureFlagBridgeComponent == null);
        Object.DestroyImmediate(bridges);
    }

    [Test]
    public void ApplyFeatureFlagWhenBridgeReceivesUpdateMessage()
    {
        //Arrange
        GameObject newGameObject = new GameObject("Test");
        FeatureFlagBridge bridge = newGameObject.AddComponent<FeatureFlagBridge>();
        FeatureFlag config = TestUtils.CreateFeatureFlag();
        DataStore.i.featureFlags.flags.OnChange += FlagConfigReceived;
        var featureFlagReceived = false;

        void FlagConfigReceived(FeatureFlag newConfig, FeatureFlag oldConfig) => featureFlagReceived = true;

        //Act
        bridge.SetFeatureFlagConfiguration(config);

        //Assert
        Assert.IsTrue(featureFlagReceived);
    }
}