using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeatureFlag
{
    public const string GPU_ONLY_MESHES = "use-gpu-only-meshes";

    public readonly Dictionary<string, bool> flags  = new Dictionary<string, bool>();
    public readonly Dictionary<string, FeatureFlagVariant> variants  = new Dictionary<string, FeatureFlagVariant>();

    public bool IsFeatureEnabled(string featureName)
    {
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
    private string name;
    private bool enable;
    private Payload payload;

    struct Payload
    {
        private string type;
        private string value;
    }
}