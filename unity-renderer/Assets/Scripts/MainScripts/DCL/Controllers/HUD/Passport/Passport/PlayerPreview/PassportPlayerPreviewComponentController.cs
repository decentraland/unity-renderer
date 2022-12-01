using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportPlayerPreviewComponentController : IDisposable
    {
        private readonly Service<ICharacterPreviewFactory> characterPreviewFactory;

        private readonly ICharacterPreviewController previewController;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IPassportPlayerPreviewComponentView view;

        public PassportPlayerPreviewComponentController(IPassportPlayerPreviewComponentView view)
        {
            this.view = view;
            view.PreviewCameraRotation.OnHorizontalRotation += RotateCharacterPreview;
            cancellationTokenSource = new CancellationTokenSource();

            previewController = characterPreviewFactory.Ref.Create(CharacterPreviewMode.WithHologram, (RenderTexture)view.CharacterPreviewImage.texture, true);
        }

        public void SetPassportPanelVisibility(bool visible)
        {
            previewController.SetEnabled(visible);
        }

        private void RotateCharacterPreview(float angularVelocity)
        {
            previewController.Rotate(angularVelocity);
        }

        public void UpdateWithUserProfile(UserProfile userProfile)
        {
            previewController.TryUpdateModelAsync(userProfile.avatar, cancellationTokenSource.Token)
                             .SuppressCancellationThrow()
                             .Forget();
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
