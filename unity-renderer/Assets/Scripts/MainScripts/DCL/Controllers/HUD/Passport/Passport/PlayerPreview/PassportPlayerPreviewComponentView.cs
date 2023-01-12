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

        [field: SerializeField]
        public PreviewCameraRotation PreviewCameraRotation { get; private set; }

        [SerializeField] private ShowHideAnimator tutorialShowHide;
        [SerializeField] private GameObject loadingSpinner;

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

        RenderTexture IPassportPlayerPreviewComponentView.CharacterPreviewTexture => (RenderTexture) CharacterPreviewImage.texture;
    }
}
