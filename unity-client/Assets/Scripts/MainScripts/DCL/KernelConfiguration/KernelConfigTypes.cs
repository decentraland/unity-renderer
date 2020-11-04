using System;

namespace KernelConfigurationTypes
{
    [Serializable]
    public class Comms
    {
        public float commRadius = 4;

        public bool Equals(Comms other)
        {
            return commRadius == other?.commRadius;
        }

        public Comms Clone()
        {
            Comms clone = (Comms)this.MemberwiseClone();
            return clone;
        }
    }
}