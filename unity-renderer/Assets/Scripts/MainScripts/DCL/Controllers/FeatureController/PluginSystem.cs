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
    private List<PluginFeature> activeFeatures = new List<PluginFeature>();

    private FeatureFlagConfiguration currentConfig;

    public FeatureFlagConfiguration GetCurrentConfig() { return currentConfig; }

    public PluginSystem(string featureConfigJson)
    {
        FeatureFlagConfiguration configuration = new FeatureFlagConfiguration(featureConfigJson);
        ApplyFeaturesConfig(configuration);
    }

    public PluginSystem(FeatureFlagConfiguration configuration) { ApplyFeaturesConfig(configuration); }

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
        foreach (PluginFeature feature in activeFeatures)
        {
            feature.Dispose();
        }
    }

    public void ApplyFeaturesConfig(FeatureFlagConfiguration config)
    {
        HandleFeature<BuilderInWorld>(config.IsFeatureEnabled("builder_in_world"));
        HandleFeature<TutorialController>(config.IsFeatureEnabled("tutorial"));
        HandleFeature<DebugPluginFeature>(true);
        HandleFeature<ExploreV2Feature>(config.IsFeatureEnabled("exploreV2"));
        currentConfig = config;
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