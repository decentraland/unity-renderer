using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class AvatarSlotComponentView : BaseComponentView<AvatarSlotComponentModel>, IAvatarSlotComponentView
    {
        private const float ANIMATION_TIME = 0.2f;
        private const float SHAKE_ANIMATION_TIME = 0.75f;

        [Header("Configuration")]
        [SerializeField] internal AvatarSlotComponentModel model;

        [SerializeField] internal NftTypeIconSO typeIcons;
        [SerializeField] internal Transform nftContainer;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] internal Image typeImage;
        [SerializeField] internal ImageComponentView nftImage;
        [SerializeField] internal Image backgroundRarityImage;
        [SerializeField] internal Image focusedImage;
        [SerializeField] internal Image selectedImage;
        [SerializeField] private GameObject emptySlot;
        [SerializeField] private GameObject hiddenSlot;
        [SerializeField] internal GameObject tooltipContainer;
        [SerializeField] internal TMP_Text tooltipCategoryText;
        [SerializeField] internal TMP_Text tooltipHiddenText;
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
                nftContainer.DOShakePosition(SHAKE_ANIMATION_TIME, 4);
                emptySlot.SetActive(false);
                tooltipHiddenText.gameObject.SetActive(true);
                tooltipHiddenText.text = $"Hidden by: {hiddenBy}";
            }
            else
            {
                tooltipHiddenText.gameObject.SetActive(false);
            }
        }

        public void SetCategory(string category)
        {
            model.category = category;
            typeImage.sprite = typeIcons.GetTypeImage(category);
            tooltipCategoryText.text = category;
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
            focusedImage.transform.localScale = new Vector3(0, 0, 0);
            focusedImage.transform.DOScale(1, ANIMATION_TIME).SetEase(Ease.OutBack);
        }

        public override void OnLoseFocus()
        {
            focusedImage.enabled = false;
            tooltipContainer.SetActive(false);
        }

        public void OnSlotClick()
        {
            isSelected = !isSelected;

            if (isSelected)
            {
                selectedImage.enabled = true;
                selectedImage.transform.localScale = new Vector3(0, 0, 0);
                selectedImage.transform.DOScale(1, ANIMATION_TIME).SetEase(Ease.OutBack);
            }
            else
            {
                selectedImage.transform.DOScale(0, ANIMATION_TIME).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    selectedImage.enabled = false;
                    selectedImage.transform.localScale = new Vector3(1,1,1);
                });
            }

            OnSelectAvatarSlot?.Invoke(model.category, isSelected);
        }

        public void OnPointerClickOnDifferentSlot()
        {
            isSelected = false;
            selectedImage.enabled = false;
        }
    }
}
