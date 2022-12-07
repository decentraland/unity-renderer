using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerPreviewComponentView : IBaseComponentView<PassportPlayerPreviewModel>
    {
        RenderTexture CharacterPreviewTexture { get; }
        PreviewCameraRotation PreviewCameraRotation { get; }
    }
}
