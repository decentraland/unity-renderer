using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class InfoCardComponentView : BaseComponentView<InfoCardComponentModel>, IInfoCardComponentView
    {
        [SerializeField] internal NftTypeIconSO typeIcons;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] internal NftRarityBackgroundSO rarityNftBackgrounds;
        [SerializeField] internal TMP_Text wearableName;
        [SerializeField] internal TMP_Text wearableDescription;
        [SerializeField] internal Image categoryImage;
        [SerializeField] internal Button equipButton;
        [SerializeField] internal Button unEquipButton;
        [SerializeField] internal Button viewMore;
        [SerializeField] internal Image background;
        [SerializeField] internal Image nftBackground;
        [SerializeField] internal ImageComponentView nftImage;
        [SerializeField] internal RectTransform dynamicSection;
        [SerializeField] internal DynamicListComponentView hidesList;
        [SerializeField] internal DynamicListComponentView hiddenByDynamicList;

        public event Action OnEquipWearable;
        public event Action OnUnEquipWearable;
        public event Action OnViewMore;

        internal InfoCardComponentModel Model => model;

        public void Start()
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => OnEquipWearable?.Invoke());
            unEquipButton.onClick.RemoveAllListeners();
            unEquipButton.onClick.AddListener(() => OnUnEquipWearable?.Invoke());
            viewMore.onClick.RemoveAllListeners();
            viewMore.onClick.AddListener(() => OnViewMore?.Invoke());
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetName(model.name);
            SetDescription(model.description);
            SetCategory(model.category);
            SetRarity(model.rarity);
            SetIsEquipped(model.isEquipped);
            SetRemovesList(model.removeList);
            SetHidesList(model.hideList);
            SetHiddenBy(model.hiddenBy);
            SetNftImage(model.imageUri);
            SetWearableId(model.wearableId);
        }

        public void SetName(string nameText)
        {
            model.name = nameText;
            wearableName.text = nameText;
        }

        public void SetDescription(string description)
        {
            model.description = description;
            wearableDescription.text = description;
        }

        public void SetCategory(string category)
        {
            model.category = category;
            categoryImage.sprite = typeIcons.GetTypeImage(category);
        }

        public void SetNftImage(string imageUri)
        {
            model.imageUri = imageUri;

            if (string.IsNullOrEmpty(imageUri))
            {
                nftImage.SetImage(Texture2D.grayTexture);
                return;
            }

            nftImage.SetImage(imageUri);
        }

        public void SetRarity(string rarity)
        {
            model.rarity = rarity;
            background.sprite = rarityBackgrounds.GetRarityImage(rarity);
            nftBackground.sprite = rarityNftBackgrounds.GetRarityImage(rarity);
        }

        public void SetHidesList(List<string> hideList)
        {
            model.hideList = hideList;
            hidesList.RemoveIcons();

            hidesList.gameObject.SetActive(hideList.Count != 0);
            foreach (string hideCategory in hideList)
                hidesList.AddIcon(typeIcons.GetTypeImage(hideCategory));

            Utils.ForceRebuildLayoutImmediate(dynamicSection);
        }

        public void SetRemovesList(List<string> removeList)
        {
            model.removeList = removeList;
        }

        public void SetIsEquipped(bool isEquipped)
        {
            model.isEquipped = isEquipped;

            equipButton.gameObject.SetActive(!isEquipped);
            unEquipButton.gameObject.SetActive(model.unEquipAllowed && isEquipped);
        }

        public void SetWearableId(string wearableId)
        {
            model.wearableId = wearableId;
        }

        public void Equip(string wearableId)
        {
            if(model.wearableId == wearableId)
                SetIsEquipped(true);
        }

        public void UnEquip(string wearableId)
        {
            if(model.wearableId == wearableId)
                SetIsEquipped(false);
        }

        public void SetHiddenBy(string hiddenBy)
        {
            model.hiddenBy = hiddenBy;

            if (string.IsNullOrEmpty(hiddenBy))
            {
                hiddenByDynamicList.gameObject.SetActive(false);
                return;
            }

            hiddenByDynamicList.gameObject.SetActive(true);
            hiddenByDynamicList.RemoveIcons();
            hiddenByDynamicList.AddIcon(typeIcons.GetTypeImage(hiddenBy));
        }

        public void SetVisible(bool visible) =>
            gameObject.SetActive(visible);
    }
}
