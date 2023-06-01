using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerPreviewComponentView : BaseComponentView<PassportPlayerPreviewModel>, IPassportPlayerPreviewComponentView
    {
        [field: SerializeField]
        public RawImage CharacterPreviewImage { get; private set; }
        public event Action<double> OnEndDragEvent;

        public PreviewCameraRotationController PreviewCameraRotationController => (PreviewCameraRotationController) avatarPreviewRotationController;
        private IPreviewCameraRotationController avatarPreviewRotationController;

        [SerializeField] private ShowHideAnimator tutorialShowHide;
        [SerializeField] private GameObject loadingSpinner;

        [Header("MOUSE INPUT CONFIGURATION")]
        [SerializeField] private CharacterPreviewInputDetector characterPreviewInputDetector;
        [SerializeField] private InputAction_Hold firstClickAction;

        [Header("ROTATE CONFIGURATION")]
        [SerializeField] private float rotationFactor = -30f;
        [SerializeField] private float slowDownTime = 0.5f;

        public void Initialize(IPreviewCameraRotationController avatarPreviewRotationController)
        {
            this.avatarPreviewRotationController = avatarPreviewRotationController;
            this.avatarPreviewRotationController.Configure(
                firstClickAction,
                rotationFactor,
                slowDownTime,
                characterPreviewInputDetector,
                null);

            this.avatarPreviewRotationController.OnEndDragEvent += EndPreviewDrag;
        }

        public override void Dispose()
        {
            base.Dispose();

            avatarPreviewRotationController.OnEndDragEvent -= EndPreviewDrag;
            avatarPreviewRotationController.Dispose();
        }

        public void HideTutorial()
        {
            tutorialShowHide.Hide();
        }

        public void SetAsLoading(bool isLoading)
        {
            loadingSpinner.SetActive(isLoading);
            CharacterPreviewImage.gameObject.SetActive(!isLoading);
        }

        public override void RefreshControl()
        {
            if (model.TutorialEnabled)
            {
                tutorialShowHide.gameObject.SetActive(true);
            }
            else
            {
                HideTutorial();
            }
        }

        private void EndPreviewDrag(double dragTime)
        {
            OnEndDragEvent?.Invoke(dragTime);
        }

        RenderTexture IPassportPlayerPreviewComponentView.CharacterPreviewTexture => (RenderTexture) CharacterPreviewImage.texture;
    }
}
