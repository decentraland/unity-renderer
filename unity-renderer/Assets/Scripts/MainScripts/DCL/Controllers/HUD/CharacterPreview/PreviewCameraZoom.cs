using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<Vector3> OnZoom;

        [SerializeField] private float zoomSpeed = 2.0f;
        [SerializeField] private float smoothTime = 0.2f;

        private bool isFocused;
        private Vector3 currentZoomDelta;
        private Vector3 currentZoomVelocity;

        public void OnPointerEnter(PointerEventData eventData) =>
            isFocused = true;

        public void OnPointerExit(PointerEventData eventData)
        {
            isFocused = false;
            currentZoomDelta = Vector3.zero;
        }

        private void Update()
        {
            if (!isFocused)
                return;

            Vector3 newZoomDelta = Vector3.forward * (-Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
            if (currentZoomDelta == newZoomDelta)
                return;

            currentZoomDelta = Vector3.SmoothDamp(currentZoomDelta, newZoomDelta, ref currentZoomVelocity, smoothTime);
            OnZoom?.Invoke(currentZoomDelta);
        }
    }
}
