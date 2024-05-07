using DCLServices.WearablesCatalogService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class WearableGridItemComponentViewShould
    {
        private WearableGridItemComponentView view;
        private NftRarityBackgroundSO backgrounds;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WearableGridItemComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/WearableGridItemView.prefab"));

            backgrounds = AssetDatabase.LoadAssetAtPath<NftRarityBackgroundSO>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/AvatarSlot/NftWearableRarityBackground.asset");
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void TriggerSelectedEvent()
        {
            var called = true;
            view.OnSelected += m => called = true;

            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            view.interactButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerEquipEvent()
        {
            var called = true;
            view.OnEquipped += m => called = true;

            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            view.interactButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerUnEquipEvent()
        {
            var called = true;
            view.OnUnequipped += m => called = true;

            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = true,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            view.interactButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void EnableSelectedContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsTrue(view.selectedContainer.activeSelf);
        }

        [Test]
        public void DisableSelectedContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            Assert.IsFalse(view.selectedContainer.activeSelf);
        }

        [Test]
        public void EnableEquippedContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = true,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsTrue(view.equippedContainer.activeSelf);
        }

        [Test]
        public void DisableEquippedContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsFalse(view.equippedContainer.activeSelf);
        }

        [Test]
        public void EnableIsNewContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = true,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsTrue(view.isNewContainer.activeSelf);
        }

        [Test]
        public void DisableIsNewContainer()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsFalse(view.isNewContainer.activeSelf);
        }

        [Test]
        public void HoverContainersAreDisabled()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            Assert.IsFalse(view.selectedContainer.activeSelf);
        }

        [Test]
        public void EnableHover()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = true,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            view.OnPointerEnter(null);

            Assert.IsTrue(view.selectedContainer.activeSelf);
        }


        [Test]
        public void DisableHover()
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "imageUrl",
                IsEquipped = true,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            view.OnPointerExit(null);

            Assert.IsFalse(view.selectedContainer.activeSelf);
        }

        [TestCaseSource(nameof(GetAllRarities))]
        public void SetCorrectBackgroundForRarity(NftRarity nftRarity)
        {
            view.SetModel(new WearableGridItemModel
            {
                Rarity = nftRarity,
                ImageUrl = "imageUrl",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            Assert.AreEqual(view.nftBackground.sprite, backgrounds.GetRarityImage(nftRarity.ToString().ToLower()));
        }

        private static IEnumerable<NftRarity> GetAllRarities() =>
            Enum.GetValues(typeof(NftRarity)).Cast<NftRarity>();
    }
}

