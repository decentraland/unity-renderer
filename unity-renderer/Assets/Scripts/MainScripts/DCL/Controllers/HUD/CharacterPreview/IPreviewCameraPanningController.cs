using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface IPreviewCameraPanningController : IDisposable
    {
        event Action<Vector3> OnPanning;

        void Configure(
            InputAction_Hold secondClickAction,
            InputAction_Hold middleClickAction,
            float panSpeed,
            bool allowVerticalPanning,
            bool allowHorizontalPanning,
            float inertiaDuration,
            ICharacterPreviewInputDetector characterPreviewInputDetector,
            Texture2D panningCursorTexture);
    }
}
