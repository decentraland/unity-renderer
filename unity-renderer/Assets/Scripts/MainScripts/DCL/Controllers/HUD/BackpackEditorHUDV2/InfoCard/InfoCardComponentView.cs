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
        [SerializeField] internal DynamicListComponentView removesList;
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

            foreach (string hideCategory in hideList)
                hidesList.AddIcon(typeIcons.GetTypeImage(hideCategory));
        }

        public void SetRemovesList(List<string> removeList)
        {
            model.removeList = removeList;

            removesList.RemoveIcons();

            foreach (string removeCategory in removeList)
                removesList.AddIcon(typeIcons.GetTypeImage(removeCategory));
        }

        public void SetIsEquipped(bool isEquipped)
        {
            model.isEquipped = isEquipped;

            equipButton.gameObject.SetActive(!isEquipped);
            unEquipButton.gameObject.SetActive(isEquipped);
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
    }
}
