using System;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);

        bool isVisible { get; }

        void Dispose();
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
        void TakeSnapshotsAfterStopPreviewAnimation(OnSnapshotsReady onSuccess, Action onFailed);
    }
}
