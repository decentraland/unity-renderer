using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UIComponents.Scripts.Components;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerPreviewComponentView : IBaseComponentView<PassportPlayerPreviewModel>
    {
        RawImage CharacterPreviewImage { get; }
        PreviewCameraRotation PreviewCameraRotation { get; }
    }
}
