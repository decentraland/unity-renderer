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
        [SerializeField] internal NftTypePreviewCameraFocusConfig previewCameraFocus;
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
        [SerializeField] internal Button overriddenHide;
        [SerializeField] internal Button normalHide;

        public event Action<AvatarSlotComponentModel, bool> OnSelectAvatarSlot;
        public event Action<string> OnUnEquip;
        public event Action<string> OnFocusHiddenBy;
        public event Action<string, bool> OnHideUnhidePressed;

        private bool isSelected = false;

        private readonly HashSet<string> hiddenByList = new HashSet<string>();
        private Vector2 nftContainerDefaultPosition;

        public override void Awake()
        {
            base.Awake();

            overriddenHide.onClick.RemoveAllListeners();
            overriddenHide.onClick.AddListener(()=>
            {
                AudioScriptableObjects.hide.Play(true);
                OnHideUnhidePressed?.Invoke(model.category, false);
                SetForceRender(false);
            });
            normalHide.onClick.RemoveAllListeners();
            normalHide.onClick.AddListener(()=>
            {
                AudioScriptableObjects.show.Play(true);
                OnHideUnhidePressed?.Invoke(model.category, true);
                SetForceRender(true);
            });
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
            SetForceRender(false);
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

        public void SetForceRender(bool isOverridden)
        {
            overriddenHide.gameObject.SetActive(isOverridden);
            normalHide.gameObject.SetActive(!isOverridden);
        }

        public void SetIsHidden(bool isHidden, string hiddenBy)
        {
            model.isHidden = isHidden;

            if (isHidden)
                hiddenByList.Add(hiddenBy);
            else
                hiddenByList.Remove(hiddenBy);

            List<string> sortedList = hiddenByList.OrderBy(x => WearableItem.CATEGORIES_PRIORITY.IndexOf(x)).ToList();

            if (sortedList.Count > 0)
            {
                SetHideIconVisible(true);
                tooltipHiddenText.gameObject.SetActive(!string.IsNullOrEmpty(model.wearableId));
                tooltipHiddenText.text = $"Hidden by {WearableItem.CATEGORIES_READABLE_MAPPING[sortedList[0]]}";
                model.hiddenBy = sortedList[0];
            }
            else
            {
                SetHideIconVisible(false);
                tooltipHiddenText.gameObject.SetActive(false);
                model.hiddenBy = "";
            }
        }

        public void SetCategory(string category)
        {
            model.category = category;
            model.allowsColorChange = typeColorSupporting.IsColorSupportedByType(category);
            model.previewCameraFocus = previewCameraFocus.GetPreviewCameraFocus(category);
            typeImage.sprite = typeIcons.GetTypeImage(category);
            WearableItem.CATEGORIES_READABLE_MAPPING.TryGetValue(category, out string readableCategory);
            tooltipCategoryText.text = readableCategory;
        }

        public void SetUnEquipAllowed(bool allowUnEquip) =>
            model.unEquipAllowed = allowUnEquip;

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
            backgroundRarityImage.sprite = rarityBackgrounds.GetRarityImage(rarity);
        }

        public void SetWearableId(string wearableId) =>
            model.wearableId = wearableId;

        public override void OnFocus()
        {
            focusedImage.enabled = true;
            tooltipContainer.gameObject.SetActive(true);

            if (model.unEquipAllowed && !string.IsNullOrEmpty(model.imageUri))
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
            if (!isSelected)
                Select(true);
            else
                UnSelect(true);
        }

        public void UnSelect(bool notify)
        {
            if (!isSelected) return;
            isSelected = false;

            ScaleDownAndResetAnimation(selectedImage);

            if (notify)
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

        public void SetHideIconVisible(bool isVisible) =>
            hiddenSlot.SetActive(isVisible && !string.IsNullOrEmpty(model.wearableId));

        public void Select(bool notify)
        {
            if (isSelected) return;

            isSelected = true;
            selectedImage.enabled = true;
            ScaleUpAnimation(selectedImage.transform);

            if (notify)
                OnSelectAvatarSlot?.Invoke(model, isSelected);
        }

        public void ShakeAnimation() =>
            nftContainer.DOShakePosition(SHAKE_ANIMATION_TIME, 4).OnComplete(() => nftContainer.anchoredPosition = nftContainerDefaultPosition);
    }
}
