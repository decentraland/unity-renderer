using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class DragAndDropSceneObjectViewShould
    {
        private DragAndDropSceneObjectView dragAndDropSceneObjectView;

        [SetUp]
        public void SetUp()
        {
            dragAndDropSceneObjectView = DragAndDropSceneObjectView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(dragAndDropSceneObjectView.gameObject);
        }

        [Test]
        public void DropCorrectly()
        {
            // Arrange
            bool isDropped = false;
            dragAndDropSceneObjectView.OnDrop += () => isDropped = true;

            // Act
            dragAndDropSceneObjectView.Drop();

            // Assert
            Assert.IsTrue(isDropped, "isDropped is false!");
        }
    }
}
