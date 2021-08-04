using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;
using Tests;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PluginSystemShould : IntegrationTestSuite
{

    [Test]
    public void TestFeatureControllerApplyConfig()
    {
        //Arrange
        PluginSystem pluginSystem = new PluginSystem();
        KernelConfigModel currentConfig = new KernelConfigModel();

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
        KernelConfigModel oldConfig = new KernelConfigModel();
        pluginSystem.ApplyFeaturesConfig(oldConfig);

        //Act
        pluginSystem.OnKernelConfigChanged(currentConfig, oldConfig);

        //Assert
        Assert.AreSame(pluginSystem.GetCurrentConfig(), currentConfig);
    }
}