using Cinemachine;
using DCL;
using DCL.Camera;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using DCLFeatures.ScreencaptureCamera.UI;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.Tests
{
    public class ScreencaptureCameraFactoryMock : ScreencaptureCameraFactory
    {
        public override PlayerName CreatePlayerNameUI(PlayerName playerNamePrefab, float minPlayerNameHeight, DataStore_Player player, PlayerAvatarController playerAvatar) =>
            Substitute.For<PlayerName>();

        public override (ScreencaptureCameraHUDController, ScreencaptureCameraHUDView) CreateHUD(ScreencaptureCameraBehaviour mainBehaviour, ScreencaptureCameraHUDView viewPrefab, ScreencaptureCameraInputSchema inputActionsSchema)
        {
            ScreencaptureCameraHUDView screencaptureCameraHUDView = Object.Instantiate(viewPrefab);

            var screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView,
                mainBehaviour, inputActionsSchema, DataStore.i);

            return (screencaptureCameraHUDController, screencaptureCameraHUDView);
        }

        public override Camera CreateScreencaptureCamera(Camera cameraPrefab, Transform characterCameraTransform, Transform parent, int layer, CharacterController cameraTarget,
            CinemachineVirtualCamera virtualCamera)
        {
            Camera screenshotCamera = Object.Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation, parent);
            return screenshotCamera;
        }
    }

    public class ScreencaptureCameraShould
    {
        private ScreencaptureCameraBehaviour screencaptureCameraBehaviour;

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
            screencaptureCameraBehaviour = gameObject.AddComponent<ScreencaptureCameraBehaviour>();
            screencaptureCameraBehaviour.inputActionsSchema = new ScreencaptureCameraInputSchema();

            // Mock prefab dependencies
            screencaptureCameraBehaviour.cameraPrefab = gameObject.AddComponent<Camera>();
            screencaptureCameraBehaviour.screencaptureCameraHUDViewPrefab = gameObject.AddComponent<ScreencaptureCameraHUDViewDummy>();
            screencaptureCameraBehaviour.characterController = gameObject.AddComponent<DCLCharacterController>();
            screencaptureCameraBehaviour.cameraController = gameObject.AddComponent<CameraControllerMock>();

            screencaptureCameraBehaviour.inputActionsSchema.ToggleScreenshotCameraAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            screencaptureCameraBehaviour.inputActionsSchema.TakeScreenshotAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            screencaptureCameraBehaviour.inputActionsSchema.CloseWindowAction = ScriptableObject.CreateInstance<InputAction_Trigger>();
            screencaptureCameraBehaviour.inputActionsSchema.ToggleCameraReelAction = ScriptableObject.CreateInstance<InputAction_Trigger>();

            // Mock external dependencies
            screencaptureCameraBehaviour.isScreencaptureCameraActive = ScriptableObject.CreateInstance<BooleanVariable>();
            Camera cameraObject = new GameObject().AddComponent<Camera>();
            screencaptureCameraBehaviour.mainCamera = cameraObject;

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

            screencaptureCameraBehaviour.factory = new ScreencaptureCameraFactoryMock();
            screencaptureCameraBehaviour.InstantiateCameraObjects();

            screencaptureCameraBehaviour.avatarsLODControllerLazyValue = Substitute.For<IAvatarsLODController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(screencaptureCameraBehaviour.mainCamera.gameObject);
            Object.DestroyImmediate(screencaptureCameraBehaviour.gameObject);
        }

        [Test] [Category("EditModeCI")]
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
