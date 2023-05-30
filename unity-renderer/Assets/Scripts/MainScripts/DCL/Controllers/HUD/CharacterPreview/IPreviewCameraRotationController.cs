using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface IPreviewCameraRotationController : IDisposable
    {
        event Action<float> OnHorizontalRotation;
        event Action<double> OnEndDragEvent;

        void Configure(
            InputAction_Hold firstClickAction,
            float rotationFactor,
            float slowDownTime,
            ICharacterPreviewInputDetector characterPreviewInputDetector,
            Texture2D rotateCursorTexture);
    }
}
