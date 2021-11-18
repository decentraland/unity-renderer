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

    // [Test]
    // public void TestFeatureControllerApplyConfig()
    // {
    //     //Arrange
    //     FeatureFlag currentConfig = TestUtils.CreateFeatureFlag();
    //     PluginSystem pluginSystem = new PluginSystem();
    //
    //     //Act
    //     pluginSystem.ApplyFeaturesConfig(currentConfig);
    //
    //     //Assert
    //     Assert.AreSame(pluginSystem.GetCurrentConfig(), currentConfig);
    // }
    //
    // [Test]
    // public void TestFeatureControllerConfigChange()
    // {
    //     //Arrange
    //     FeatureFlag oldConfig = TestUtils.CreateFeatureFlag();
    //     FeatureFlag newConfig = TestUtils.CreateFeatureFlag();
    //     PluginSystem pluginSystem = new PluginSystem();
    //     pluginSystem.ApplyFeaturesConfig(oldConfig);
    //
    //     //Act
    //     pluginSystem.ApplyFeaturesConfig(newConfig);
    //
    //     //Assert
    //     Assert.AreSame(pluginSystem.GetCurrentConfig(), newConfig);
    // }
}