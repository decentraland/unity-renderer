using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        event Action<Color> OnColorChanged;
        event Action OnColorPickerToggle;
        event Action OnContinueSignup;
        event Action OnAvatarUpdated;
        event Action OnOutfitsOpened;
        event Action OnVRMExport;

        delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);
        IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers { get; }

        bool isVisible { get; }

        void Dispose();
        void SetOutfitsEnabled(bool isEnabled);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void PlayPreviewEmote(string emoteId, long timestamp);
        void ResetPreviewPanel();
        void UpdateAvatarPreview(AvatarModel avatarModel);
        void SetAvatarPreviewFocus(PreviewCameraFocus focus, bool useTransition = true);
        void TakeSnapshotsAfterStopPreviewAnimation(OnSnapshotsReady onSuccess, Action onFailed);
        void SetColorPickerVisibility(bool isActive);
        void SetColorPickerAsSkinMode(bool isSkinMode);
        void UpdateHideUnhideStatus(string slotCategory, HashSet<string> forceRender);
        void SetColorPickerValue(Color color);
        void ShowContinueSignup();
        void HideContinueSignup();
        void SetVRMButtonActive(bool enabled);
        void SetVRMButtonEnabled(bool enabled);
        void SetVRMSuccessToastActive(bool active);

    }
}
