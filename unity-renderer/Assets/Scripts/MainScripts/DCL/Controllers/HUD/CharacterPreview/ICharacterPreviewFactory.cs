using DCL;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewFactory : IService
    {
        ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible,
            global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController.CameraFocus cameraFocus = global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController.CameraFocus.DefaultEditing);
    }
}
