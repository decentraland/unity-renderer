using KernelConfigurationTypes;
using System;

[Serializable]
public class KernelConfigModel
{
    public Comms comms = new Comms();


    public bool Equals(KernelConfigModel other)
    {
        if (other == null) return false;

        return this.comms.Equals(other.comms);
    }

    public KernelConfigModel Clone()
    {
        // NOTE: We need to use deep clone
        KernelConfigModel clone = new KernelConfigModel();
        clone.comms = comms.Clone();
        return clone;
    }
}
