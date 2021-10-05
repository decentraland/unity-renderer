using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct AudioSettings
    {
        public float masterVolume;
        public float voiceChatVolume;
        public float avatarSFXVolume;
        public float uiSFXVolume;
        public float sceneSFXVolume; // Note(Mordi): Also known as "World SFX"
        public float musicVolume;
        public bool chatSFXEnabled;

        public bool Equals(AudioSettings settings)
        {
            return masterVolume == settings.masterVolume
                   && voiceChatVolume == settings.voiceChatVolume
                   && avatarSFXVolume == settings.avatarSFXVolume
                   && uiSFXVolume == settings.uiSFXVolume
                   && sceneSFXVolume == settings.sceneSFXVolume
                   && musicVolume == settings.musicVolume
                   && chatSFXEnabled == settings.chatSFXEnabled;
        }
    }
}