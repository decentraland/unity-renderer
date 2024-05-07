using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraZoomController : IPreviewCameraZoomController
    {
        public event Action<Vector3> OnZoom;

        private InputAction_Measurable mouseWheelAction;
        private float zoomSpeed;
        private float smoothTime;
        private ICharacterPreviewInputDetector characterPreviewInputDetector;
        private Vector3 currentZoomDelta;
        private Vector3 currentZoomVelocity;
        private CancellationTokenSource cts = new ();

        public void Configure(
            InputAction_Measurable mouseWheelAction,
            float zoomSpeed,
            float smoothTime,
            ICharacterPreviewInputDetector characterPreviewInputDetector)
        {
            this.mouseWheelAction = mouseWheelAction;
            this.zoomSpeed = zoomSpeed;
            this.smoothTime = smoothTime;
            this.characterPreviewInputDetector = characterPreviewInputDetector;

            characterPreviewInputDetector.OnPointerFocus += OnPointerEnter;
            characterPreviewInputDetector.OnPointerUnFocus += OnPointerExit;
        }

        private void OnPointerEnter(PointerEventData eventData)
        {
            cts = cts.SafeRestart();
            ZoomAsync(cts.Token).Forget();
        }

        private void OnPointerExit(PointerEventData eventData)
        {
            cts = cts.SafeRestart();
            currentZoomDelta = Vector3.zero;
        }

        private async UniTask ZoomAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Vector3 newZoomDelta = Vector3.forward * (-mouseWheelAction.GetValue() * zoomSpeed);

                if (currentZoomDelta != newZoomDelta)
                {
                    currentZoomDelta = Vector3.SmoothDamp(currentZoomDelta, newZoomDelta, ref currentZoomVelocity, smoothTime);
                    OnZoom?.Invoke(currentZoomDelta);
                }

                await UniTask.NextFrame(ct);
            }
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            characterPreviewInputDetector.OnPointerFocus -= OnPointerEnter;
            characterPreviewInputDetector.OnPointerUnFocus -= OnPointerExit;
        }
    }
}
