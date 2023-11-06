using DCLServices.WearablesCatalogService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class WearableGridComponentViewShould
    {
        private WearableGridComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WearableGridComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/WearablesSection.prefab"));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [TestCase(1, 4)]
        [TestCase(3, 5)]
        [TestCase(9, 26)]
        public void SetPages(int currentPage, int totalPages)
        {
            view.SetWearablePages(currentPage, totalPages);

            Assert.IsTrue(view.wearablePageSelector.gameObject.activeSelf);
            Assert.AreEqual(currentPage - 1, view.wearablePageSelector.CurrentPage);
            Assert.AreEqual(totalPages, view.wearablePageSelector.TotalPages);
        }

        [Test]
        public void HidePagesWhenOnlyOnePage()
        {
            view.SetWearablePages(1, 1);

            Assert.IsFalse(view.wearablePageSelector.gameObject.activeSelf);
        }

        [Test]
        public void ShowWearables()
        {
            view.ShowWearables(new[]
            {
                new WearableGridItemModel
                {
                    Rarity = NftRarity.Common,
                    ImageUrl = "url",
                    IsEquipped = false,
                    IsNew = false,
                    IsSelected = false,
                    WearableId = "w1",
                },
                new WearableGridItemModel
                {
                    Rarity = NftRarity.Epic,
                    ImageUrl = "url2",
                    IsEquipped = true,
                    IsNew = true,
                    IsSelected = true,
                    WearableId = "w2",
                },
            });

            WearableGridItemComponentView[] gridItems = view.wearablesGridContainer.GetComponentsInChildren<WearableGridItemComponentView>();

            Assert.AreEqual(2, gridItems.Length);
        }

        [Test]
        public void ClearWearables()
        {
            view.ShowWearables(new[]
            {
                new WearableGridItemModel
                {
                    Rarity = NftRarity.Common,
                    ImageUrl = "url",
                    IsEquipped = false,
                    IsNew = false,
                    IsSelected = false,
                    WearableId = "w1",
                },
                new WearableGridItemModel
                {
                    Rarity = NftRarity.Epic,
                    ImageUrl = "url2",
                    IsEquipped = true,
                    IsNew = true,
                    IsSelected = true,
                    WearableId = "w2",
                },
            });

            view.ClearWearables();

            WearableGridItemComponentView[] gridItems = view.wearablesGridContainer.GetComponentsInChildren<WearableGridItemComponentView>();

            Assert.AreEqual(0, gridItems.Length);
        }

        [Test]
        public void UpdateWearableWhenAlreadyExist()
        {
            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "url",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "url",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            WearableGridItemComponentView[] gridItems = view.wearablesGridContainer.GetComponentsInChildren<WearableGridItemComponentView>();

            Assert.AreEqual(1, gridItems.Length);
        }

        [Test]
        public void SelectWearable()
        {
            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "url",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                WearableId = "w1",
            });

            view.SelectWearable("w1");

            WearableGridItemComponentView[] gridItems = view.wearablesGridContainer.GetComponentsInChildren<WearableGridItemComponentView>();

            Assert.IsTrue(gridItems[0].Model.IsSelected);
        }

        [Test]
        public void ClearWearableSelection()
        {
            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "url",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Epic,
                ImageUrl = "url2",
                IsEquipped = true,
                IsNew = true,
                IsSelected = false,
                WearableId = "w2",
            });

            view.ClearWearableSelection();

            WearableGridItemComponentView[] gridItems = view.wearablesGridContainer.GetComponentsInChildren<WearableGridItemComponentView>();

            Assert.IsFalse(gridItems[0].Model.IsSelected);
            Assert.IsFalse(gridItems[1].Model.IsSelected);
        }

        [Test]
        public void FillInfoCard()
        {
            var model = new InfoCardComponentModel
            {
                category = "upper_body",
                description = "desc",
                name = "my wearable",
                rarity = "rare",
                hiddenBy = "lower_body",
                hideList = new List<string>{"eyes"},
                isEquipped = true,
                removeList = new List<string>(),
            };

            view.FillInfoCard(model);

            Assert.AreEqual(view.infoCardComponentView.Model, model);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetInfoCardVisible(bool isVisible)
        {
            view.SetInfoCardVisible(isVisible);

            Assert.AreEqual(isVisible, view.infoCardComponentView.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetLoadingActive(bool isActive)
        {
            view.SetLoadingActive(isActive);

            Assert.AreEqual(isActive, view.loadingSpinner.activeSelf);
        }

        [Test]
        public void SetBreadcrumb()
        {
            var model = new NftBreadcrumbModel
            {
                Current = 0,
                ResultCount = 64,
                Path = new (string Filter, string Name, string Type, bool Removable)[]
                {
                    (Filter: "all", Name: "All", "all", false),
                },
            };

            view.SetWearableBreadcrumb(model);

            Assert.AreEqual(view.wearablesBreadcrumbComponentView.Model, model);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableEmptyStateWhenNoWearables(bool isSignIn)
        {
            DataStore.i.common.isSignUpFlow.Set(isSignIn);
            view.ClearWearables();
            view.ShowWearables(Array.Empty<WearableGridItemModel>());

            Assert.IsTrue(isSignIn ? view.emptyStateContainerForSignUp : view.emptyStateContainer.activeSelf);
            Assert.IsFalse(view.wearablesGridContainer.gameObject.activeSelf);
        }

        [Test]
        public void DisableEmptyStateWhenShowingWearables()
        {
            view.ClearWearables();
            view.SetWearable(new WearableGridItemModel
            {
                Rarity = NftRarity.Common,
                ImageUrl = "url",
                IsEquipped = false,
                IsNew = false,
                IsSelected = true,
                WearableId = "w1",
            });

            Assert.IsFalse(view.emptyStateContainer.activeSelf);
            Assert.IsTrue(view.wearablesGridContainer.gameObject.activeSelf);
        }
    }
}
