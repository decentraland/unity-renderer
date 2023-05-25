using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraPanningDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPreviewCameraPanningDetector
    {
        public event Action OnDragStarted;
        public event Action OnDragging;
        public event Action OnDragFinished;

        public void OnBeginDrag(PointerEventData eventData) =>
            OnDragStarted?.Invoke();

        public void OnDrag(PointerEventData eventData) =>
            OnDragging?.Invoke();

        public void OnEndDrag(PointerEventData eventData) => OnDragFinished?.Invoke();
    }
}
