using DCL;
using DCL.Tutorial;
using System;
using System.Collections.Generic;

/// <summary>
/// This class is used to handle a feature that needs the MonoBehaviour callbacks.
///
/// You need to add it as a feature toggle in gitlab so kernel will understand when to activate it and deactivate it.
/// After that, your feature manager should inherit from 'Feature' so it can start receiving MonoBehaviour callbacks where it is activated
/// </summary>
public class PluginSystem
{
    private readonly List<PluginFeature> activeFeatures = new List<PluginFeature>();

    private FeatureFlag currentConfig;

    public FeatureFlag GetCurrentConfig() { return currentConfig; }

    public PluginSystem()
    {
        DataStore.i.featureFlags.flags.OnChange += ApplyConfig;
        AddBasePlugins();
    }

    public void OnGUI()
    {
        foreach (PluginFeature feature in activeFeatures)
        {
            feature.OnGUI();
        }
    }

    public void Update()
    {
        foreach (PluginFeature feature in activeFeatures)
        {
            feature.Update();
        }
    }

    public void LateUpdate()
    {
        foreach (PluginFeature feature in activeFeatures)
        {
            feature.LateUpdate();
        }
    }

    public void OnDestroy()
    {
        DataStore.i.featureFlags.flags.OnChange -= ApplyConfig;

        foreach (PluginFeature feature in activeFeatures)
        {
            feature.Dispose();
        }
    }

    public void ApplyConfig(FeatureFlag newConfig, FeatureFlag oldConfig) { ApplyFeaturesConfig(newConfig); }

    public void AddBasePlugins() { HandleFeature<DebugPluginFeature>(true); }

    public void ApplyFeaturesConfig(FeatureFlag featureFlag)
    {
        HandleFeature<BuilderInWorldPlugin>(featureFlag.IsFeatureEnabled("builder_in_world"));
        HandleFeature<TutorialController>(featureFlag.IsFeatureEnabled("tutorial"));
        HandleFeature<ExploreV2Feature>(featureFlag.IsFeatureEnabled("explorev2"));
        HandleFeature<ShortcutsFeature>(true);
        currentConfig = featureFlag;
    }

    private void HandleFeature<T>(bool isActive) where T : PluginFeature, new ()
    {
        if (isActive)
            InitializeFeature<T>();
        else
            RemoveFeature<T>();
    }

    private void InitializeFeature<T>() where T : PluginFeature, new ()
    {
        for (int i = 0; i < activeFeatures.Count; i++)
        {
            if (activeFeatures[i].GetType() == typeof(T))
                return;
        }

        //NOTE: We should revise this code on the future, because we'd want to use custom constructors to DI.
        //      So here we most likely need to use an abstract factory, or just pass the new Feature object by argument.
        PluginFeature pluginFeature = new T();
        pluginFeature.Initialize();
        activeFeatures.Add(pluginFeature);
    }

    private void RemoveFeature<T>() where T : PluginFeature
    {
        for (int i = 0; i < activeFeatures.Count; i++)
        {
            if (activeFeatures[i].GetType() == typeof(T))
            {
                activeFeatures[i].Dispose();
                activeFeatures.Remove(activeFeatures[i]);
            }
        }
    }
}