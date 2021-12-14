using System;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct GeneralSettings
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }
        public enum ProceduralSkyboxMode { DYNAMIC, FIXED }

        public float mouseSensitivity;
        public float voiceChatVolume;
        public VoiceChatAllow voiceChatAllow;
        public bool autoqualityOn;
        public float scenesLoadRadius;
        public float avatarsLODDistance;
        public float maxNonLODAvatars;
        public float namesOpacity;
        public bool profanityChatFiltering;
        public ProceduralSkyboxMode proceduralSkyboxMode;
        public float skyboxTime;
    }
}