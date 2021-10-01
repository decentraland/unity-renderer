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
    public void IsFeatureEnable(bool isEnabled)
    {
        //Arrange
        FeatureFlag featureFlag = new FeatureFlag();

        //Act
        featureFlag.flags.Add("Test", isEnabled);

        //Assert
        Assert.AreEqual(isEnabled, featureFlag.IsFeatureEnabled("Test"));
    }

    [Test]
    public void FeatureFlagControllerAddBridgeCorrectly()
    {
        //Arrange
        FeatureFlagController controller = new FeatureFlagController();
        GameObject newGameObject = new GameObject("Test");

        //Act
        controller.AddBridgeComponent(newGameObject);

        //Assert
        Assert.IsNotNull(controller.featureFlagBridgeComponent);
        Assert.IsNotNull(newGameObject.GetComponent<FeatureFlagBridge>());
        GameObject.Destroy(newGameObject);
    }

    [Test]
    public void FeatureFlagControllerDisposeCorrectly()
    {
        //Arrange
        FeatureFlagController controller = new FeatureFlagController();

        //Act
        controller.Dispose();

        //Assert
        Assert.IsNull(controller.featureFlagBridgeComponent);
    }

    [Test]
    public void FeatureFlagApplyCorrectly()
    {
        //Arrange
        GameObject newGameObject = new GameObject("Test");
        FeatureFlagBridge bridge = newGameObject.AddComponent<FeatureFlagBridge>();
        FeatureFlag confifg = TestHelpers.CreatetFeatureFlag();
        DataStore.i.featureFlags.featureFlags.OnChange += FeatureFlagConfigReceived;
        featureFlagReceived = false;

        //Act
        bridge.SetFeatureFlagConfiguration(confifg);

        //Assert
        Assert.IsTrue(featureFlagReceived);
    }

    private bool featureFlagReceived = false;
    private void FeatureFlagConfigReceived(FeatureFlag newConfig, FeatureFlag oldConfig) { featureFlagReceived = true; }

}