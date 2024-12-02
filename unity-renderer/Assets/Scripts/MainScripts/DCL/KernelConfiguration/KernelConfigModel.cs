using KernelConfigurationTypes;
using System;
using System.Collections.Generic;

[Serializable]
public class KernelConfigModel
{
    private const string MAIN_NET = "mainnet";
    private const string ORG = ".org";
    private const string ZONE = ".zone";

    public Features features = new Features();
    public Comms comms = new Comms();
    public Profiles profiles = new Profiles();
    public bool gifSupported = false;
    public string network = MAIN_NET;
    public List<WorldRange> validWorldRanges = new List<WorldRange>();
    public string kernelVersion = string.Empty;
    public string rendererVersion = string.Empty;
    public Debugging debugConfig = new Debugging();
    public ProceduralSkybox proceduralSkyboxConfig = new ProceduralSkybox();
    public string avatarTextureAPIBaseUrl = string.Empty;
    public bool urlParamsForWearablesDebug = false;
    public string builderUrl;

    public override bool Equals(object obj) { return obj is KernelConfigModel other && Equals(other); }

    public bool Equals(KernelConfigModel other)
    {
        if (other == null)
            return false;

        if (validWorldRanges.Count != other.validWorldRanges.Count)
            return false;

        for (var i = 0; i < validWorldRanges.Count; i++)
        {
            if (!validWorldRanges[i].Equals(other.validWorldRanges[i]))
                return false;
        }

        return comms.Equals(other.comms)
               && profiles.Equals(other.profiles)
               && features.Equals(other.features)
               && gifSupported == other.gifSupported
               && network == other.network
               && kernelVersion == other.kernelVersion
               && rendererVersion == other.rendererVersion
               && debugConfig.Equals(other.debugConfig)
               && proceduralSkyboxConfig.Equals(other.proceduralSkyboxConfig)
               && avatarTextureAPIBaseUrl == other.avatarTextureAPIBaseUrl
               && urlParamsForWearablesDebug == other.urlParamsForWearablesDebug
               && builderUrl == other.builderUrl;
    }

    public string GetTld() =>
        network.Equals(MAIN_NET, StringComparison.OrdinalIgnoreCase) ? ORG : ZONE;

    public KernelConfigModel Clone()
    {
        // NOTE: We need to use deep clone
        KernelConfigModel clone = new KernelConfigModel();
        clone.comms = comms.Clone();
        clone.profiles = profiles.Clone();
        clone.features = features.Clone();
        clone.gifSupported = gifSupported;
        clone.network = network;
        clone.validWorldRanges = new List<WorldRange>(validWorldRanges);
        clone.kernelVersion = kernelVersion;
        clone.rendererVersion = rendererVersion;
        clone.debugConfig = debugConfig.Clone();
        clone.proceduralSkyboxConfig = proceduralSkyboxConfig.Clone();
        clone.avatarTextureAPIBaseUrl = avatarTextureAPIBaseUrl;
        clone.urlParamsForWearablesDebug = urlParamsForWearablesDebug;
        clone.builderUrl = builderUrl;
        return clone;
    }
}
