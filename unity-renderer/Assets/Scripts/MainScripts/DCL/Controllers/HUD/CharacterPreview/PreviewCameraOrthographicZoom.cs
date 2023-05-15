using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraOrthographicZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<float> OnZoom;

        [SerializeField] private float zoomSpeed = 2.0f;
        [SerializeField] private float smoothness = 5f;

        private bool isFocused;
        private float currentZoomDelta;

        public void OnPointerEnter(PointerEventData eventData) =>
            isFocused = true;

        public void OnPointerExit(PointerEventData eventData)
        {
            isFocused = false;
            currentZoomDelta = 0;
        }

        private void Update()
        {
            if (!isFocused)
                return;

            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            float newZoomDelta = -scrollInput * zoomSpeed;
            currentZoomDelta = Mathf.Lerp(currentZoomDelta, newZoomDelta, Time.deltaTime * smoothness);

            OnZoom?.Invoke(currentZoomDelta);
        }
    }
}
