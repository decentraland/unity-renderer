using DCL;
using DCL.Camera;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.Tests
{
    [Category("EditModeCI")]
    public class ScreencaptureCameraShould
    {
        private ScreencaptureCamera screencaptureCamera;

        private InputAction_Trigger cameraInputAction;
        private InputAction_Trigger takeScreenshotAction;

        private BooleanVariable allUIHidden;
        private BooleanVariable cameraModeInputLocked;
        private BooleanVariable isScreenshotCameraActive;
        private BooleanVariable cameraBlocked;
        private BooleanVariable featureKeyTriggersBlocked;
        private BooleanVariable userMovementKeysBlocked;
        private BaseVariable<bool> cameraLeftMouseButtonCursorLock;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject();
            screencaptureCamera = gameObject.AddComponent<ScreencaptureCamera>();

            // Mock prefab dependencies
            cameraInputAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            takeScreenshotAction = ScriptableObject.CreateInstance<InputAction_Trigger>();

            screencaptureCamera.cameraPrefab = gameObject.AddComponent<Camera>();
            screencaptureCamera.screencaptureCameraHUDViewPrefab = gameObject.AddComponent<ScreencaptureCameraHUDViewDummy>();

            screencaptureCamera.characterController = gameObject.AddComponent<DCLCharacterController>();
            screencaptureCamera.cameraController = gameObject.AddComponent<CameraControllerMock>();
            screencaptureCamera.inputActionsSchema.ToggleScreenshotCameraAction = cameraInputAction;
            screencaptureCamera.inputActionsSchema.TakeScreenshotAction = takeScreenshotAction;

            // Mock external dependencies
            screencaptureCamera.isScreencaptureCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();

            allUIHidden = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraModeInputLocked = ScriptableObject.CreateInstance<BooleanVariable>();
            isScreenshotCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            featureKeyTriggersBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            userMovementKeysBlocked = ScriptableObject.CreateInstance<BooleanVariable>();

            cameraLeftMouseButtonCursorLock = Substitute.For<BaseVariable<bool>>();

            // Inject dependencies
            screencaptureCamera.SetExternalDependencies(allUIHidden, cameraModeInputLocked, cameraLeftMouseButtonCursorLock,
                cameraBlocked, featureKeyTriggersBlocked, userMovementKeysBlocked, isScreenshotCameraActive);

            screencaptureCamera.screenRecorderLazyValue = new ScreenRecorderDummy();
            screencaptureCamera.InstantiateCameraObjects();

            screencaptureCamera.avatarsLODControllerLazyValue = Substitute.For<IAvatarsLODController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(screencaptureCamera.gameObject);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenGuest_DoesNothing_ExternalVarsNotToggled()
        {
            // Arrange
            screencaptureCamera.isGuestLazyValue = true;
            screencaptureCamera.screenshotCamera.gameObject.SetActive(false);

            // Act
            screencaptureCamera.ToggleScreenshotCamera();

            // Assert
            Assert.IsFalse(screencaptureCamera.isScreencaptureCameraActive.Get());
            Assert.IsFalse(allUIHidden.Get());
            Assert.IsFalse(cameraModeInputLocked.Get());
            Assert.IsFalse(cameraBlocked.Get());
            Assert.IsFalse(featureKeyTriggersBlocked.Get());
            Assert.IsFalse(userMovementKeysBlocked.Get());
            cameraLeftMouseButtonCursorLock.DidNotReceive().Set(true);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenNotGuest_ActivatesScreenshotCamera_CorrectlyTogglesVariables()
        {
            // Arrange
            screencaptureCamera.isGuestLazyValue = false;
            screencaptureCamera.screenshotCamera.gameObject.SetActive(false);

            // Act
            screencaptureCamera.ToggleScreenshotCamera();

            // Assert
            Assert.IsTrue(allUIHidden.Get());
            Assert.IsFalse(cameraModeInputLocked.Get());
            Assert.IsTrue(cameraBlocked.Get());
            Assert.IsTrue(featureKeyTriggersBlocked.Get());
            Assert.IsTrue(userMovementKeysBlocked.Get());
            Assert.IsTrue(screencaptureCamera.isScreencaptureCameraActive.Get());
            cameraLeftMouseButtonCursorLock.Received().Set(true);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenNotGuest_DeactivatesScreenshotCamera_CorrectlyTogglesVariables()
        {
            // Arrange
            screencaptureCamera.isGuestLazyValue = false;
            screencaptureCamera.screenshotCamera.gameObject.SetActive(true);

            // Act
            screencaptureCamera.ToggleScreenshotCamera();

            // Assert
            Assert.IsFalse(screencaptureCamera.isScreencaptureCameraActive.Get());
            Assert.IsFalse(allUIHidden.Get());
            Assert.IsFalse(cameraBlocked.Get());
            Assert.IsFalse(featureKeyTriggersBlocked.Get());
            Assert.IsFalse(userMovementKeysBlocked.Get());
            cameraLeftMouseButtonCursorLock.Received().Set(false);
        }
    }

    public class CameraControllerMock : CameraController
    {
        public override void SetCameraEnabledState(bool _) { }

        public override Camera GetCamera() =>
            new GameObject().AddComponent<Camera>();
    }

    public class ScreenRecorderDummy : ScreenRecorder
    {
        public ScreenRecorderDummy() : base(null) { }

        public override Texture2D CaptureScreenshot() =>
            new (1, 1);
    }

    public class ScreencaptureCameraHUDViewDummy : ScreencaptureCameraHUDView
    {
        public override void SetVisibility(bool _, bool __) { }
    }
}
