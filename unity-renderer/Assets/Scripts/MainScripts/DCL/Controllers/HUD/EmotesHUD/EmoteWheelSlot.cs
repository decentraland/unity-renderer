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
        [SerializeField] internal GameObject loadingSpinnerGO;

        public event Action<string> onSlotHover;

        public string emoteId { get; private set; }
        public bool isLoading { get; private set; }

        private string emoteName;

        public void SetId(string id) { emoteId = id; }

        public void SetName(string name) { emoteName = name; }

        public void SetRarity(bool isActive, Color color)
        {
            rarityImage.transform.parent.gameObject.SetActive(isActive);
            rarityImage.color = color;
        }

        public void SetAsLoading(bool isLoading)
        {
            this.isLoading = isLoading;
            loadingSpinnerGO.SetActive(isLoading);
            image.gameObject.SetActive(!isLoading);
            button.enabled = !isLoading;
        }

        public void OnPointerEnter(PointerEventData eventData) { onSlotHover?.Invoke(emoteName); }
        
        public void OnPointerExit(PointerEventData eventData) { onSlotHover?.Invoke(string.Empty); }
    }
}