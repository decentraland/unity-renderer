using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class DragAndDropSceneObjectControllerShould
    {
        private DragAndDropSceneObjectController dragAndDropSceneObjectController;

        [SetUp]
        public void SetUp()
        {
            dragAndDropSceneObjectController = new DragAndDropSceneObjectController();
            dragAndDropSceneObjectController.Initialize(Substitute.For<IDragAndDropSceneObjectView>());
        }

        [TearDown]
        public void TearDown()
        {
            dragAndDropSceneObjectController.Dispose();
        }

        [Test]
        public void ClickCorrectly()
        {
            // Arrange
            bool dropped = false;
            dragAndDropSceneObjectController.OnDrop += () => { dropped = true; };

            // Act
            dragAndDropSceneObjectController.Drop();

            // Assert
            Assert.IsTrue(dropped, "dropped is false!");
        }
    }
}
