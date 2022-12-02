using MainScripts.DCL.Controllers.HUD.CharacterPreview;
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

        [SerializeField] private GameObject tutorialContainer;

        public override void RefreshControl()
        {
            tutorialContainer.SetActive(model.TutorialEnabled);
        }
    }
}
