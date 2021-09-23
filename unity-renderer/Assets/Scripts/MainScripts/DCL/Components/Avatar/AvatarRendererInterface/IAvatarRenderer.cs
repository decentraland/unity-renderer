using System;

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

        IAvatarImpostor impostor { get; }
        event Action<VisualCue> OnVisualCue;
        void CleanupAvatar();
        void SetExpression(string id, long timestamp);
        void SetGOVisibility(bool newVisibility);
        void SetRendererEnabled(bool newVisibility);
        void SetAvatarFade(float avatarFade);
        void SetFacialFeaturesVisible(bool visible);
        void SetSSAOEnabled(bool enabled);
    }
}