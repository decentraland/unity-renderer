using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public class GeneralSettings : ICloneable, IEquatable<GeneralSettings>
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }

        public float mouseSensitivity;
        public float voiceChatVolume;
        public VoiceChatAllow voiceChatAllow;
        public bool autoqualityOn;
        public float scenesLoadRadius;
        public float avatarsLODDistance;
        public float maxNonLODAvatars;
        public float namesOpacity;

        public object Clone() { return (GeneralSettings)MemberwiseClone(); }

        public bool Equals(GeneralSettings other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return mouseSensitivity.Equals(other.mouseSensitivity) && voiceChatVolume.Equals(other.voiceChatVolume) && voiceChatAllow == other.voiceChatAllow && autoqualityOn == other.autoqualityOn && scenesLoadRadius.Equals(other.scenesLoadRadius) && avatarsLODDistance.Equals(other.avatarsLODDistance) && maxNonLODAvatars.Equals(other.maxNonLODAvatars) && namesOpacity.Equals(other.namesOpacity);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((GeneralSettings) obj);
        }
    }
}