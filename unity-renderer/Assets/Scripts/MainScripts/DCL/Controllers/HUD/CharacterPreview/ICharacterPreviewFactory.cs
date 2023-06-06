using DCL;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewFactory : IService
    {
        ICharacterPreviewController Create(
            CharacterPreviewMode loadingMode,
            RenderTexture renderTexture,
            bool isVisible,
            PreviewCameraFocus previewCameraFocus = PreviewCameraFocus.DefaultEditing,
            bool isAvatarShadowActive = false);
    }
}
