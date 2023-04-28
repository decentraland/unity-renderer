using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] internal RectTransform nftContainer;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] internal Image typeImage;
        [SerializeField] internal ImageComponentView nftImage;
        [SerializeField] internal Image backgroundRarityImage;
        [SerializeField] internal Image focusedImage;
        [SerializeField] internal Image selectedImage;
        [SerializeField] private GameObject emptySlot;
        [SerializeField] private GameObject hiddenSlot;
        [SerializeField] internal RectTransform tooltipContainer;
        [SerializeField] internal TMP_Text tooltipCategoryText;
        [SerializeField] internal TMP_Text tooltipHiddenText;
        [SerializeField] internal Button button;
        [SerializeField] internal Button unequipButton;

        public event Action<string, bool> OnSelectAvatarSlot;
        public event Action<string> OnUnEquip;
        private bool isSelected = false;
        private readonly HashSet<string> hiddenByList = new HashSet<string>();
        private Vector2 tooltipDefaultPosition;
        private Vector2 tooltipFullPosition;

        public override void Awake()
        {
            base.Awake();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClick);
            unequipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.AddListener(()=>
            {
                OnUnEquip?.Invoke(model.wearableId);
                unequipButton.gameObject.SetActive(false);
            });
            ResetSlot();
            tooltipContainer.gameObject.SetActive(true);
            tooltipDefaultPosition = new Vector2(30, 120);
            tooltipFullPosition = new Vector2(30, 150);
            tooltipContainer.anchoredPosition = tooltipDefaultPosition;
            tooltipContainer.gameObject.SetActive(false);
        }

        public void ResetSlot()
        {
            SetRarity(null);
            SetNftImage("");
            RefreshControl();
            SetWearableId("");
            SetHideList(Array.Empty<string>());
        }

        public string[] GetHideList() =>
            model.hidesList;

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetCategory(model.category);
            SetNftImage(model.imageUri);
            SetRarity(model.rarity);
            SetIsHidden(model.isHidden, model.hiddenBy);
            SetWearableId(model.wearableId);
            SetHideList(model.hidesList);
        }

        public void SetHideList(string[] hideList) =>
            model.hidesList = hideList;

        public void SetIsHidden(bool isHidden, string hiddenBy)
        {
            model.isHidden = isHidden;
            model.hiddenBy = hiddenBy;
            hiddenSlot.SetActive(isHidden);

            if (isHidden)
            {
                ShakeAnimation(nftContainer);
                emptySlot.SetActive(false);
                tooltipContainer.anchoredPosition = tooltipFullPosition;
                tooltipHiddenText.gameObject.SetActive(true);
                tooltipHiddenText.text = $"Hidden by: {hiddenBy}";
                hiddenByList.Add(hiddenBy);
            }
            else
            {
                hiddenByList.Remove(hiddenBy);
                tooltipHiddenText.gameObject.SetActive(false);
                tooltipContainer.anchoredPosition = tooltipDefaultPosition;

                if (hiddenByList.Count > 0)
                {
                    hiddenSlot.SetActive(true);
                    tooltipContainer.anchoredPosition = tooltipFullPosition;
                    tooltipHiddenText.gameObject.SetActive(true);
                    tooltipHiddenText.text = $"Hidden by: {hiddenByList.Last()}";
                }
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
                nftImage.SetLoadingIndicatorVisible(false);
                emptySlot.SetActive(true);
                return;
            }

            emptySlot.SetActive(false);
            nftImage.SetImage(imageUri);
        }

        public void SetRarity(string rarity)
        {
            model.rarity = rarity;
            backgroundRarityImage.sprite = string.IsNullOrEmpty(rarity) ? null : rarityBackgrounds.GetRarityImage(rarity);
        }

        public void SetWearableId(string wearableId)
        {
            model.wearableId = wearableId;
        }

        public override void OnFocus()
        {
            focusedImage.enabled = true;
            tooltipContainer.gameObject.SetActive(true);
            if(!emptySlot.activeInHierarchy)
                unequipButton.gameObject.SetActive(true);

            ScaleUpAnimation(focusedImage.transform);
        }

        public override void OnLoseFocus()
        {
            focusedImage.enabled = false;
            tooltipContainer.gameObject.SetActive(false);
            unequipButton.gameObject.SetActive(false);
        }

        public void OnSlotClick()
        {
            isSelected = !isSelected;

            if (isSelected)
            {
                selectedImage.enabled = true;
                ScaleUpAnimation(selectedImage.transform);
            }
            else
            {
                ScaleDownAndResetAnimation(selectedImage);
            }

            OnSelectAvatarSlot?.Invoke(model.category, isSelected);
        }


        public void OnPointerClickOnDifferentSlot()
        {
            isSelected = false;
            selectedImage.enabled = false;
        }

        private void ScaleUpAnimation(Transform targetTransform)
        {
            targetTransform.transform.localScale = new Vector3(0, 0, 0);
            targetTransform.transform.DOScale(1, ANIMATION_TIME).SetEase(Ease.OutBack);
        }

        private void ScaleDownAndResetAnimation(Image targetImage)
        {
            targetImage.transform.DOScale(0, ANIMATION_TIME).SetEase(Ease.OutBack).OnComplete(() =>
            {
                targetImage.enabled = false;
                targetImage.transform.localScale = new Vector3(1, 1, 1);
            });
        }

        private void ShakeAnimation(Transform targetTransform) =>
            targetTransform.DOShakePosition(SHAKE_ANIMATION_TIME, 4);
    }
}
