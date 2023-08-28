using Cinemachine;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.CameraTool;
using DCL.Helpers;
using DCL.Skybox;
using DCL.Tasks;
using DCLFeatures.ScreencaptureCamera.UI;
using DCLServices.CameraReelService;
using System;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraBehaviour : MonoBehaviour
    {
        private const string UPLOADING_ERROR_MESSAGE = "There was an unexpected error when uploading the picture. Try again later.";
        private const string STORAGE_LIMIT_REACHED_MESSAGE = "You can't take more pictures because you have reached the storage limit of the camera reel.\nTo make room we recommend you to download your photos and then delete them.";

        private const float SPLASH_FX_DURATION = 0.5f;
        private const float MIDDLE_PAUSE_FX_DURATION = 0.1f;
        private const float IMAGE_TRANSITION_FX_DURATION = 0.5f;

        private const float MIN_PLAYERNAME_HEIGHT = 1.14f;

        [Header("MAIN COMPONENTS")]
        [SerializeField] internal Camera cameraPrefab;
        [SerializeField] internal ScreencaptureCameraHUDView screencaptureCameraHUDViewPrefab;
        [SerializeField] private PlayerName playerNamePrefab;

        [SerializeField] private CharacterController cameraTarget;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        internal Camera mainCamera;
        internal DCLCharacterController characterController;
        internal CameraController cameraController;

        internal Camera screenshotCamera;
        internal BooleanVariable isScreencaptureCameraActive;

        internal ScreenRecorder screenRecorderLazyValue;
        internal IAvatarsLODController avatarsLODControllerLazyValue;

        internal ScreencaptureCameraFactory factory = new ();

        private bool isInstantiated;
        private float lastScreenshotTime = -Mathf.Infinity;
        private CameraReelStorageStatus storageStatus;
        private string playerId;
        private PlayerName playerName;
        private CancellationTokenSource uploadPictureCancellationToken;

        // UI
        private Canvas enableCameraButton;
        private ScreencaptureCameraHUDView screencaptureCameraHUDView;
        private ScreencaptureCameraHUDController screencaptureCameraHUDController;

        // Lazy Values
        private CinemachineBrain characterCinemachineBrainLazyValue;
        private ICameraReelStorageService cameraReelStorageServiceLazyValue;

        // Cached states
        private bool prevMouseLockState;
        private bool prevMouseButtonCursorLockMode;
        private Camera prevSkyboxCamera;

        // DataStore Variables
        private BooleanVariable allUIHidden;
        private BooleanVariable cameraModeInputLocked;
        private BaseVariable<bool> cameraLeftMouseButtonCursorLock;
        private BooleanVariable cameraBlocked;
        private BooleanVariable featureKeyTriggersBlocked;
        private BooleanVariable userMovementKeysBlocked;

        private bool isInTransition;

        private InputAction_Trigger toggleScreenshotCameraAction;
        private InputAction_Trigger toggleCameraReelAction;
        private InputAction_Trigger exitScreenshotModeAction;

        public DataStore_Player Player { private get; set; }
        public bool HasStorageSpace => storageStatus.HasFreeSpace;

        private ICameraReelStorageService cameraReelStorageService => cameraReelStorageServiceLazyValue ??= Environment.i.serviceLocator.Get<ICameraReelStorageService>();
        private ICameraReelAnalyticsService analytics => Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>();

        private Transform characterCameraTransform => mainCamera.transform;
        private CinemachineBrain characterCinemachineBrain => characterCinemachineBrainLazyValue ??= mainCamera.GetComponent<CinemachineBrain>();
        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private ScreenRecorder screenRecorderLazy
        {
            get
            {
                if (isInstantiated)
                    return screenRecorderLazyValue;

                InstantiateCameraObjects();

                return screenRecorderLazyValue;
            }
        }

        private bool isOnCooldown => Time.time - lastScreenshotTime < SPLASH_FX_DURATION + IMAGE_TRANSITION_FX_DURATION + MIDDLE_PAUSE_FX_DURATION;

        internal void Awake()
        {
            toggleScreenshotCameraAction = Resources.Load<InputAction_Trigger>("ToggleScreenshotCamera");
            toggleCameraReelAction = Resources.Load<InputAction_Trigger>("ToggleCameraReelSection");
            exitScreenshotModeAction = Resources.Load<InputAction_Trigger>("CloseScreenshotCamera");

            toggleScreenshotCameraAction.OnTriggered += ToggleScreenshotCamera;
            toggleCameraReelAction.OnTriggered += OpenCameraReelGallery;

            exitScreenshotModeAction.OnTriggered += CloseScreenshotCamera;
        }

        private void Start()
        {
            playerId = Player.ownPlayer.Get().id;
            storageStatus = new CameraReelStorageStatus(0, 0);

            UpdateStorageInfo();

            characterController = DCLCharacterController.i;
            cameraController = SceneReferences.i.cameraController;
            mainCamera = cameraController.GetCamera();
        }

        internal void OnDestroy()
        {
            toggleScreenshotCameraAction.OnTriggered -= ToggleScreenshotCamera;
            toggleCameraReelAction.OnTriggered -= OpenCameraReelGallery;
            exitScreenshotModeAction.OnTriggered -= CloseScreenshotCamera;
        }

        private void OpenCameraReelGallery(DCLAction_Trigger _) =>
            OpenCameraReelGallery("Shortcut");

        private void OpenCameraReelGallery(string source)
        {
            if (isScreencaptureCameraActive.Get())
                ToggleScreenshotCamera(isEnabled: false);

            bool cameraReelWasOpen = DataStore.i.HUDs.cameraReelSectionVisible.Get();

            if (!cameraReelWasOpen)
                DataStore.i.HUDs.cameraReelOpenSource.Set(source);

            DataStore.i.HUDs.cameraReelSectionVisible.Set(!cameraReelWasOpen);
        }

        public void SetExternalDependencies(BooleanVariable allUIHidden, BooleanVariable cameraModeInputLocked, BaseVariable<bool> cameraLeftMouseButtonCursorLock,
            BooleanVariable cameraBlocked, BooleanVariable featureKeyTriggersBlocked, BooleanVariable userMovementKeysBlocked, BooleanVariable isScreenshotCameraActive)
        {
            this.allUIHidden = allUIHidden;
            this.cameraModeInputLocked = cameraModeInputLocked;
            this.cameraLeftMouseButtonCursorLock = cameraLeftMouseButtonCursorLock;
            this.cameraBlocked = cameraBlocked;
            this.featureKeyTriggersBlocked = featureKeyTriggersBlocked;
            this.userMovementKeysBlocked = userMovementKeysBlocked;
            isScreencaptureCameraActive = isScreenshotCameraActive;
        }

        public void CaptureScreenshot(string source)
        {
            if (isOnCooldown || !storageStatus.HasFreeSpace || !isScreencaptureCameraActive.Get()) return;

            lastScreenshotTime = Time.time;

            screencaptureCameraHUDController.SetVisibility(false, storageStatus.HasFreeSpace);
            StartCoroutine(screenRecorderLazy.CaptureScreenshot(SkyboxController.i.SkyboxCamera.BaseCamera, OnComplete));

            void OnComplete(Texture2D screenshot)
            {
                screencaptureCameraHUDController.SetVisibility(true, storageStatus.HasFreeSpace);
                screencaptureCameraHUDController.PlayScreenshotFX(screenshot, SPLASH_FX_DURATION, MIDDLE_PAUSE_FX_DURATION, IMAGE_TRANSITION_FX_DURATION);

                var metadata = ScreenshotMetadata.Create(Player, avatarsLODController, screenshotCamera);
                uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
                UploadScreenshotAsync(screenshot, metadata, source, uploadPictureCancellationToken.Token).Forget();
            }

            async UniTaskVoid UploadScreenshotAsync(Texture2D screenshot, ScreenshotMetadata metadata, string source, CancellationToken cancellationToken)
            {
                try
                {
                    storageStatus = await cameraReelStorageService.UploadScreenshot(screenshot, metadata, cancellationToken);

                    analytics.TakePhoto(metadata.userAddress,
                        $"{metadata.scene.location.x},{metadata.scene.location.y}",
                        metadata.visiblePeople.Length,
                        source);
                }
                catch (OperationCanceledException) { }
                catch (ScreenshotLimitReachedException) { DataStore.i.notifications.DefaultErrorNotification.Set(STORAGE_LIMIT_REACHED_MESSAGE, true); }
                catch (Exception e)
                {
                    DataStore.i.notifications.DefaultErrorNotification.Set(UPLOADING_ERROR_MESSAGE, true);
                    Debug.LogException(e);
                }
            }
        }

        public void ToggleScreenshotCamera(string source = null, bool isEnabled = true)
        {
            if (isInTransition || isEnabled == isScreencaptureCameraActive.Get()) return;

            if (isEnabled && !string.IsNullOrEmpty(source))
                analytics.OpenCamera(source);

            UpdateStorageInfo();

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);
            Utils.UnlockCursor();

            if (activateScreenshotCamera)
                OpenCameraAsync();
            else
            {
                ToggleExternalSystems(activateScreenshotCamera: false);
                ToggleCameraSystems(activateScreenshotCamera: false);
                isScreencaptureCameraActive.Set(false);
            }
        }

        private async void OpenCameraAsync()
        {
            isInTransition = true;

            cameraModeInputLocked.Set(true);
            cameraController.SetCameraMode(CameraMode.ModeId.ThirdPerson);

            await UniTask.WaitWhile(() => cameraController.CameraIsBlending);
            isInTransition = false;

            ToggleExternalSystems(activateScreenshotCamera: true);
            ToggleCameraSystems(activateScreenshotCamera: true);
            isScreencaptureCameraActive.Set(true);
        }

        private void CloseScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera("Shortcut", isEnabled: false);

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera("Shortcut", isEnabled: !isScreencaptureCameraActive.Get());

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            if (activateScreenshotCamera)
            {
                if (!isInstantiated)
                    InstantiateCameraObjects();

                prevSkyboxCamera = SkyboxController.i.SkyboxCamera.CurrentCamera;
                SkyboxController.i.AssignMainOverlayCamera(screenshotCamera.transform);
                playerName.Show();
            }
            else
            {
                SkyboxController.i.AssignMainOverlayCamera(prevSkyboxCamera.transform);
                playerName.Hide();
            }

            SetPlayerCameraActive(isActive: !activateScreenshotCamera);
            SetScreenshotCameraActive(isActive: activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : mainCamera);
        }

        private void SetScreenshotCameraActive(bool isActive)
        {
            screenshotCamera.gameObject.SetActive(isActive);
            screencaptureCameraHUDController.SetVisibility(isActive, storageStatus.HasFreeSpace);
        }

        private void SetPlayerCameraActive(bool isActive)
        {
            cameraController.SetCameraEnabledState(isActive);
            characterController.SetMovementInputToZero();
            characterCinemachineBrain.enabled = isActive;
        }

        private void ToggleExternalSystems(bool activateScreenshotCamera)
        {
            if (activateScreenshotCamera)
            {
                allUIHidden.Set(true);

                cameraModeInputLocked.Set(true);

                prevMouseButtonCursorLockMode = cameraLeftMouseButtonCursorLock.Get();
                cameraLeftMouseButtonCursorLock.Set(true);
            }
            else
            {
                allUIHidden.Set(false);
                cameraModeInputLocked.Set(false);
                cameraLeftMouseButtonCursorLock.Set(prevMouseButtonCursorLockMode);
            }

            cameraBlocked.Set(activateScreenshotCamera);
            featureKeyTriggersBlocked.Set(activateScreenshotCamera);
            userMovementKeysBlocked.Set(activateScreenshotCamera);
        }

        private async void UpdateStorageInfo() =>
            storageStatus = await cameraReelStorageService.GetUserGalleryStorageInfo(playerId);

        internal void InstantiateCameraObjects()
        {
            (screencaptureCameraHUDController, screencaptureCameraHUDView) = factory.CreateHUD(this, screencaptureCameraHUDViewPrefab);
            screenRecorderLazyValue = factory.CreateScreenRecorder(screencaptureCameraHUDView.RectTransform);
            screenshotCamera = factory.CreateScreencaptureCamera(cameraPrefab, characterCameraTransform, transform, characterController.gameObject.layer, cameraTarget, virtualCamera);
            playerName = factory.CreatePlayerNameUI(playerNamePrefab, MIN_PLAYERNAME_HEIGHT, Player, playerAvatar: characterController.GetComponent<PlayerAvatarController>());

            isInstantiated = true;
        }
    }
}
