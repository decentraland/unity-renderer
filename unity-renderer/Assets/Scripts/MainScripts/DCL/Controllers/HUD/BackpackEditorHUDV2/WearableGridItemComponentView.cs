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
        [Serializable]
        internal struct RareBackground
        {
            public NftRarity rarity;
            public GameObject container;
        }

        [SerializeField] internal GameObject selectedContainer;
        [SerializeField] internal GameObject equippedContainer;
        [SerializeField] internal GameObject hoverUnequippedContainer;
        [SerializeField] internal GameObject hoverEquippedContainer;
        [SerializeField] internal GameObject hoverSelectedUnequippedContainer;
        [SerializeField] internal GameObject hoverSelectedEquippedContainer;
        [SerializeField] internal GameObject isNewContainer;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal Button interactButton;
        [SerializeField] internal RareBackground[] backgroundsByRarityConfiguration;

        private Dictionary<NftRarity, GameObject> backgroundsByRarity;
        private string lastThumbnailUrl;

        public WearableGridItemModel Model => model;

        public event Action<WearableGridItemModel> OnSelected;
        public event Action<WearableGridItemModel> OnEquipped;
        public event Action<WearableGridItemModel> OnUnequipped;

        public override void Awake()
        {
            base.Awake();

            backgroundsByRarity = backgroundsByRarityConfiguration
               .ToDictionary(background => background.rarity, background => background.container);

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

            foreach ((NftRarity _, GameObject container) in backgroundsByRarity)
                container.SetActive(false);

            if (backgroundsByRarity.TryGetValue(model.Rarity, out var go))
                go.SetActive(true);
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
