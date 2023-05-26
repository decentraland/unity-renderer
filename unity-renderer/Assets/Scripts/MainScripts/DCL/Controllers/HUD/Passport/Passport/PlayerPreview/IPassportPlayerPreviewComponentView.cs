using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerPreviewComponentView : IBaseComponentView<PassportPlayerPreviewModel>
    {
        event Action<double> OnEndDragEvent;
        void Initialize(IPreviewCameraRotationController avatarPreviewRotationController);
        RenderTexture CharacterPreviewTexture { get; }
        PreviewCameraRotationController PreviewCameraRotationController { get; }
        void HideTutorial();
        void SetAsLoading(bool isLoading);
    }
}
