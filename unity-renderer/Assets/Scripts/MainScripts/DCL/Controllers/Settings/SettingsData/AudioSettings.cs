using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public class AudioSettings : ICloneable
    {
        public float masterVolume;
        public float voiceChatVolume;
        public float avatarSFXVolume;
        public float uiSFXVolume;
        public float sceneSFXVolume; // Note(Mordi): Also known as "World SFX"
        public float musicVolume;
        public bool chatSFXEnabled;

        public object Clone() => MemberwiseClone();
    }
}