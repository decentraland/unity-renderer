using DCL;
using DCL.Camera;
using Features.ScreenshotCamera.Scripts;
using NSubstitute;
using NUnit.Framework;
using System;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCLServices.QuestsService.Tests
{
    public class CameraControllerMock : CameraController
    {
        public override void SetCameraEnabledState(bool _) { }

        public override Camera GetCamera() =>
            new GameObject().AddComponent<Camera>();
    }

    public class ScreenshotCaptureMock : ScreenshotCapture
    {
        public ScreenshotCaptureMock() : base(null, null, null, null) { }

        public override byte[] CaptureScreenshot() =>
            Array.Empty<byte>();
    }

    public class ScreenshotHUDViewMock : ScreenshotHUDView
    {
        public override void SwitchVisibility(bool isVisible) { }
    }

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
            screenshotCamera.screenshotHUDViewPrefab = gameObject.AddComponent<ScreenshotHUDViewMock>();

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

            screenshotCamera.screenshotCaptureLazyValue = new ScreenshotCaptureMock();
            screenshotCamera.InstantiateCameraObjects();

            screenshotCamera.avatarsLODControllerLazyValue = Substitute.For<IAvatarsLODController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(screenshotCamera.gameObject);
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
        public void ToggleScreenshotCamera_WhenGuest_DoesNothing()
        {
            // Arrange
            screenshotCamera.isGuestLazyValue = true;

            // Act
            screenshotCamera.ToggleScreenshotCamera(new DCLAction_Trigger());

            // Assert
            Assert.That(screenshotCamera.isScreenshotCameraActive.Get(), Is.False);
        }

        // [Test]
        // public void CaptureScreenshot_WhenInScreenshotModeAndNotGuest_UploadsScreenshot()
        // {
        //     // Arrange
        //     screenshotCamera.isGuestLazyValue = false;
        //     screenshotCamera.isInScreenshotMode = true;
        //     screenshotCamera.cameraReelNetworkService = Substitute.For<ICameraReelNetworkService>();
        //     screenshotCamera.screenshotCapture = Substitute.For<ScreenshotCapture>();
        //
        //     // Act
        //     screenshotCamera.CaptureScreenshot(Arg.Any<DCLAction_Trigger>());
        //
        //     // Assert
        //     // Now we should expect that UploadScreenshot was called.
        //     screenshotCamera.cameraReelNetworkService.Received().UploadScreenshot(Arg.Any<byte[]>(), Arg.Any<ScreenshotMetadata>());
        // }

        // [Test]
        // public void CaptureScreenshot_WhenNotInScreenshotModeOrGuest_DoesNothing()
        // {
        //     // Arrange
        //     screenshotCamera.isGuestLazyValue = true;
        //     screenshotCamera.isInScreenshotMode = false;
        //
        //     // Act
        //     screenshotCamera.CaptureScreenshot(Arg.Any<DCLAction_Trigger>());
        //
        //     // Assert
        //     // We assume that cameraReelNetworkService has a method called UploadScreenshot that we can substitute.
        //     // If the UploadScreenshot method was not called, then we know CaptureScreenshot didn't do anything.
        //     screenshotCamera.cameraReelNetworkService.DidNotReceive().UploadScreenshot(Arg.Any<byte[]>(), Arg.Any<ScreenshotMetadata>());
        // }
    }
}
