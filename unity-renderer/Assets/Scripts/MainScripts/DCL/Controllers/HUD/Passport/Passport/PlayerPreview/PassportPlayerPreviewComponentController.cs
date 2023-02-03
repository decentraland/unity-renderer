using Cysharp.Threading.Tasks;
using DCL.Helpers;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Threading;

namespace DCL.Social.Passports
{
    public class PassportPlayerPreviewComponentController : IDisposable
    {
        private const string TUTORIAL_ENABLED_KEY = "PassportPreviewRotationHintEnabled";

        private readonly Service<ICharacterPreviewFactory> characterPreviewFactory;

        private readonly ICharacterPreviewController previewController;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IPassportPlayerPreviewComponentView view;

        public PassportPlayerPreviewComponentController(IPassportPlayerPreviewComponentView view)
        {
            this.view = view;
            view.PreviewCameraRotation.OnHorizontalRotation += RotateCharacterPreview;
            cancellationTokenSource = new CancellationTokenSource();

            previewController = characterPreviewFactory.Ref.Create(CharacterPreviewMode.WithoutHologram,
                view.CharacterPreviewTexture, true, CharacterPreviewController.CameraFocus.Preview);

            view.SetModel(new (TutorialEnabled));
        }

        public void SetPassportPanelVisibility(bool visible)
        {
            previewController.SetEnabled(visible);
            if (visible)
                previewController.ResetRotation();
        }

        public void SetAsLoading(bool isLoading) => view.SetAsLoading(isLoading);

        private bool TutorialEnabled => PlayerPrefsUtils.GetBool(TUTORIAL_ENABLED_KEY, true);

        private void RotateCharacterPreview(float angularVelocity)
        {
            previewController.Rotate(angularVelocity);
            SetRotationTutorialCompleted();
        }

        private void SetRotationTutorialCompleted()
        {
            if (!TutorialEnabled)
                return;

            PlayerPrefsUtils.SetBool(TUTORIAL_ENABLED_KEY, false);
            PlayerPrefsUtils.Save();
            view.HideTutorial();

            view.SetModel(new (false));
        }

        public void UpdateWithUserProfile(UserProfile userProfile, bool activateLoading)
        {
            async UniTask UpdateWithUserProfileAsync()
            {
                if (activateLoading)
                    SetAsLoading(true);

                await previewController.TryUpdateModelAsync(userProfile.avatar, cancellationTokenSource.Token)
                                       .SuppressCancellationThrow();

                SetAsLoading(false);
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            UpdateWithUserProfileAsync().Forget();
        }

        public void Dispose()
        {
            view.PreviewCameraRotation.OnHorizontalRotation -= RotateCharacterPreview;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
