using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EmotesCustomization
{
    public class EmoteWheelSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] internal Button_OnPointerDown button;
        [SerializeField] internal ImageComponentView image;

        public event Action<string> onSlotHover;

        private string emoteName;

        public void SetName(string name) { emoteName = name; }
        public void OnPointerEnter(PointerEventData eventData) { onSlotHover?.Invoke(emoteName); }
        public void OnPointerExit(PointerEventData eventData) { onSlotHover?.Invoke(string.Empty); }
    }
}