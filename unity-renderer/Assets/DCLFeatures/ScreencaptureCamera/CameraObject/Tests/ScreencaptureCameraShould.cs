using DCL;
using DCL.Camera;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using DCLFeatures.ScreencaptureCamera.UI;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.Tests
{
    [Category("EditModeCI")]
    public class ScreencaptureCameraShould
    {
        private CameraObject.ScreencaptureCameraBehaviour screencaptureCameraBehaviour;

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
            screencaptureCameraBehaviour = gameObject.AddComponent<CameraObject.ScreencaptureCameraBehaviour>();

            // Mock prefab dependencies
            cameraInputAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            takeScreenshotAction = ScriptableObject.CreateInstance<InputAction_Trigger>();

            screencaptureCameraBehaviour.cameraPrefab = gameObject.AddComponent<Camera>();
            screencaptureCameraBehaviour.screencaptureCameraHUDViewPrefab = gameObject.AddComponent<ScreencaptureCameraHUDViewDummy>();

            screencaptureCameraBehaviour.characterController = gameObject.AddComponent<DCLCharacterController>();
            screencaptureCameraBehaviour.cameraController = gameObject.AddComponent<CameraControllerMock>();
            screencaptureCameraBehaviour.inputActionsSchema.ToggleScreenshotCameraAction = cameraInputAction;
            screencaptureCameraBehaviour.inputActionsSchema.TakeScreenshotAction = takeScreenshotAction;

            // Mock external dependencies
            screencaptureCameraBehaviour.isScreencaptureCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();

            allUIHidden = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraModeInputLocked = ScriptableObject.CreateInstance<BooleanVariable>();
            isScreenshotCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            featureKeyTriggersBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            userMovementKeysBlocked = ScriptableObject.CreateInstance<BooleanVariable>();

            cameraLeftMouseButtonCursorLock = Substitute.For<BaseVariable<bool>>();

            // Inject dependencies
            screencaptureCameraBehaviour.SetExternalDependencies(allUIHidden, cameraModeInputLocked, cameraLeftMouseButtonCursorLock,
                cameraBlocked, featureKeyTriggersBlocked, userMovementKeysBlocked, isScreenshotCameraActive);

            screencaptureCameraBehaviour.screenRecorderLazyValue = new ScreenRecorderDummy();
            screencaptureCameraBehaviour.InstantiateCameraObjects();

            screencaptureCameraBehaviour.avatarsLODControllerLazyValue = Substitute.For<IAvatarsLODController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(screencaptureCameraBehaviour.gameObject);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenGuest_DoesNothing_ExternalVarsNotToggled()
        {
            // Arrange
            screencaptureCameraBehaviour.isGuestLazyValue = true;
            screencaptureCameraBehaviour.screenshotCamera.gameObject.SetActive(false);

            // Act
            screencaptureCameraBehaviour.ToggleScreenshotCamera();

            // Assert
            Assert.IsFalse(screencaptureCameraBehaviour.isScreencaptureCameraActive.Get());
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
            screencaptureCameraBehaviour.isGuestLazyValue = false;
            screencaptureCameraBehaviour.screenshotCamera.gameObject.SetActive(false);

            // Act
            screencaptureCameraBehaviour.ToggleScreenshotCamera();

            // Assert
            Assert.IsTrue(allUIHidden.Get());
            Assert.IsFalse(cameraModeInputLocked.Get());
            Assert.IsTrue(cameraBlocked.Get());
            Assert.IsTrue(featureKeyTriggersBlocked.Get());
            Assert.IsTrue(userMovementKeysBlocked.Get());
            Assert.IsTrue(screencaptureCameraBehaviour.isScreencaptureCameraActive.Get());
            cameraLeftMouseButtonCursorLock.Received().Set(true);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenNotGuest_DeactivatesScreenshotCamera_CorrectlyTogglesVariables()
        {
            // Arrange
            screencaptureCameraBehaviour.isGuestLazyValue = false;
            screencaptureCameraBehaviour.screenshotCamera.gameObject.SetActive(true);

            // Act
            screencaptureCameraBehaviour.ToggleScreenshotCamera();

            // Assert
            Assert.IsFalse(screencaptureCameraBehaviour.isScreencaptureCameraActive.Get());
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
