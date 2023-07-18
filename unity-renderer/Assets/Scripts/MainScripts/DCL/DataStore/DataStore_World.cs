using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public record ExperiencesConfirmationData
    {
        public record ExperienceMetadata
        {
            public string ExperienceId { get; set; }
            public string ExperienceName { get; set; }
            public string Description { get; set; }
            public string IconUrl { get; set; }
            public string[] Permissions { get; set; }
        }

        public Action OnAcceptCallback;
        public Action OnRejectCallback;
        public ExperienceMetadata Experience;
    }

    public class DataStore_World
    {
        public readonly BaseHashSet<string> portableExperienceIds = new ();
        public readonly BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperienceIds = new ();
        public readonly BaseVariable<ExperiencesConfirmationData> portableExperiencePendingToConfirm = new ();
        public readonly BaseVariable<string> forcePortableExperience = new ();
        public readonly BaseVariable<GraphicRaycaster> currentRaycaster = new BaseVariable<GraphicRaycaster>();

        public BaseVariable<Transform> avatarTransform = new BaseVariable<Transform>(null);
        public BaseVariable<Transform> fpsTransform = new BaseVariable<Transform>(null);
        public BaseVariable<string> requestTeleportData = new BaseVariable<string>(null);
    }
}
