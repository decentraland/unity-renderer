using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class AvatarSlotComponentView : BaseComponentView<AvatarSlotComponentModel>, IPointerClickHandler, IAvatarSlotComponentView
    {
        [Header("Configuration")]
        [SerializeField] internal AvatarSlotComponentModel model;

        [SerializeField] internal NftTypeIconSO typeIcons;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] internal Image typeImage;
        [SerializeField] internal ImageComponentView nftImage;
        [SerializeField] internal Image backgroundRarityImage;
        [SerializeField] internal Image focusedImage;
        [SerializeField] internal Image selectedImage;
        [SerializeField] private GameObject emptySlot;
        [SerializeField] private GameObject hiddenSlot;
        [SerializeField] internal GameObject tooltipContainer;
        [SerializeField] internal TMP_Text tooltipText;

        public event Action<string> OnSelectAvatarSlot;

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetCategory(model.category);
            SetNftImage(model.imageUri);
            SetRarity(model.rarity);
            SetIsHidden(model.isHidden, model.hiddenBy);
        }

        public void SetIsHidden(bool isHidden, string hiddenBy)
        {
            model.isHidden = isHidden;
            model.hiddenBy = hiddenBy;
            hiddenSlot.SetActive(isHidden);
            if (isHidden)
                tooltipText.text = $"{tooltipText.text}\nHidden by: {hiddenBy}";
        }

        public void SetCategory(string category)
        {
            model.category = category;
            typeImage.sprite = typeIcons.GetTypeImage(category);
            tooltipText.text = category;
        }

        public void SetNftImage(string imageUri)
        {
            model.imageUri = imageUri;

            if (string.IsNullOrEmpty(imageUri))
            {
                nftImage.SetImage(Texture2D.grayTexture);
                emptySlot.SetActive(true);
                return;
            }

            emptySlot.SetActive(false);
            nftImage.SetImage(imageUri);
        }

        public void SetRarity(string rarity)
        {
            model.rarity = rarity;
            backgroundRarityImage.sprite = rarityBackgrounds.GetRarityImage(rarity);
        }

        public override void OnFocus()
        {
            focusedImage.enabled = true;
            tooltipContainer.SetActive(true);
        }

        public override void OnLoseFocus()
        {
            focusedImage.enabled = false;
            tooltipContainer.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSelectAvatarSlot?.Invoke(model.category);
            selectedImage.enabled = true;
        }

        public void OnPointerClickOnDifferentSlot()
        {
            selectedImage.enabled = false;
        }
    }
}
