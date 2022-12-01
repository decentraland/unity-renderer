using DCL;
using UnityEngine;

public interface ICharacterPreviewFactory : IService
{
    ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible);
}
