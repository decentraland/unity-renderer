using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.BuildModeHUDControllers
{
    public class QuickBarControllerShould
    {
        private QuickBarController quickBarController;

        [SetUp]
        public void SetUp()
        {
            quickBarController = new QuickBarController();
            quickBarController.Initialize(
                Substitute.For<IQuickBarView>(),
                Substitute.For<ISceneCatalogController>());
        }

        [TearDown]
        public void TearDown()
        {
            quickBarController.Dispose();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void QuickBarObjectSelectedCorrectly(int objectIndex)
        {
            // Arrange
            CatalogItem catalogItemSelected = null;
            quickBarController.OnCatalogItemSelected += (item) => { catalogItemSelected = item; };

            quickBarController.quickBarShortcutsCatalogItems = new CatalogItem[3];
            quickBarController.quickBarShortcutsCatalogItems[0] = new CatalogItem { id = "testId1" };
            quickBarController.quickBarShortcutsCatalogItems[1] = new CatalogItem { id = "testId2" };
            quickBarController.quickBarShortcutsCatalogItems[2] = new CatalogItem { id = "testId3" };

            // Act
            CatalogItem result = quickBarController.QuickBarObjectSelected(objectIndex);

            // Assert
            Assert.AreEqual(catalogItemSelected, result, "The catalog item does not match!");
            Assert.AreEqual(catalogItemSelected.id, result.id, "The catalog item id does not match!");
        }

        [Test]
        public void SetIndexToDropCorrectly()
        {
            // Arrange
            int testLastIndexDropped = 5;
            quickBarController.lastIndexDroped = -1;

            // Act
            quickBarController.SetIndexToDrop(testLastIndexDropped);

            // Assert
            Assert.AreEqual(testLastIndexDropped, quickBarController.lastIndexDroped, "The last index dropped does not match!");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SceneObjectDroppedCorrectly(int objectIndex)
        {
            // Arrange
            quickBarController.lastIndexDroped = objectIndex;
            quickBarController.quickBarShortcutsCatalogItems = new CatalogItem[3];
            quickBarController.quickBarShortcutsCatalogItems[0] = new CatalogItem {};
            quickBarController.quickBarShortcutsCatalogItems[1] = new CatalogItem {};
            quickBarController.quickBarShortcutsCatalogItems[2] = new CatalogItem {};

            string testCatalogItemId = "testId";
            CatalogItemAdapter testCatalogAdapter = new GameObject("_CatalogItemAdapter").AddComponent<CatalogItemAdapter>();
            testCatalogAdapter.smartItemGO = new GameObject("_SmartItemGO");
            testCatalogAdapter.lockedGO = new GameObject("_LockedGO");
            testCatalogAdapter.SetContent(new CatalogItem { id = testCatalogItemId });

            Asset_Texture testTexture = new Asset_Texture();
            testCatalogAdapter.thumbnailImg = new GameObject("_RawImage").AddComponent<RawImage>();
            testCatalogAdapter.thumbnailImg.texture = testTexture.texture;
            testCatalogAdapter.thumbnailImg.enabled = true;
            quickBarController.sceneCatalogController.GetLastCatalogItemDragged().ReturnsForAnyArgs(testCatalogAdapter);

            // Act
            quickBarController.SceneObjectDropped(null);

            // Assert
            quickBarController.sceneCatalogController.Received(1).GetLastCatalogItemDragged();
            Assert.AreEqual(testCatalogItemId, quickBarController.quickBarShortcutsCatalogItems[objectIndex].id);
            quickBarController.quickBarView.Received(1).SetTextureToShortcut(objectIndex, testTexture.texture);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void QuickBarInputCorrectly(int quickBarSlotIndex)
        {
            // Arrange
            int selectedQuickBarSlot = -1;
            quickBarController.OnQuickBarShortcutSelected += (quickBarSlot) => { selectedQuickBarSlot = quickBarSlot; };

            // Act
            quickBarController.QuickBarInput(quickBarSlotIndex);

            // Assert
            Assert.AreEqual(quickBarSlotIndex, selectedQuickBarSlot, "The QuickBar Slot index does not match!");
        }
    }
}
