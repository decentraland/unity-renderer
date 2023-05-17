using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        event Action<Color> OnColorChanged;
        event Action OnContinueSignup;

        delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);

        bool isVisible { get; }

        void Dispose();
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
        void SetAvatarPreviewFocus(CharacterPreviewController.CameraFocus focus, bool useTransition = true);
        void TakeSnapshotsAfterStopPreviewAnimation(OnSnapshotsReady onSuccess, Action onFailed);
        void SetColorPickerVisibility(bool isActive);
        void SetColorPickerAsSkinMode(bool isSkinMode);
        void SetColorPickerValue(Color color);
        void ShowContinueSignup();
        void HideContinueSignup();
    }
}
