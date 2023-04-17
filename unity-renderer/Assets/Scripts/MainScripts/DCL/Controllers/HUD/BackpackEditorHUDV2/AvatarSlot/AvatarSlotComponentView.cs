using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class AvatarSlotComponentView : BaseComponentView<AvatarSlotComponentModel>, IAvatarSlotComponentView
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
        [SerializeField] internal Button button;

        public event Action<string, bool> OnSelectAvatarSlot;
        private bool isSelected = false;

        public void Start()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClick);
        }

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
            {
                emptySlot.SetActive(false);
                tooltipText.text = $"{tooltipText.text}\nHidden by: {hiddenBy}";
            }
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

        public void OnSlotClick()
        {
            isSelected = !isSelected;
            selectedImage.enabled = isSelected;

            OnSelectAvatarSlot?.Invoke(model.category, isSelected);
        }

        public void OnPointerClickOnDifferentSlot()
        {
            isSelected = false;
            selectedImage.enabled = false;
        }
    }
}
