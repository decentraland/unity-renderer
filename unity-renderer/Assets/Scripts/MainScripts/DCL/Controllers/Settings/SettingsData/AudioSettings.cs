using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct AudioSettings
    {
        public int inputDevice;
        public float masterVolume;
        public float voiceChatVolume;
        public float avatarSFXVolume;
        public float uiSFXVolume;
        public float sceneSFXVolume; // Note(Mordi): Also known as "World SFX"
        public float musicVolume;
        public bool chatSFXEnabled;
    }
}