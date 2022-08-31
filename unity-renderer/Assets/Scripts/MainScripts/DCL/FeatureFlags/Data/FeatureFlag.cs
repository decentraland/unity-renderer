using System.Collections.Generic;

public class FeatureFlag
{
    public readonly Dictionary<string, bool> flags  = new Dictionary<string, bool>();
    public readonly Dictionary<string, FeatureFlagVariant> variants  = new Dictionary<string, FeatureFlagVariant>();
    
    public bool IsFeatureEnabled(string featureName)
    {
        if (variants.TryGetValue(featureName, out var variant) && variant.enabled)
        {
            //We use name property, to determine what A/B test variant is used
            //Currently for simplicity we supposed enabled/disabled name 
            return variant.name == "enabled";
        }
        
        if (flags.ContainsKey(featureName))
            return flags[featureName];

        return false;
    }
   
    public string ToString()
    {
        string result = "";

        foreach ( var flag in flags )
        {
            result += $"{flag.Key}: {flag.Value}\n";
        }

        return result;
    }
}

public class FeatureFlagVariant
{
    public string name;
    public bool enabled;
    public FeatureFlagVariantPayload payload;
}

public class FeatureFlagVariantPayload
{
    public string type;
    public string value;
}