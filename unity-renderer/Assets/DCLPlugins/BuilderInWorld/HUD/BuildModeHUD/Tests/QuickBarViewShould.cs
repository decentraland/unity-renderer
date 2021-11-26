using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class QuickBarViewShould
    {
        private QuickBarView quickBarView;

        [SetUp]
        public void SetUp()
        {
            quickBarView = QuickBarView.Create();
            quickBarView.generalCanvas = new GameObject().AddComponent<Canvas>();
        }

        [TearDown]
        public void TearDown() { Object.Destroy(quickBarView.gameObject); }

        [Test]
        public void SelectQuickBarObjectCorrectly()
        {
            // Arrange
            int selectedObjectIndex = -1;
            int indexToSelect = 3;
            quickBarView.OnQuickBarObjectSelected += (index) => selectedObjectIndex = index;

            // Act
            quickBarView.QuickBarObjectSelected(indexToSelect);

            // Assert
            Assert.AreEqual(indexToSelect, selectedObjectIndex, "The selected object index does not match!");
        }

        [Test]
        public void SetIndexToBeginDragCorrectly()
        {
            // Arrange
            int draggedObjectIndex = -1;
            int indexToBeginDrag = 3;
            quickBarView.OnSetIndexToBeginDrag += (index) => draggedObjectIndex = index;

            // Act
            quickBarView.SetIndexToBeginDrag(indexToBeginDrag);

            // Assert
            Assert.AreEqual(indexToBeginDrag, draggedObjectIndex, "The dragged object index does not match!");
        }

        [Test]
        public void SetIndexToDropCorrectly()
        {
            // Arrange
            int dropObjectIndex = -1;
            int indexToDrop = 3;
            quickBarView.OnSetIndexToDrop += (index) => dropObjectIndex = index;

            // Act
            quickBarView.SetIndexToDrop(indexToDrop);

            // Assert
            Assert.AreEqual(indexToDrop, dropObjectIndex, "The drop object index does not match!");
        }

        [Test]
        public void DropSceneObjectFromQuickBarCorrectly()
        {
            // Arrange
            Texture testTexture = null;
            int testFromIndex = 0;
            int testToIndex = 1;
            Texture returnedTexture;
            int returnedFromIndex = 0;
            int returnedToIndex = 1;

            quickBarView.OnSceneObjectDroppedFromQuickBar += (fromIndex, toIndex, texture) =>
            {
                returnedFromIndex = fromIndex;
                returnedToIndex = toIndex;
                returnedTexture = texture;
            };

            // Act
            quickBarView.SceneObjectDroppedFromQuickBar(testFromIndex, testToIndex, testTexture);

            // Assert
            Assert.AreEqual(returnedFromIndex, testFromIndex, "The returnedFromIndex does not match!");
            Assert.AreEqual(returnedToIndex, testToIndex, "The returnedToIndex does not match!");
        }

        [Test]
        public void DropSceneObjectFromCatalogCorrectly()
        {
            // Arrange
            BaseEventData droppedObject = null;
            BaseEventData objectToDrop = new BaseEventData(null);
            quickBarView.OnSceneObjectDroppedFromCatalog += (data) => droppedObject = data;

            // Act
            quickBarView.SceneObjectDroppedFromCatalog(objectToDrop);

            // Assert
            Assert.IsNotNull(droppedObject, "The dropped object is null!");
            Assert.AreEqual(objectToDrop, droppedObject, "The dropped object does not match!");
        }

        [Test]
        public void TriggerQuickBarInputCorrectly()
        {
            // Arrange
            int triggeredIndex = -1;
            int indexToDrop = 3;
            quickBarView.OnQuickBarInputTriggered += (index) => triggeredIndex = index;

            // Act
            quickBarView.OnQuickBarInputTriggedered(indexToDrop);

            // Assert
            Assert.AreEqual(indexToDrop, triggeredIndex, "The triggered index does not match!");
        }

        [Test]
        public void BeginDragSlotCorrectly()
        {
            // Arrange
            int testIndex = 0;
            quickBarView.lastIndexToBeginDrag = -1;
            quickBarView.shortcutsImgs[0].SetTexture(new Texture2D(10, 10));


            // Act
            quickBarView.BeginDragSlot(testIndex);

            // Assert
            Assert.AreEqual(testIndex, quickBarView.lastIndexToBeginDrag, "The lastIndexToBeginDrag does not match!");
            Assert.IsTrue(quickBarView.draggedSlot.isActiveAndEnabled, "The draggedSlot is not active!");
            Assert.IsTrue(quickBarView.draggedSlot.image.isActiveAndEnabled, "The draggedSlot image is not active!");
        }

        [Test]
        public void DragSlotCorrectly()
        {
            // Arrange
            PointerEventData testEventData = new PointerEventData(null);
            testEventData.position = new Vector2(5, 3);
            int testIndex = 0;
            quickBarView.draggedSlot.slotTransform.position = Vector3.zero;
            quickBarView.shortcutsImgs[0].SetTexture(new Texture2D(10, 10));


            // Act
            quickBarView.DragSlot(testEventData, testIndex);

            // Assert
            Assert.AreEqual(testEventData.position, (Vector2)quickBarView.draggedSlot.slotTransform.position, "The draggedSlot position does not match!");
        }

        [Test]
        public void EndDragSlotCorrectly()
        {
            // Arrange
            int testIndex = 0;
            int returnedIndex = -1;
            quickBarView.shortcutsImgs[0].SetTexture(new Texture2D(10, 10));
            quickBarView.OnQuickBarObjectSelected += (index) => { returnedIndex = index; };


            // Act
            quickBarView.EndDragSlot(testIndex);

            // Assert
            Assert.IsFalse(quickBarView.draggedSlot.image.isActiveAndEnabled, "The draggedSlot image is active!");
            Assert.IsFalse(quickBarView.draggedSlot.isActiveAndEnabled, "The draggedSlot is active!");
            Assert.AreEqual(testIndex, returnedIndex, "The selected index does not match!");
        }

        [Test]
        public void CancelCurrentDraggingCorrectly()
        {
            // Arrange
            quickBarView.lastIndexToBeginDrag = 5;

            // Act
            quickBarView.CancelCurrentDragging();

            // Assert
            Assert.AreEqual(-1, quickBarView.lastIndexToBeginDrag, "The lastIndexToBeginDrag does not match!");
        }
    }
}