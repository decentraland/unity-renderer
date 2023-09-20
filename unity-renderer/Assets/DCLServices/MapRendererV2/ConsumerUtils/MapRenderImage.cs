using DCLServices.MapRendererV2.MapCameraController;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace DCLServices.MapRendererV2.ConsumerUtils
{
    /// <summary>
    /// Extends <see cref="RawImage"/> to provide interactivity functionality
    /// </summary>
    public class MapRenderImage : RawImage, IPointerMoveHandler, IPointerExitHandler, IPointerClickHandler,
        IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public struct ParcelClickData
        {
            public Vector2Int Parcel;
            public Vector2 WorldPosition;
        }

        private static readonly string DRAG_SAMPLE_NAME = $"{nameof(MapRenderImage)}.{nameof(OnDrag)}";
        private static readonly string POINTER_MOVE_SAMPLE_NAME = $"{nameof(MapRenderImage)}.{nameof(OnPointerMove)}";
        private static readonly string POINTER_CLICK_SAMPLE_NAME = $"{nameof(MapRenderImage)}.{nameof(OnPointerClick)}";

        public event Action<ParcelClickData> ParcelClicked;

        /// <summary>
        /// Notifies with the world position
        /// </summary>
        public event Action<Vector2> Hovered;

        public event Action DragStarted;

        private MapCameraDragBehavior dragBehavior;

        private bool highlightEnabled;
        private IMapInteractivityController interactivityController;
        private Camera hudCamera;

        private bool isActive;

        public void EmbedMapCameraDragBehavior(MapCameraDragBehavior.MapCameraDragBehaviorData data)
        {
            dragBehavior = new MapCameraDragBehavior(rectTransform, data);
        }

        public void Activate(Camera hudCamera, RenderTexture renderTexture, IMapCameraController mapCameraController)
        {
            interactivityController = mapCameraController.GetInteractivityController();
            this.highlightEnabled = interactivityController.HighlightEnabled;
            this.hudCamera = hudCamera;

            texture = renderTexture;

            dragBehavior?.Activate(mapCameraController);

            isActive = true;
        }

        public void Deactivate()
        {
            dragBehavior?.Deactivate();

            hudCamera = null;
            interactivityController = null;
            texture = null;

            isActive = false;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!isActive)
                return;

            if (dragging)
                return;

            Profiler.BeginSample(POINTER_MOVE_SAMPLE_NAME);

            ProcessHover(eventData);

            Profiler.EndSample();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActive)
                return;

            if (!highlightEnabled)
                return;

            interactivityController.RemoveHighlight();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Profiler.BeginSample(POINTER_CLICK_SAMPLE_NAME);

            if (isActive && !dragging && TryGetParcelUnderPointer(eventData, out var parcel, out _, out _))
            {
                ParcelClicked?.Invoke(new ParcelClickData
                {
                    Parcel = parcel,
                    WorldPosition = GetParcelWorldPosition(parcel),
                });
            }

            Profiler.EndSample();
        }

        private bool dragging => dragBehavior is { dragging: true };

        private Vector2 GetParcelWorldPosition(Vector2Int parcel)
        {
            var normalizedDiscretePosition = interactivityController.GetNormalizedPosition(parcel);
            return rectTransform.TransformPoint(rectTransform.rect.size * (normalizedDiscretePosition - rectTransform.pivot));
        }

        private void ProcessHover(PointerEventData eventData)
        {
            if (TryGetParcelUnderPointer(eventData, out var parcel, out _, out var worldPosition))
            {
                if (highlightEnabled)
                    interactivityController.HighlightParcel(parcel);

                Hovered?.Invoke(worldPosition);
            }
            else if (highlightEnabled)
                interactivityController.RemoveHighlight();
        }

        private bool TryGetParcelUnderPointer(PointerEventData eventData, out Vector2Int parcel, out Vector2 localPosition, out Vector3 worldPosition)
        {
            var screenPoint = eventData.position;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, hudCamera, out worldPosition))
            {
                var rectSize = rectTransform.rect.size;
                localPosition = (Vector2) rectTransform.InverseTransformPoint(worldPosition);
                var leftCornerRelativeLocalPosition = localPosition + (rectTransform.pivot * rectSize);
                return interactivityController.TryGetParcel(leftCornerRelativeLocalPosition / rectSize, out parcel);
            }
            parcel = Vector2Int.zero;
            localPosition = Vector2.zero;
            return false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActive) return;

            Profiler.BeginSample(DRAG_SAMPLE_NAME);

            dragBehavior?.OnDrag(eventData);

            Profiler.EndSample();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isActive) return;

            DragStarted?.Invoke();
            dragBehavior?.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isActive) return;

            dragBehavior?.OnEndDrag(eventData);

            ProcessHover(eventData);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            dragBehavior?.Dispose();
        }
    }
}
