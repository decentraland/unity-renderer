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
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(quickBarView.gameObject);
        }

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
        public void DropSceneObjectCorrectly()
        {
            // Arrange
            BaseEventData droppedObject = null;
            BaseEventData objectToDrop = new BaseEventData(null);
            quickBarView.OnSceneObjectDropped += (data) => droppedObject = data;

            // Act
            quickBarView.SceneObjectDropped(objectToDrop);

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
    }
}
