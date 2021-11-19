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
    [Test]
    public void EnablePluginWhenFlagIsSetBeforeAddingIt()
    {
        var featureFlags = new FeatureFlag();
        featureFlags.flags.Add("test-flag", true);

        var flagData = new BaseVariable<FeatureFlag>();
        flagData.Set(featureFlags);

        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<IPlugin>();

        pluginSystem.RegisterWithFlag(pluginBuilder, "test-flag");
        Assert.That(pluginSystem.IsEnabled(pluginBuilder), Is.False);

        pluginSystem.SetFeatureFlagsData(flagData);

        Assert.That(pluginSystem.IsEnabled(pluginBuilder), Is.True);
    }

    [Test]
    public void EnablePluginWhenFlagIsSetAfterAddingIt()
    {
        var flagData = new BaseVariable<FeatureFlag>(new FeatureFlag());

        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<IPlugin>();

        pluginSystem.RegisterWithFlag(pluginBuilder, "test-flag");
        Assert.That(pluginSystem.IsEnabled(pluginBuilder), Is.False);

        pluginSystem.SetFeatureFlagsData(flagData);

        var newFF = new FeatureFlag();
        newFF.flags.Add("test-flag", true);
        flagData.Set(newFF);

        Assert.That(pluginSystem.IsEnabled(pluginBuilder), Is.True);
    }

    [Test]
    public void EnablePluginWhenIsAddedWithoutFlag()
    {
        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder = () => Substitute.For<IPlugin>();

        pluginSystem.Register(pluginBuilder);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder), Is.True);
    }

    [Test]
    public void SupportManyPluginsSharingTheSameFlag()
    {
        var featureFlags = new FeatureFlag();
        featureFlags.flags.Add("test-flag-1", true);

        var flagData = new BaseVariable<FeatureFlag>(featureFlags);

        var pluginSystem = new PluginSystem();
        PluginBuilder pluginBuilder1 = () => Substitute.For<IPlugin>();
        PluginBuilder pluginBuilder2 = () => Substitute.For<IPlugin>();
        PluginBuilder pluginBuilder3 = () => Substitute.For<IPlugin>();
        PluginBuilder pluginBuilder4 = () => Substitute.For<IPlugin>();
        PluginBuilder pluginBuilder5 = () => Substitute.For<IPlugin>();
        PluginBuilder pluginBuilder6 = () => Substitute.For<IPlugin>();

        pluginSystem.RegisterWithFlag(pluginBuilder1, "test-flag-1");
        pluginSystem.RegisterWithFlag(pluginBuilder2, "test-flag-1");
        pluginSystem.RegisterWithFlag(pluginBuilder3, "test-flag-1");
        pluginSystem.RegisterWithFlag(pluginBuilder4, "test-flag-2");
        pluginSystem.RegisterWithFlag(pluginBuilder5, "test-flag-2");
        pluginSystem.RegisterWithFlag(pluginBuilder6, "test-flag-2");

        pluginSystem.SetFeatureFlagsData(flagData);

        Assert.That(pluginSystem.IsEnabled(pluginBuilder1), Is.True);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder2), Is.True);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder3), Is.True);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder4), Is.False);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder5), Is.False);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder6), Is.False);

        var newFF = new FeatureFlag();
        newFF.flags.Add("test-flag-2", true);
        flagData.Set(newFF);

        Assert.That(pluginSystem.IsEnabled(pluginBuilder4), Is.True);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder5), Is.True);
        Assert.That(pluginSystem.IsEnabled(pluginBuilder6), Is.True);
    }

    [Test]
    public void CreateAndDisposePluginWhenApplicable()
    {
        var pluginSystem = new PluginSystem();
    }

    [Test]
    public void BeDisposedProperly()
    {
        var pluginSystem = new PluginSystem();
    }
}