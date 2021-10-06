using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public class GeneralSettings : ICloneable
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
        public bool Equals(GeneralSettings settings)
        {
            return mouseSensitivity == settings.mouseSensitivity
                   && scenesLoadRadius == settings.scenesLoadRadius
                   && avatarsLODDistance == settings.avatarsLODDistance
                   && maxNonLODAvatars == settings.maxNonLODAvatars
                   && voiceChatVolume == settings.voiceChatVolume
                   && voiceChatAllow == settings.voiceChatAllow
                   && autoqualityOn == settings.autoqualityOn
                   && namesOpacity == settings.namesOpacity;
        }
    }
}