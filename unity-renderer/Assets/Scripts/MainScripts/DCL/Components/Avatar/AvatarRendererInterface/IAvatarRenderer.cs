using System;
using UnityEngine;

namespace DCL
{
    public interface IAvatarRenderer
    {
        public enum VisualCue
        {
            CleanedUp,
            Loaded
        }

        bool isReady { get; }
        event Action<VisualCue> OnVisualCue;
        void CleanupAvatar();
        void UpdateExpressions(string id, long timestamp);
        void SetVisibility(bool newVisibility);
        void SetImpostorVisibility(bool impostorVisibility);
        void SetImpostorForward(Vector3 newForward);
        void SetAvatarFade(float avatarFade);
        void SetImpostorFade(float impostorFade);
        void SetFacialFeaturesVisible(bool visible);
        void SetSSAOEnabled(bool enabled);
    }

}