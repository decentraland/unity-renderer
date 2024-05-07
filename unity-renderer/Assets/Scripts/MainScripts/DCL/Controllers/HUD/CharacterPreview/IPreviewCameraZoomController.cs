using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface IPreviewCameraZoomController : IDisposable
    {
        event Action<Vector3> OnZoom;

        void Configure(
            InputAction_Measurable mouseWheelAction,
            float zoomSpeed,
            float smoothTime,
            ICharacterPreviewInputDetector characterPreviewInputDetector);
    }
}
