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

        [field: SerializeField]
        public PreviewCameraRotation PreviewCameraRotation { get; private set; }

        [SerializeField] private ShowHideAnimator tutorialShowHide;
        [SerializeField] private GameObject loadingSpinner;

        public override void Awake()
        {
            base.Awake();

            PreviewCameraRotation.OnEndDragEvent += EndPreviewDrag;
        }

        public override void Dispose()
        {
            base.Dispose();

            PreviewCameraRotation.OnEndDragEvent -= EndPreviewDrag;
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
