using DCL;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewFactory : IService
    {
        ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible,
            CharacterPreviewController.CameraFocus cameraFocus = CharacterPreviewController.CameraFocus.DefaultEditing);
    }
}
