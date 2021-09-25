using System.Collections;
using System.Collections.Generic;
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
        PluginSystem pluginSystem = new PluginSystem();
        KernelConfigModel currentConfig = new KernelConfigModel();
        currentConfig.features.enableTutorial = false;

        //Act
        pluginSystem.ApplyFeaturesConfig(currentConfig);

        //Assert
        Assert.AreSame(pluginSystem.GetCurrentConfig(), currentConfig);
    }

    [Test]
    public void TestFeatureControllerConfigChange()
    {
        //Arrange
        PluginSystem pluginSystem = new PluginSystem();
        KernelConfigModel currentConfig = new KernelConfigModel();
        currentConfig.features.enableTutorial = false;
        KernelConfigModel oldConfig = new KernelConfigModel();
        oldConfig.features.enableTutorial = false;
        pluginSystem.ApplyFeaturesConfig(oldConfig);

        //Act
        pluginSystem.OnKernelConfigChanged(currentConfig, oldConfig);

        //Assert
        Assert.AreSame(pluginSystem.GetCurrentConfig(), currentConfig);
    }
}