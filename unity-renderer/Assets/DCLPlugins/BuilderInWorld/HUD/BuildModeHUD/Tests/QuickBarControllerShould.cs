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
                Substitute.For<IDragAndDropSceneObjectController>());
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
        public void SceneObjectDroppedFromQuickBarCorrectly()
        {
            // Arrange
            Asset_Texture testTexture = new Asset_Texture();
            CatalogItem testCatalogItem = new CatalogItem { };
            int testFromIndex = 0;
            int testToIndex = 1;
            quickBarController.quickBarShortcutsCatalogItems = new CatalogItem[2];
            quickBarController.quickBarShortcutsCatalogItems[0] = testCatalogItem;
            quickBarController.quickBarShortcutsCatalogItems[1] = null;

            // Act
            quickBarController.SceneObjectDroppedFromQuickBar(testFromIndex, testToIndex, testTexture.texture);

            // Assert
            Assert.AreEqual(testCatalogItem, quickBarController.quickBarShortcutsCatalogItems[1], "The CatalogItem 1 does not match!");
            quickBarController.quickBarView.Received(1).SetTextureToShortcut(testToIndex, testTexture.texture);
            Assert.IsNull(quickBarController.quickBarShortcutsCatalogItems[testFromIndex], "The CatalogItem 0 is not null!");
            quickBarController.quickBarView.Received(1).SetShortcutAsEmpty(testFromIndex);
        }

        [Test]
        public void SceneObjectDroppedFromCatalogCorrectly()
        {
            // Act
            quickBarController.SceneObjectDroppedFromCatalog(null);

            // Assert
            quickBarController.dragAndDropController.Received(1).GetLastAdapterDragged();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SetQuickBarShortcutCorrectly(int objectIndex)
        {
            // Arrange
            string testCatalogItemId = "testId";
            quickBarController.lastIndexDroped = objectIndex;
            quickBarController.quickBarShortcutsCatalogItems = new CatalogItem[3];
            quickBarController.quickBarShortcutsCatalogItems[0] = new CatalogItem { };
            quickBarController.quickBarShortcutsCatalogItems[1] = new CatalogItem { };
            quickBarController.quickBarShortcutsCatalogItems[2] = new CatalogItem { };
            CatalogItem testItem = new CatalogItem { id = testCatalogItemId };
            Texture testTexture = new Texture2D(10, 10);

            // Act
            quickBarController.SetQuickBarShortcut(testItem, objectIndex, testTexture);

            // Assert
            Assert.AreEqual(testCatalogItemId, quickBarController.quickBarShortcutsCatalogItems[objectIndex].id);
            quickBarController.quickBarView.Received(1).SetTextureToShortcut(objectIndex, testTexture);
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

        [Test]
        public void CancelDraggingCorrectly()
        {
            // Act
            quickBarController.CancelDragging();

            // Assert
            quickBarController.quickBarView.Received(1).CancelCurrentDragging();
        }
    }
}