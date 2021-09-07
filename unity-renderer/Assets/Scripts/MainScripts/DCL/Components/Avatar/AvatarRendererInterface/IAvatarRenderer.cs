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

        public enum AnimationThrottling
        {
            //We set the value to the amount of frames between Updates
            Full = 1,
            Near = 2,
            FarAway = 6
        }

        bool isReady { get; }
        event Action<VisualCue> OnVisualCue;
        void CleanupAvatar();
        void SetExpression(string id, long timestamp);
        void SetGOVisibility(bool newVisibility);
        void SetRendererEnabled(bool newVisibility);
        void SetImpostorVisibility(bool impostorVisibility);
        void SetImpostorForward(Vector3 newForward);
        void SetAvatarFade(float avatarFade);
        void SetImpostorFade(float impostorFade);
        void SetFacialFeaturesVisible(bool visible);
        void SetSSAOEnabled(bool enabled);
        void SetImpostorColor(Color newColor);
        void SetThrottling(AnimationThrottling throttling);
    }
}