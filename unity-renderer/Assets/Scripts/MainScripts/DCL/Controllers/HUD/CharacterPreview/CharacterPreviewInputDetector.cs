using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class CharacterPreviewInputDetector : MonoBehaviour, ICharacterPreviewInputDetector
    {
        public event Action<PointerEventData> OnDragStarted;
        public event Action<PointerEventData> OnDragging;
        public event Action<PointerEventData> OnDragFinished;
        public event Action<PointerEventData> OnPointerFocus;
        public event Action<PointerEventData> OnPointerUnFocus;

        public void OnBeginDrag(PointerEventData eventData) =>
            OnDragStarted?.Invoke(eventData);

        public void OnDrag(PointerEventData eventData) =>
            OnDragging?.Invoke(eventData);

        public void OnEndDrag(PointerEventData eventData) =>
            OnDragFinished?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) =>
            OnPointerFocus?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) =>
            OnPointerUnFocus?.Invoke(eventData);
    }
}
