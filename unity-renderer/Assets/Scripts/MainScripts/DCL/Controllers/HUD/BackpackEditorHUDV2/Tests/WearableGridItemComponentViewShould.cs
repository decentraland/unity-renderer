using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;
using RareBackground = DCL.Backpack.WearableGridItemComponentView.RareBackground;

namespace DCL.Backpack
{
    public class WearableGridItemComponentViewShould
    {
        private WearableGridItemComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WearableGridItemComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/WearableGridItemView.prefab"));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
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

            Assert.IsFalse(view.hoverUnequippedContainer.activeSelf);
            Assert.IsFalse(view.hoverEquippedContainer.activeSelf);
            Assert.IsFalse(view.hoverSelectedEquippedContainer.activeSelf);
            Assert.IsFalse(view.hoverSelectedUnequippedContainer.activeSelf);
        }

        [Test]
        public void EnableHoverContainer()
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

            view.OnPointerEnter(null);

            Assert.IsTrue(view.hoverUnequippedContainer.activeSelf);
        }

        [Test]
        public void EnableHoverEquippedContainer()
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

            Assert.IsTrue(view.hoverEquippedContainer.activeSelf);
        }

        [Test]
        public void EnableHoverSelectedContainer()
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

            view.OnPointerEnter(null);

            Assert.IsTrue(view.hoverSelectedUnequippedContainer.activeSelf);
        }

        [Test]
        public void EnableHoverSelectedEquippedContainer()
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

            view.OnPointerEnter(null);

            Assert.IsTrue(view.hoverSelectedEquippedContainer.activeSelf);
        }

        [TestCaseSource(nameof(GetAllRarities))]
        public void EnableRarityBackground(NftRarity nftRarity)
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

            foreach (RareBackground background in view.backgroundsByRarityConfiguration)
                Assert.AreEqual(background.rarity == nftRarity, background.container.activeSelf);
        }

        private static IEnumerable<NftRarity> GetAllRarities() =>
            Enum.GetValues(typeof(NftRarity)).Cast<NftRarity>();
    }
}
