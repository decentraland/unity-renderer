using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.SettingsCommon
{
    [Serializable]
    public struct GeneralSettings
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }

        public float mouseSensitivity;
        public bool invertYAxis;
        public bool leftMouseButtonCursorLock;

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
        public bool adultContent;

        public bool dynamicProceduralSkybox;
        public float useDynamicSkybox;
        public float skyboxTime;

        [Tooltip("First person camera FOV")]
        [Range(50, 80)]
        public float firstPersonCameraFOV;
    }
}
