using KernelConfigurationTypes;
using System;
using System.Collections.Generic;

[Serializable]
public class KernelConfigModel
{
    public Features features = new Features();
    public Comms comms = new Comms();
    public Profiles profiles = new Profiles();
    public bool gifSupported = false;
    public string tld = "org";
    public List<WorldRange> validWorldRanges = new List<WorldRange>();

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

        return this.comms.Equals(other.comms) &&
               this.profiles.Equals(other.profiles) &&
               this.features.Equals(other.features) &&
               this.gifSupported == other.gifSupported &&
               this.tld == other.tld;
    }

    public KernelConfigModel Clone()
    {
        // NOTE: We need to use deep clone
        KernelConfigModel clone = new KernelConfigModel();
        clone.comms = comms.Clone();
        clone.profiles = profiles.Clone();
        clone.features = features.Clone();
        clone.gifSupported = gifSupported;
        clone.tld = tld;
        clone.validWorldRanges = new List<WorldRange>(validWorldRanges);
        return clone;
    }
}