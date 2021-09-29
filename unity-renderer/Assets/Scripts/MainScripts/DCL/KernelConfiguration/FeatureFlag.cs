using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureFlag
{
    public Dictionary<string, bool> flags;
    public Dictionary<string, FeatureFlagVariant> variants;
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