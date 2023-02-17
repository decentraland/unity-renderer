using System;
using DCL;
using Newtonsoft.Json;
using UnityEngine;

public class FeatureFlagBridge : MonoBehaviour
{
    public void SetFeatureFlagConfiguration(string json)
    {
        FeatureFlag config = null;
        try
        {
            config = JsonConvert.DeserializeObject<FeatureFlag>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("FeatureFlag has been unable to parse the json! Error: " + e);
        }

        SetFeatureFlagConfiguration(config);
    }

    public void SetFeatureFlagConfiguration(FeatureFlag config)
    {
        if (config == null)
            return;

        config.SetAsInitialized();
        DataStore.i.featureFlags.flags.Set(config);
    }
}
