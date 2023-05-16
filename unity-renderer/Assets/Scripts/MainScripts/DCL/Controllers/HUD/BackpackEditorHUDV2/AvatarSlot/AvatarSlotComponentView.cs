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

        [SerializeField] internal NftTypeColorSupportingSO typeColorSupporting;
        [SerializeField] internal NftTypeIconSO typeIcons;
        [SerializeField] internal RectTransform nftContainer;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] internal Image typeImage;
        [SerializeField] internal ImageComponentView nftImage;
        [SerializeField] internal Image backgroundRarityImage;
        [SerializeField] internal Image focusedImage;
        [SerializeField] internal Image selectedImage;
        [SerializeField] private GameObject emptySlot;
        [SerializeField] internal GameObject hiddenSlot;
        [SerializeField] internal RectTransform tooltipContainer;
        [SerializeField] internal TMP_Text tooltipCategoryText;
        [SerializeField] internal TMP_Text tooltipHiddenText;
        [SerializeField] internal Button button;
        [SerializeField] internal Button unequipButton;

        public event Action<AvatarSlotComponentModel, bool> OnSelectAvatarSlot;
        public event Action<string> OnUnEquip;
        public event Action<string> OnFocusHiddenBy;
        private bool isSelected = false;
        private readonly HashSet<string> hiddenByList = new HashSet<string>();
        private Vector2 tooltipDefaultPosition;
        private Vector2 tooltipFullPosition;
        private Vector2 nftContainerDefaultPosition;

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
            InitializeTooltipPositions();
        }

        private void InitializeTooltipPositions()
        {
            tooltipContainer.gameObject.SetActive(true);
            tooltipDefaultPosition = new Vector2(30, 120);
            tooltipFullPosition = new Vector2(30, 150);
            tooltipContainer.anchoredPosition = tooltipDefaultPosition;
            tooltipContainer.gameObject.SetActive(false);
            nftContainerDefaultPosition = nftContainer.anchoredPosition;
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
            SetRarity(model.rarity);
            SetIsHidden(model.isHidden, model.hiddenBy);
            SetNftImage(model.imageUri);
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
                emptySlot.SetActive(string.IsNullOrEmpty(model.imageUri));

                if (hiddenByList.Count > 0)
                {
                    emptySlot.SetActive(false);
                    tooltipContainer.anchoredPosition = tooltipFullPosition;
                    tooltipHiddenText.gameObject.SetActive(true);
                    tooltipHiddenText.text = $"Hidden by: {hiddenByList.Last()}";
                }
            }
        }

        public void SetCategory(string category)
        {
            model.category = category;
            model.allowsColorChange = typeColorSupporting.IsColorSupportedByType(category);
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

            if (!string.IsNullOrEmpty(model.imageUri))
                unequipButton.gameObject.SetActive(true);

            if(model.isHidden)
                OnFocusHiddenBy?.Invoke(model.hiddenBy);

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

            OnSelectAvatarSlot?.Invoke(model, isSelected);
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

        public void ShakeAnimation() =>
            nftContainer.DOShakePosition(SHAKE_ANIMATION_TIME, 4).OnComplete(() => nftContainer.anchoredPosition = nftContainerDefaultPosition);
    }
}
