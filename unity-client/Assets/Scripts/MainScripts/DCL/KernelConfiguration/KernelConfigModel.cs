using KernelConfigurationTypes;
using System;

[Serializable]
public class KernelConfigModel
{
    public Features features = new Features();
    public Comms comms = new Comms();
    public Profiles profiles = new Profiles();
    public bool gifSupported = false;

    public bool Equals(KernelConfigModel other)
    {
        if (other == null) return false;

        return this.comms.Equals(other.comms) &&
               this.profiles.Equals(other.profiles) &&
               this.features.Equals(other.features) &&
               this.gifSupported == other.gifSupported;
    }

    public KernelConfigModel Clone()
    {
        // NOTE: We need to use deep clone
        KernelConfigModel clone = new KernelConfigModel();
        clone.comms = comms.Clone();
        clone.profiles = profiles.Clone();
        clone.features = features.Clone();
        return clone;
    }
}