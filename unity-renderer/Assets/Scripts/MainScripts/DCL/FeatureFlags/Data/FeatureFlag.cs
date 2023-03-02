using System.Collections.Generic;

public class FeatureFlag
{
    public readonly Dictionary<string, bool> flags  = new Dictionary<string, bool>();
    public readonly Dictionary<string, FeatureFlagVariant> variants  = new Dictionary<string, FeatureFlagVariant>();

    public bool IsInitialized { get; private set; }

    public void SetAsInitialized() =>
        IsInitialized = true;

        /// <summary>
    /// Will check if featureName contains variant to be checked.
    /// Supported formats: featureName, featureName:variantName
    /// </summary>
    /// <param name="featureNameLong"></param>
    /// <returns></returns>
    public bool IsFeatureEnabled(string featureNameLong)
    {
        string[] splitFeatureName = featureNameLong.Split(':');
        string featureName = splitFeatureName[0];

        if (splitFeatureName.Length > 1 && variants.TryGetValue(featureName, out var variant))
        {
            string variantName = splitFeatureName[splitFeatureName.Length - 1];
            return variant.enabled && variant.name == variantName;
        }

        if (flags.ContainsKey(featureName))
            return flags[featureName];

        return false;
    }

    public FeatureFlagVariantPayload GetFeatureFlagVariantPayload(string featureNameLong)
    {
        string[] splitFeatureName = featureNameLong.Split(':');
        string featureName = splitFeatureName[0];
        FeatureFlagVariantPayload payloadResult = null;

        if (splitFeatureName.Length > 1 && variants.TryGetValue(featureName, out var variant))
        {
            string variantName = splitFeatureName[splitFeatureName.Length - 1];
            if (variant.enabled && variant.name == variantName)
            {
                payloadResult = variant.payload;
            }
        }

        return payloadResult;
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
