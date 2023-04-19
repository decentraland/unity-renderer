using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        bool isVisible { get; }

        void Dispose();
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
    }
}
