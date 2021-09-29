using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using Tests;
using UnityEngine;
using Tests;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PluginSystemShould : IntegrationTestSuite
{
    private GameObject main;

    [SetUp]
    public void Setup() { main = new GameObject("Main"); }

    [TearDown]
    public void TearDown() { Object.DestroyImmediate(main); }

    [Test]
    public void TestFeatureControllerApplyConfig()
    {
        //Arrange
        FeatureFlagConfiguration currentConfig = TestHelpers.CreatetFeatureFlagConfiguration();
        PluginSystem pluginSystem = new PluginSystem(currentConfig);

        //Act
        pluginSystem.ApplyFeaturesConfig(currentConfig);

        //Assert
        Assert.AreSame(pluginSystem.GetCurrentConfig(), currentConfig);
    }

    [Test]
    public void TestFeatureControllerConfigChange()
    {
        //Arrange
        FeatureFlagConfiguration oldConfig = TestHelpers.CreatetFeatureFlagConfiguration();
        FeatureFlagConfiguration newConfig = TestHelpers.CreatetFeatureFlagConfiguration();
        PluginSystem pluginSystem = new PluginSystem(oldConfig);

        //Act
        pluginSystem.ApplyFeaturesConfig(newConfig);

        //Assert
        Assert.AreSame(pluginSystem.GetCurrentConfig(), newConfig);
    }
}