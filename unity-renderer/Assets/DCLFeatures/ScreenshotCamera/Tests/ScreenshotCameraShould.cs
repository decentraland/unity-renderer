using DCL;
using DCL.Camera;
using DCLFeatures.ScreenshotCamera;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCLServices.QuestsService.Tests
{
    [Category("EditModeCI")]
    public class ScreenshotCameraShould
    {
        private ScreenshotCamera screenshotCamera;

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
            screenshotCamera = gameObject.AddComponent<ScreenshotCamera>();

            // Mock prefab dependencies
            cameraInputAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            takeScreenshotAction = ScriptableObject.CreateInstance<InputAction_Trigger>();

            screenshotCamera.cameraPrefab = gameObject.AddComponent<Camera>();
            screenshotCamera.screenshotHUDViewPrefab = gameObject.AddComponent<ScreenshotHUDViewDummy>();

            screenshotCamera.characterController = gameObject.AddComponent<DCLCharacterController>();
            screenshotCamera.cameraController = gameObject.AddComponent<CameraControllerMock>();
            screenshotCamera.cameraInputAction = cameraInputAction;
            screenshotCamera.takeScreenshotAction = takeScreenshotAction;

            // Mock external dependencies
            screenshotCamera.isScreenshotCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();

            allUIHidden = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraModeInputLocked = ScriptableObject.CreateInstance<BooleanVariable>();
            isScreenshotCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();
            cameraBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            featureKeyTriggersBlocked = ScriptableObject.CreateInstance<BooleanVariable>();
            userMovementKeysBlocked = ScriptableObject.CreateInstance<BooleanVariable>();

            cameraLeftMouseButtonCursorLock = Substitute.For<BaseVariable<bool>>();

            // Inject dependencies
            screenshotCamera.SetExternalDependencies(allUIHidden, cameraModeInputLocked, cameraLeftMouseButtonCursorLock,
                cameraBlocked, featureKeyTriggersBlocked, userMovementKeysBlocked, isScreenshotCameraActive);

            screenshotCamera.screenshotCaptureLazyValue = new ScreenshotCaptureDummy();
            screenshotCamera.InstantiateCameraObjects();

            screenshotCamera.avatarsLODControllerLazyValue = Substitute.For<IAvatarsLODController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(screenshotCamera.gameObject);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenGuest_DoesNothing_ExternalVarsNotToggled()
        {
            // Arrange
            screenshotCamera.isGuestLazyValue = true;
            screenshotCamera.screenshotCamera.gameObject.SetActive(false);

            // Act
            screenshotCamera.ToggleScreenshotCamera(new DCLAction_Trigger());

            // Assert
            Assert.IsFalse(screenshotCamera.isScreenshotCameraActive.Get());
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
            screenshotCamera.isGuestLazyValue = false;
            screenshotCamera.screenshotCamera.gameObject.SetActive(false);

            // Act
            screenshotCamera.ToggleScreenshotCamera(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(allUIHidden.Get());
            Assert.IsFalse(cameraModeInputLocked.Get());
            Assert.IsTrue(cameraBlocked.Get());
            Assert.IsTrue(featureKeyTriggersBlocked.Get());
            Assert.IsTrue(userMovementKeysBlocked.Get());
            Assert.IsTrue(screenshotCamera.isScreenshotCameraActive.Get());
            cameraLeftMouseButtonCursorLock.Received().Set(true);
        }

        [Test]
        public void ToggleScreenshotCamera_WhenNotGuest_DeactivatesScreenshotCamera_CorrectlyTogglesVariables()
        {
            // Arrange
            screenshotCamera.isGuestLazyValue = false;
            screenshotCamera.screenshotCamera.gameObject.SetActive(true);

            // Act
            screenshotCamera.ToggleScreenshotCamera(new DCLAction_Trigger());

            // Assert
            Assert.IsFalse(screenshotCamera.isScreenshotCameraActive.Get());
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

    public class ScreenshotCaptureDummy : ScreenshotCapture
    {
        public ScreenshotCaptureDummy() : base(null, null, null, null) { }

        public override Texture2D CaptureScreenshot() =>
            new (1, 1);
    }

    public class ScreenshotHUDViewDummy : ScreenshotHUDView
    {
        public override void SwitchVisibility(bool isVisible) { }
    }
}
