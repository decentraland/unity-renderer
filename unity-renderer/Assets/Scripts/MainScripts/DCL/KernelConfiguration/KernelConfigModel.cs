using KernelConfigurationTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KernelConfigModel
{
    public Features features = new Features();
    public Comms comms = new Comms();
    public Profiles profiles = new Profiles();
    public bool gifSupported = false;
    public string network = "mainnet";
    public List<WorldRange> validWorldRanges = new List<WorldRange>();
    public string kernelURL;
    public string rendererURL;
    public override bool Equals(object obj)
    {
        return obj is KernelConfigModel other && Equals(other);
    }
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
               && kernelURL == other.kernelURL
               && rendererURL == other.rendererURL;
    }

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
        return clone;
    }
}