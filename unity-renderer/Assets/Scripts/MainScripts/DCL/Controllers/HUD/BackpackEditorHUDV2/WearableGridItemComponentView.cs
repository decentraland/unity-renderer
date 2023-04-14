using System.Collections.Generic;
using System.Linq;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class WearableGridItemComponentView : BaseComponentView<WearableGridItemModel>
    {
        internal struct RareBackground
        {
            public NftRarity Rarity;
            public GameObject Container;
        }

        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal GameObject readyContainer;
        [SerializeField] internal GameObject equippedContainer;
        [SerializeField] internal GameObject selectedContainer;
        [SerializeField] internal GameObject hoverUnequippedContainer;
        [SerializeField] internal GameObject hoverEquippedContainer;
        [SerializeField] internal GameObject isNewContainer;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal RareBackground[] backgroundsByRarityConfiguration;

        private Dictionary<NftRarity, GameObject> backgroundsByRarity;

        public override void Awake()
        {
            base.Awake();

            backgroundsByRarity = backgroundsByRarityConfiguration
               .ToDictionary(background => background.Rarity, background => background.Container);
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
            loadingContainer.SetActive(model.IsLoading);
            readyContainer.SetActive(!model.IsLoading);
            equippedContainer.SetActive(model.IsEquipped);
            selectedContainer.SetActive(model.IsSelected);
            hoverEquippedContainer.SetActive(model.IsEquipped && isFocused);
            hoverUnequippedContainer.SetActive(!model.IsEquipped && isFocused);
            isNewContainer.SetActive(model.IsNew);
            image.SetImage(model.ImageUrl);

            foreach ((NftRarity _, GameObject container) in backgroundsByRarity)
                container.SetActive(false);

            backgroundsByRarity[model.Rarity].SetActive(true);
        }
    }
}
