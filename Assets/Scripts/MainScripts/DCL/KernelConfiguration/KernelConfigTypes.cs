using System;

namespace KernelConfigurationTypes
{
    [Serializable]
    public class Features
    {
        public bool enableBuilderInWorld = false;

        public bool Equals(Features other)
        {
            return enableBuilderInWorld == other?.enableBuilderInWorld;
        }

        public Features Clone()
        {
            Features clone = (Features) this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class Comms
    {
        public float commRadius = 4;
        public bool voiceChatEnabled = false;

        public bool Equals(Comms other)
        {
            return commRadius == other?.commRadius &&
                   voiceChatEnabled == other?.voiceChatEnabled;
        }

        public Comms Clone()
        {
            Comms clone = (Comms) this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class Profiles
    {
        public string nameValidCharacterRegex = "";
        public string nameValidRegex = "";

        public bool Equals(Profiles other)
        {
            return nameValidCharacterRegex == other?.nameValidCharacterRegex &&
                   nameValidRegex == other?.nameValidRegex;
        }

        public Profiles Clone()
        {
            Profiles clone = (Profiles) this.MemberwiseClone();
            return clone;
        }
    }
}