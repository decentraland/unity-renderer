using Cinemachine;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.CameraTool;
using DCL.Helpers;
using DCL.Skybox;
using DCL.Tasks;
using DCLFeatures.CameraReel.Section;
using DCLFeatures.ScreencaptureCamera.UI;
using DCLServices.CameraReelService;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
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

        private readonly WaitForEndOfFrame waitEndOfFrameYield = new ();

        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] internal Camera mainCamera;
        [SerializeField] internal DCLCharacterController characterController;
        [SerializeField] internal CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] internal Camera cameraPrefab;
        [SerializeField] internal ScreencaptureCameraHUDView screencaptureCameraHUDViewPrefab;
        [SerializeField] internal Canvas enableCameraButtonPrefab;
        [SerializeField] private PlayerName playerNamePrefab;

        [SerializeField] private CharacterController cameraTarget;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [Space] [SerializeField] internal ScreencaptureCameraInputSchema inputActionsSchema;

        internal Camera screenshotCamera;
        internal BooleanVariable isScreencaptureCameraActive;

        internal ScreenRecorder screenRecorderLazyValue;
        internal bool? isGuestLazyValue;
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
        private bool prevUiHiddenState;
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

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        private ICameraReelStorageService cameraReelStorageService => cameraReelStorageServiceLazyValue ??= Environment.i.serviceLocator.Get<ICameraReelStorageService>();
        private ICameraReelAnalyticsService analytics => Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>();

        private Transform characterCameraTransform => mainCamera.transform;
        private CinemachineBrain characterCinemachineBrain => characterCinemachineBrainLazyValue ??= mainCamera.GetComponent<CinemachineBrain>();
        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private DataStore_Player player => DataStore.i.player;
        private bool isGuest => isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

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

        private bool isOnCooldown => Time.realtimeSinceStartup - lastScreenshotTime < SPLASH_FX_DURATION + IMAGE_TRANSITION_FX_DURATION + MIDDLE_PAUSE_FX_DURATION;

        private void Awake()
        {
            storageStatus = new CameraReelStorageStatus(0, 0);
            SetExternalDependencies(CommonScriptableObjects.allUIHidden, CommonScriptableObjects.cameraModeInputLocked, DataStore.i.camera.leftMouseButtonCursorLock, CommonScriptableObjects.cameraBlocked, CommonScriptableObjects.featureKeyTriggersBlocked, CommonScriptableObjects.userMovementKeysBlocked, CommonScriptableObjects.isScreenshotCameraActive);
        }

        // TODO(Vitaly): Remove this logic when feature flag will be enabled
        private IEnumerator Start()
        {
            enabled = false;
            yield return new WaitUntil(() => featureFlags.IsInitialized);

            if (!featureFlags.IsFeatureEnabled("camera_reel"))
                Destroy(gameObject);
            else
            {
                yield return new WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id));

                if(isGuest)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Canvas enableCameraButtonCanvas = Instantiate(enableCameraButtonPrefab);
                    enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(() => ToggleScreenshotCamera("Button"));
                    CommonScriptableObjects.allUIHidden.OnChange += (isHidden, _) => enableCameraButtonCanvas.enabled = !isHidden;

                    enabled = true;

                    playerId = player.ownPlayer.Get().id;
                    UpdateStorageInfo();
                }
            }
        }

        internal void OnEnable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered += ToggleScreenshotCamera;
            inputActionsSchema.ToggleCameraReelAction.OnTriggered += OpenCameraReelGallery;
        }

        internal void OnDisable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered -= ToggleScreenshotCamera;
            inputActionsSchema.ToggleCameraReelAction.OnTriggered -= OpenCameraReelGallery;
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

        internal void SetExternalDependencies(BooleanVariable allUIHidden, BooleanVariable cameraModeInputLocked, BaseVariable<bool> cameraLeftMouseButtonCursorLock,
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
            StopAllCoroutines();
            StartCoroutine(CaptureScreenshotAtTheFrameEnd(source));
        }

        private IEnumerator CaptureScreenshotAtTheFrameEnd(string source)
        {
            if (!isScreencaptureCameraActive.Get() || isGuest || isOnCooldown || !storageStatus.HasFreeSpace) yield break;

            lastScreenshotTime = Time.realtimeSinceStartup;

            screencaptureCameraHUDController.SetVisibility(false, storageStatus.HasFreeSpace);
            yield return waitEndOfFrameYield;

            Texture2D screenshot = screenRecorderLazy.CaptureScreenshot();

            screencaptureCameraHUDController.SetVisibility(true, storageStatus.HasFreeSpace);
            screencaptureCameraHUDController.PlayScreenshotFX(screenshot, SPLASH_FX_DURATION, MIDDLE_PAUSE_FX_DURATION, IMAGE_TRANSITION_FX_DURATION);

            var metadata = ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera);
            uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
            UploadScreenshotAsync(screenshot, metadata, source, uploadPictureCancellationToken.Token).Forget();

            async UniTaskVoid UploadScreenshotAsync(Texture2D screenshot, ScreenshotMetadata metadata, string source, CancellationToken cancellationToken)
            {
                try
                {
                    (CameraReelResponse cameraReelResponse, CameraReelStorageStatus cameraReelStorageStatus) =
                        await cameraReelStorageService.UploadScreenshot(screenshot, metadata, cancellationToken);

                    storageStatus = cameraReelStorageStatus;
                    CameraReelModel.i.AddScreenshotAsFirst(cameraReelResponse);
                    CameraReelModel.i.SetStorageStatus(cameraReelStorageStatus.CurrentScreenshots, cameraReelStorageStatus.MaxScreenshots);

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
            if (isGuest) return;
            if (isEnabled == isScreencaptureCameraActive.Get()) return;

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
            cameraController.SetCameraMode(CameraMode.ModeId.ThirdPerson);
            await UniTask.WaitWhile(() => cameraController.CameraIsBlending);

            ToggleExternalSystems(activateScreenshotCamera: true);
            ToggleCameraSystems(activateScreenshotCamera: true);
            isScreencaptureCameraActive.Set(true);
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera("Shortcut", !isScreencaptureCameraActive.Get());

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

            SetActivePlayerCamera(isActive: !activateScreenshotCamera);
            SetActiveScreenshotCamera(isActive: activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : mainCamera);
        }

        private void SetActiveScreenshotCamera(bool isActive)
        {
            screenshotCamera.gameObject.SetActive(isActive);
            screencaptureCameraHUDController.SetVisibility(isActive, storageStatus.HasFreeSpace);
        }

        private void SetActivePlayerCamera(bool isActive)
        {
            cameraController.SetCameraEnabledState(isActive);
            characterController.SetEnabled(isActive);
            characterCinemachineBrain.enabled = isActive;
        }

        private void ToggleExternalSystems(bool activateScreenshotCamera)
        {
            if (activateScreenshotCamera)
            {
                prevUiHiddenState = allUIHidden.Get();
                allUIHidden.Set(true);

                prevMouseLockState = cameraModeInputLocked.Get();
                cameraModeInputLocked.Set(true);

                prevMouseButtonCursorLockMode = cameraLeftMouseButtonCursorLock.Get();
                cameraLeftMouseButtonCursorLock.Set(true);
            }
            else
            {
                allUIHidden.Set(prevUiHiddenState);
                cameraModeInputLocked.Set(prevMouseLockState);
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
            (screencaptureCameraHUDController, screencaptureCameraHUDView) = factory.CreateHUD(this, screencaptureCameraHUDViewPrefab, inputActionsSchema);

            screenRecorderLazyValue = new ScreenRecorder(screencaptureCameraHUDView.RectTransform);

            screenshotCamera = factory.CreateScreencaptureCamera(cameraPrefab, characterCameraTransform, transform, characterController.gameObject.layer, cameraTarget, virtualCamera);

            playerName = factory.CreatePlayerNameUI(
                playerNamePrefab,
                MIN_PLAYERNAME_HEIGHT,
                player,
                playerAvatar: characterController.GetComponent<PlayerAvatarController>()
            );

            isInstantiated = true;
        }
    }
}
