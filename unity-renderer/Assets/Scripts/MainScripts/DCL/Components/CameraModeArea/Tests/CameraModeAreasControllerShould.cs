using DCL.CameraTool;
using DCL.Components;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class CameraModeAreasControllerShould
    {
        private CameraModeAreasController controller;

        [SetUp]
        public void SetUp()
        {
            controller = new CameraModeAreasController();
        }

        [Test]
        public void ApplyTriggersPrioritiesCorrectly()
        {
            var modes = new[]
            {
                CameraMode.ModeId.FirstPerson,
                CameraMode.ModeId.BuildingToolGodMode,
                CameraMode.ModeId.ThirdPerson
            };

            ICameraModeArea[] areas = new ICameraModeArea[modes.Length];

            for (int i = 0; i < areas.Length; i++)
            {
                var mode = modes[i];
                areas[i] = Substitute.For<ICameraModeArea>();
                areas[i].cameraMode.Returns(mode);
            }

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            Assert.IsFalse(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.AddInsideArea(areas[0]);
            Assert.AreEqual(modes[0], CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.AddInsideArea(areas[1]);
            Assert.AreEqual(modes[1], CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.AddInsideArea(areas[2]);
            Assert.AreEqual(modes[2], CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.RemoveInsideArea(areas[0]);
            Assert.AreEqual(modes[2], CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.RemoveInsideArea(areas[2]);
            Assert.AreEqual(modes[1], CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.RemoveInsideArea(areas[1]);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());
            Assert.IsFalse(CommonScriptableObjects.cameraModeInputLocked.Get());
        }

        [Test]
        public void UpdateComponentModeCorrectly()
        {
            ICameraModeArea[] areas = new ICameraModeArea[2];
            areas[0] = Substitute.For<ICameraModeArea>();
            areas[0].cameraMode.Returns(CameraMode.ModeId.BuildingToolGodMode);

            areas[1] = Substitute.For<ICameraModeArea>();
            areas[1].cameraMode.Returns(CameraMode.ModeId.FirstPerson);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            Assert.IsFalse(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.AddInsideArea(areas[0]);
            Assert.AreEqual(CameraMode.ModeId.BuildingToolGodMode, CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            controller.AddInsideArea(areas[1]);
            Assert.AreEqual(CameraMode.ModeId.FirstPerson, CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());

            areas[1].cameraMode.Returns( CameraMode.ModeId.ThirdPerson);
            controller.ChangeAreaMode(areas[1]);
            Assert.AreEqual(CameraMode.ModeId.ThirdPerson, CommonScriptableObjects.cameraMode.Get());
            Assert.IsTrue(CommonScriptableObjects.cameraModeInputLocked.Get());
            areas[1].cameraMode.Returns(CameraMode.ModeId.FirstPerson);
            
            controller.RemoveInsideArea(areas[0]);
            controller.RemoveInsideArea(areas[1]);

            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());
            Assert.IsFalse(CommonScriptableObjects.cameraModeInputLocked.Get());
        }

        [Test]
        public void ShowNotificationCorrectly()
        {
            ICameraModeArea[] areas = new ICameraModeArea[2];
            areas[0] = Substitute.For<ICameraModeArea>();
            areas[0].cameraMode.Returns(CameraMode.ModeId.BuildingToolGodMode);

            areas[1] = Substitute.For<ICameraModeArea>();
            areas[1].cameraMode.Returns(CameraMode.ModeId.FirstPerson);

            CameraModeAreasController controllerMock = Substitute.ForPartsOf<CameraModeAreasController>();

            controllerMock.AddInsideArea(areas[0]);
            controllerMock.AddInsideArea(areas[1]);
            controllerMock.RemoveInsideArea(areas[0]);
            controllerMock.RemoveInsideArea(areas[1]);

            controllerMock.Received(1).ShowCameraModeLockedNotification();
        }

        [Test]
        public void HideNotificationCorrectly()
        {
            ICameraModeArea[] areas = new ICameraModeArea[2];
            areas[0] = Substitute.For<ICameraModeArea>();
            areas[0].cameraMode.Returns(CameraMode.ModeId.BuildingToolGodMode);

            areas[1] = Substitute.For<ICameraModeArea>();
            areas[1].cameraMode.Returns(CameraMode.ModeId.FirstPerson);

            CameraModeAreasController controllerMock = Substitute.ForPartsOf<CameraModeAreasController>();

            controllerMock.AddInsideArea(areas[0]);
            controllerMock.DidNotReceive().HideCameraModeLockedNotification();

            controllerMock.AddInsideArea(areas[1]);
            controllerMock.DidNotReceive().HideCameraModeLockedNotification();

            controllerMock.RemoveInsideArea(areas[0]);
            controllerMock.DidNotReceive().HideCameraModeLockedNotification();

            controllerMock.RemoveInsideArea(areas[0]);
            controllerMock.Received(1).HideCameraModeLockedNotification();
        }
    }
}