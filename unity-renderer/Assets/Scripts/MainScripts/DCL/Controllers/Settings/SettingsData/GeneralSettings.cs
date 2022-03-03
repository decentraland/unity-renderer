using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct GeneralSettings
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }

        public float mouseSensitivity;
        public float voiceChatVolume;
        public VoiceChatAllow voiceChatAllow;
        public float scenesLoadRadius;
        public float avatarsLODDistance;
        public float maxNonLODAvatars;
        public float namesOpacity;
        public bool profanityChatFiltering;
        public bool nightMode;
        public bool hideUI;
        public bool showAvatarNames;
        public bool dynamicProceduralSkybox;
        public bool invertYAxis;
        public float skyboxTime;

        [Tooltip("First person camera FOV")]
        [Range(50, 80)]
        public float firstPersonCameraFOV;
    }
}