using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class WearableGridItemComponentView : BaseComponentView<WearableGridItemModel>
    {

        [SerializeField] internal NftRarityBackgroundSO rarityNftBackgrounds;
        [SerializeField] internal NFTTypeIconsAndColors nftTypesIcons;
        [SerializeField] internal Image nftBackground;
        [SerializeField] internal Image categoryImage;
        [SerializeField] internal Image categoryBackground;
        [SerializeField] internal GameObject selectedContainer;
        [SerializeField] internal GameObject equippedContainer;
        [SerializeField] internal GameObject hoverUnequippedContainer;
        [SerializeField] internal GameObject hoverEquippedContainer;
        [SerializeField] internal GameObject hoverSelectedUnequippedContainer;
        [SerializeField] internal GameObject hoverSelectedEquippedContainer;
        [SerializeField] internal GameObject isNewContainer;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal Button interactButton;

        private string lastThumbnailUrl;

        public WearableGridItemModel Model => model;

        public event Action<WearableGridItemModel> OnSelected;
        public event Action<WearableGridItemModel> OnEquipped;
        public event Action<WearableGridItemModel> OnUnequipped;

        public override void Awake()
        {
            base.Awake();

            interactButton.onClick.AddListener(() =>
            {
                if (model.IsSelected)
                {
                    if (model.IsEquipped)
                        OnUnequipped?.Invoke(model);
                    else
                        OnEquipped?.Invoke(model);
                }
                else
                    OnSelected?.Invoke(model);
            });
        }

        public override void OnFocus()
        {
            base.OnFocus();
            RefreshControl();
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();
            RefreshControl();
        }

        public override void RefreshControl()
        {
            selectedContainer.SetActive(model.IsSelected);
            equippedContainer.SetActive(model.IsEquipped);
            hoverEquippedContainer.SetActive(!model.IsSelected && model.IsEquipped && isFocused);
            hoverUnequippedContainer.SetActive(!model.IsSelected && !model.IsEquipped && isFocused);
            hoverSelectedUnequippedContainer.SetActive(model.IsSelected && !model.IsEquipped && isFocused);
            hoverSelectedEquippedContainer.SetActive(model.IsSelected && model.IsEquipped && isFocused);
            isNewContainer.SetActive(model.IsNew);

            // we gotta check for url changes, otherwise the image component will start a "loading" state, even if the url is the same
            if (lastThumbnailUrl != model.ImageUrl)
            {
                image.SetImage(model.ImageUrl);
                lastThumbnailUrl = model.ImageUrl;
            }

            string nftRarity = model.Rarity.ToString().ToLower();
            nftBackground.sprite = rarityNftBackgrounds.GetRarityImage(nftRarity);
            categoryBackground.color = nftTypesIcons.GetColor(nftRarity);
            categoryImage.sprite = nftTypesIcons.GetTypeImage(model.Category);
        }

        public void Unselect()
        {
            model.IsSelected = false;
            RefreshControl();
        }

        public void Select()
        {
            model.IsSelected = true;
            RefreshControl();
        }
    }
}
