using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerPreviewComponentView : BaseComponentView, IPassportPlayerPreviewComponentView
    {
        [field: SerializeField]
        public RawImage CharacterPreviewImage { get; private set; }

        [field: SerializeField]
        public PreviewCameraRotation PreviewCameraRotation { get; private set; }

        public override void RefreshControl()
        {
        }
    }
}
