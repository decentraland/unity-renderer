using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        event Action<Color> OnColorChanged;

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
        void SetColorPresets(List<Color> colors);
        void SetColorPickerVisibility(bool isActive);
        void SetColorPickerValue(Color color);
    }
}
