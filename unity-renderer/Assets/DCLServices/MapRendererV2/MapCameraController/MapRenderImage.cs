using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCLServices.MapRendererV2.MapCameraController
{
    /// <summary>
    /// Extends <see cref="RawImage"/> to provide interactivity functionality
    /// </summary>
    public class MapRenderImage : RawImage, IPointerMoveHandler, IPointerExitHandler
    {
        private bool highlightEnabled;
        private IMapInteractivityController interactivityController;
        private Camera hudCamera;

        public void SetCameraController(Camera hudCamera, RenderTexture renderTexture, IMapInteractivityController interactivityController)
        {
            this.highlightEnabled = interactivityController.HighlightEnabled;
            this.hudCamera = hudCamera;
            this.interactivityController = interactivityController;

            texture = renderTexture;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!highlightEnabled)
                return;

            var screenPoint = eventData.position;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, hudCamera, out var localPoint))
            {
                var normalizedPoint = rectTransform.rect.size * localPoint;
                interactivityController.HighlightParcel(normalizedPoint);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!highlightEnabled)
                return;

            interactivityController.RemoveHighlight();
        }
    }
}
