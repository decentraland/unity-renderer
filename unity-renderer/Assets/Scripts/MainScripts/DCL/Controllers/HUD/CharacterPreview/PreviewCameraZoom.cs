using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<Vector3> OnZoom;

        [SerializeField] internal InputAction_Measurable mouseWheelAction;
        [SerializeField] private float zoomSpeed = 2.0f;
        [SerializeField] private float smoothTime = 0.2f;

        private Vector3 currentZoomDelta;
        private Vector3 currentZoomVelocity;
        private CancellationTokenSource cts = new ();

        public void OnPointerEnter(PointerEventData eventData) =>
            ZoomAsync(cts.Token).Forget();

        public void OnPointerExit(PointerEventData eventData)
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

        private void OnDestroy() =>
            cts.SafeCancelAndDispose();
    }
}
