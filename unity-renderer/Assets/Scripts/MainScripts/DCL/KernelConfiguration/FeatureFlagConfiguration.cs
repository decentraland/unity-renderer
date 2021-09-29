using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureFlagConfiguration
{
    private FeatureFlag featureFlagConfig;
    public event Action<FeatureFlag> OnFeatureFlagConfigChange;

    public FeatureFlagConfiguration(string json)
    {
        try
        {
            var config = JsonUtility.FromJson<FeatureFlag>(json);
            SetConfig(config);
        }
        catch (Exception e)
        {
            Debug.LogError("FeatureFlagConfiguration has been unable to parse the json! " + e);
        }
    }

    public FeatureFlagConfiguration(FeatureFlag currentFlags) { SetConfig(currentFlags); }

    private void SetConfig(FeatureFlag featureFlagConfiguration)
    {
        featureFlagConfig = featureFlagConfiguration;

        OnFeatureFlagConfigChange?.Invoke(featureFlagConfig);
    }

    public bool IsFeatureEnabled(string featureName)
    {
        if (featureFlagConfig.flags.ContainsKey(featureName))
            return featureFlagConfig.flags[featureName];

        return false;
    }
}