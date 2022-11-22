using System.Linq;
using DCL;
using NUnit.Framework;

namespace Tests
{
    public class PreviewMenuControllerShould
    {
        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void LoadMenuCorrectly()
        {
            var controller = new PreviewMenuController();
            Assert.NotNull(controller.menuView);
            Assert.False(controller.menuView.contentContainer.activeSelf);
            controller.Dispose();
        }

        [Test]
        public void ToggleFpsCorrectly()
        {
            var controller = new PreviewMenuController();

            bool initialFPSstate = DataStore.i.debugConfig.isFPSPanelVisible.Get();
            controller.showFps.buttonReference.onClick.Invoke();
            Assert.AreNotEqual(initialFPSstate, DataStore.i.debugConfig.isFPSPanelVisible.Get());

            controller.Dispose();
        }

        [Test]
        public void ToggleBoundingBoxCorrectly()
        {
            var controller = new PreviewMenuController();

            DataStore.i.debugConfig.showSceneBoundingBoxes.AddOrSet(666, true);
            bool initialBoundingBoxState = true;
            controller.showBoundingBox.buttonReference.onClick.Invoke();
            Assert.AreNotEqual(initialBoundingBoxState,
                DataStore.i.debugConfig.showSceneBoundingBoxes.Get().FirstOrDefault(pair => pair.Key == 666).Value);

            controller.Dispose();
        }
    }
}