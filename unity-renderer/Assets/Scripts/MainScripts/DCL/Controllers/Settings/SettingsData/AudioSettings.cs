using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public class AudioSettings : ICloneable, IEquatable<AudioSettings>
    {
        public float masterVolume;
        public float voiceChatVolume;
        public float avatarSFXVolume;
        public float uiSFXVolume;
        public float sceneSFXVolume; // Note(Mordi): Also known as "World SFX"
        public float musicVolume;
        public bool chatSFXEnabled;

        public object Clone() { return MemberwiseClone(); }
        public bool Equals(AudioSettings other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return masterVolume.Equals(other.masterVolume) && voiceChatVolume.Equals(other.voiceChatVolume) && avatarSFXVolume.Equals(other.avatarSFXVolume) && uiSFXVolume.Equals(other.uiSFXVolume) && sceneSFXVolume.Equals(other.sceneSFXVolume) && musicVolume.Equals(other.musicVolume) && chatSFXEnabled == other.chatSFXEnabled;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((AudioSettings) obj);
        }
    }
}