using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmotesCustomization
{
    public class EmoteWheelSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] internal Button_OnPointerDown button;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal Image rarityImage;

        public event Action<string> onSlotHover;

        private string emoteName;

        public void SetName(string name) { emoteName = name; }
        public void SetRarity(bool isActive, Color color)
        {
            rarityImage.transform.parent.gameObject.SetActive(isActive);
            rarityImage.color = color;
        }
        public void OnPointerEnter(PointerEventData eventData) { onSlotHover?.Invoke(emoteName); }
        public void OnPointerExit(PointerEventData eventData) { onSlotHover?.Invoke(string.Empty); }
    }
}