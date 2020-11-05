using System;

namespace KernelConfigurationTypes
{
    [Serializable]
    public class Comms
    {
        public float commRadius = 4;
        public bool voiceChatEnabled = false;

        public bool Equals(Comms other)
        {
            return commRadius == other?.commRadius
            && voiceChatEnabled == other?.voiceChatEnabled;
        }

        public Comms Clone()
        {
            Comms clone = (Comms)this.MemberwiseClone();
            return clone;
        }
    }
}