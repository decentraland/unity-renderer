using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using Tests;
using UnityEngine;
using Tests;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PluginSystemShould
{
    public interface Plugin1 : IPlugin {}
    public interface Plugin2 : IPlugin {}
    public interface Plugin3 : Plugin2 {}
    public interface Plugin4 : Plugin3 {}
    public interface Plugin5 : Plugin4 {}
    public interface Plugin6 : Plugin5 {}
    
    [Test]
    public void EnablePluginWhenFlagIsSetBeforeAddingIt()
    {
        var featureFlags = new FeatureFlag();
        featureFlags.flags.Add("test-flag", true);

        var flagData = new BaseVariable<FeatureFlag>();
        flagData.Set(featureFlags);

        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<Plugin1>();

        pluginSystem.RegisterWithFlag<Plugin1>(pluginBuilder, "test-flag");
        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.False);

        pluginSystem.SetFeatureFlagsData(flagData);

        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.True);
        pluginSystem.Dispose();
    }

    [Test]
    public void EnablePluginWhenFlagIsSetAfterAddingIt()
    {
        var flagData = new BaseVariable<FeatureFlag>(new FeatureFlag());

        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<Plugin1>();

        pluginSystem.RegisterWithFlag<Plugin1>(pluginBuilder, "test-flag");
        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.False);

        pluginSystem.SetFeatureFlagsData(flagData);

        var newFF = new FeatureFlag();
        newFF.flags.Add("test-flag", true);
        flagData.Set(newFF);

        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.True);
        pluginSystem.Dispose();
    }

    [Test]
    public void EnablePluginWhenIsAddedWithoutFlag()
    {
        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<Plugin1>();

        pluginSystem.Register<Plugin1>(pluginBuilder);
        pluginSystem.Initialize();
        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.True);
        pluginSystem.Dispose();
    }

    [Test]
    public void SupportManyPluginsSharingTheSameFlag()
    {
        var featureFlags = new FeatureFlag();
        featureFlags.flags.Add("test-flag-1", true);

        var flagData = new BaseVariable<FeatureFlag>(featureFlags);

        var pluginSystem = new PluginSystem();

        PluginBuilder pluginBuilder1 = () => Substitute.For<Plugin1>();
        PluginBuilder pluginBuilder2 = () => Substitute.For<Plugin2>();
        PluginBuilder pluginBuilder3 = () => Substitute.For<Plugin3>();
        PluginBuilder pluginBuilder4 = () => Substitute.For<Plugin4>();
        PluginBuilder pluginBuilder5 = () => Substitute.For<Plugin5>();
        PluginBuilder pluginBuilder6 = () => Substitute.For<Plugin6>();

        pluginSystem.RegisterWithFlag<Plugin1>(pluginBuilder1, "test-flag-1");
        pluginSystem.RegisterWithFlag<Plugin2>(pluginBuilder2, "test-flag-1");
        pluginSystem.RegisterWithFlag<Plugin3>(pluginBuilder3, "test-flag-1");
        pluginSystem.RegisterWithFlag<Plugin4>(pluginBuilder4, "test-flag-2");
        pluginSystem.RegisterWithFlag<Plugin5>(pluginBuilder5, "test-flag-2");
        pluginSystem.RegisterWithFlag<Plugin6>(pluginBuilder6, "test-flag-2");
        pluginSystem.Initialize();

        pluginSystem.SetFeatureFlagsData(flagData);

        Assert.That(pluginSystem.IsEnabled<Plugin1>(), Is.True);
        Assert.That(pluginSystem.IsEnabled<Plugin2>(), Is.True);
        Assert.That(pluginSystem.IsEnabled<Plugin3>(), Is.True);
        Assert.That(pluginSystem.IsEnabled<Plugin4>(), Is.False);
        Assert.That(pluginSystem.IsEnabled<Plugin5>(), Is.False);
        Assert.That(pluginSystem.IsEnabled<Plugin6>(), Is.False);

        var newFF = new FeatureFlag();
        newFF.flags.Add("test-flag-2", true);
        flagData.Set(newFF);

        Assert.That(pluginSystem.IsEnabled<Plugin4>(), Is.True);
        Assert.That(pluginSystem.IsEnabled<Plugin5>(), Is.True);
        Assert.That(pluginSystem.IsEnabled<Plugin6>(), Is.True);
        pluginSystem.Dispose();
    }

    [Test]
    public void NotCrashWhenAnUnconfiguredFlagIsSet()
    {
        var featureFlags = new FeatureFlag();
        featureFlags.flags.Add("this-flag-is-not-bound-to-a-plugin", true);

        var flagData = new BaseVariable<FeatureFlag>(featureFlags);

        var pluginSystem = new PluginSystem();
        pluginSystem.SetFeatureFlagsData(flagData);
        pluginSystem.Dispose();
    }

    [Test]
    public void CreateAndDisposePluginWhenApplicable()
    {
        var pluginSystem = new PluginSystem();
        pluginSystem.Dispose();
    }

    [Test]
    public void BeDisposedProperly()
    {
        var pluginSystem = new PluginSystem();
        pluginSystem.Dispose();
    }

    [Test]
    public void OverrideRegister()
    {
        var pluginSystem = new PluginSystem();

        PluginBuilder pluginBuilder1 = () => Substitute.For<Plugin1>();
        PluginBuilder pluginBuilder2 = () => Substitute.For<Plugin2>();

        pluginSystem.Register<Plugin1>(pluginBuilder1);
        pluginSystem.Register<Plugin1>(pluginBuilder2);
        pluginSystem.Initialize();

        Type type = typeof(Plugin1);
        PluginInfo pluginInfo;
        bool hasValue = pluginSystem.allPlugins.plugins.TryGetValue(type, out pluginInfo);
        Assert.That(hasValue, Is.True);
        Assert.That(pluginInfo != null, Is.True);
        Assert.That(pluginInfo.builder == pluginBuilder2, Is.True);
        pluginSystem.Dispose();
    }
}