using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.Helpers;
using DCL.Tasks;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreencaptureCamera : MonoBehaviour, IScreencaptureCamera
    {
        private const string UPLOADING_ERROR_MESSAGE = "There was an unexpected error when uploading the picture. Try again later.";
        private const string STORAGE_LIMIT_REACHED_MESSAGE = "You can't take more pictures because you have reached the storage limit of the camera reel.\nTo make room we recommend you to download your photos and then delete them.";
        private const float SPLASH_FX_DURATION = 1f;
        private const float MIDDLE_PAUSE_FX_DURATION = 0.1f;
        private const float IMAGE_TRANSITION_FX_DURATION = 0.5f;
        private readonly WaitForEndOfFrame waitEndOfFrameYield = new ();

        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] internal DCLCharacterController characterController;
        [SerializeField] internal CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] internal Camera cameraPrefab;
        [SerializeField] internal ScreencaptureCameraHUDView screencaptureCameraHUDViewPrefab;
        [SerializeField] internal Canvas enableCameraButtonPrefab;

        [Space] [SerializeField] internal ScreencaptureCameraInputSchema inputActionsSchema;

        internal ScreenRecorder screenRecorderLazyValue;
        internal bool? isGuestLazyValue;
        internal BooleanVariable isScreencaptureCameraActive;

        internal Camera screenshotCamera;
        internal IAvatarsLODController avatarsLODControllerLazyValue;
        private float lastScreenshotTime = -Mathf.Infinity;

        private bool isInstantiated;
        private ScreencaptureCameraHUDView screencaptureCameraHUDView;
        private CancellationTokenSource uploadPictureCancellationToken;
        private Transform characterCameraTransform;
        private ICameraReelService cameraReelServiceLazyValue;

        private bool prevUiHiddenState;
        private bool prevMouseLockState;
        private bool prevMouseButtonCursorLockMode;

        private BooleanVariable allUIHidden;
        private BooleanVariable cameraModeInputLocked;
        private BaseVariable<bool> cameraLeftMouseButtonCursorLock;
        private BooleanVariable cameraBlocked;
        private BooleanVariable featureKeyTriggersBlocked;
        private BooleanVariable userMovementKeysBlocked;
        private ScreencaptureCameraHUDController screencaptureCameraHUDController;
        private Canvas enableCameraButton;

        private string playerId;
        private CameraReelStorageStatus storageStatus;

        private bool isOnCooldown => Time.realtimeSinceStartup - lastScreenshotTime < SPLASH_FX_DURATION + IMAGE_TRANSITION_FX_DURATION + MIDDLE_PAUSE_FX_DURATION;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private ICameraReelService cameraReelService => cameraReelServiceLazyValue ??= Environment.i.serviceLocator.Get<ICameraReelService>();

        private bool isGuest => isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

        private DataStore_Player player => DataStore.i.player;

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        private ScreenRecorder screenRecorder
        {
            get
            {
                if (isInstantiated)
                    return screenRecorderLazyValue;

                InstantiateCameraObjects();

                return screenRecorderLazyValue;
            }
        }

        private void Awake()
        {
            storageStatus = new CameraReelStorageStatus(0, 0);

            DataStore.i.exploreV2.isOpen.OnChange += SelfRegisterToCameraReelService;
            SetExternalDependencies(CommonScriptableObjects.allUIHidden, CommonScriptableObjects.cameraModeInputLocked, DataStore.i.camera.leftMouseButtonCursorLock, CommonScriptableObjects.cameraBlocked, CommonScriptableObjects.featureKeyTriggersBlocked, CommonScriptableObjects.userMovementKeysBlocked, CommonScriptableObjects.isScreenshotCameraActive);
        }

        // TODO(Vitaly): Remove this logic when feature flag will be enalbed
        private IEnumerator Start()
        {
            enabled = false;
            yield return new WaitUntil(() => featureFlags.IsInitialized);

            if (!featureFlags.IsFeatureEnabled("camera_reel"))
                Destroy(gameObject);
            else
            {
                Canvas enableCameraButtonCanvas = Instantiate(enableCameraButtonPrefab);
                enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(() => ToggleScreenshotCamera());
                CommonScriptableObjects.allUIHidden.OnChange += (isHidden, _) => enableCameraButtonCanvas.enabled = !isHidden;

                enabled = true;

                yield return new WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id));
                playerId = player.ownPlayer.Get().id;
                UpdateStorageInfo();
            }
        }

        internal void OnEnable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered += ToggleScreenshotCamera;
        }

        internal void OnDisable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered -= ToggleScreenshotCamera;
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

        private void SelfRegisterToCameraReelService(bool current, bool _)
        {
            if (current && cameraReelServiceLazyValue == null)
            {
                cameraReelService.SetCamera(this);
                DataStore.i.exploreV2.isOpen.OnChange -= SelfRegisterToCameraReelService;
            }
        }

        public void CaptureScreenshot()
        {
            StopAllCoroutines();
            StartCoroutine(CaptureScreenshotAtTheFrameEnd());
        }

        private IEnumerator CaptureScreenshotAtTheFrameEnd()
        {
            if (!isScreencaptureCameraActive.Get() || isGuest || isOnCooldown || !storageStatus.HasFreeSpace) yield break;

            lastScreenshotTime = Time.realtimeSinceStartup;

            screencaptureCameraHUDController.SetVisibility(false, storageStatus.HasFreeSpace);
            yield return waitEndOfFrameYield;

            screenRecorder.CalculateTargetScreenResolution(out var targetScreenWidth, out var targetScreenHeight);

            var oldResolution = Screen.currentResolution;
            var mode = Screen.fullScreenMode;
            Debug.Log($"Old resolution {oldResolution.width}, {oldResolution.height}");
            Screen.SetResolution(Mathf.RoundToInt(targetScreenWidth), Mathf.RoundToInt(targetScreenHeight), mode);

            yield return new WaitUntil(() => Screen.width == Mathf.RoundToInt(targetScreenWidth)
                                             && Screen.height == Mathf.RoundToInt(targetScreenHeight));
            yield return waitEndOfFrameYield;
            screenshotCamera.Render();

            Debug.Log($"Current resolution {Screen.currentResolution.width}, {Screen.currentResolution.height}");

            var screenshot = screenRecorder.CaptureScreenshot(Mathf.RoundToInt(targetScreenWidth), Mathf.RoundToInt(targetScreenHeight));
            Screen.SetResolution(oldResolution.width, oldResolution.height, mode);

            screencaptureCameraHUDController.SetVisibility(true, storageStatus.HasFreeSpace);
            screencaptureCameraHUDController.PlayScreenshotFX(screenshot, SPLASH_FX_DURATION, MIDDLE_PAUSE_FX_DURATION, IMAGE_TRANSITION_FX_DURATION);

            var metadata = ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera);
            uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
            UploadScreenshotAsync(screenshot, metadata, uploadPictureCancellationToken.Token).Forget();

            async UniTaskVoid UploadScreenshotAsync(Texture2D screenshot, ScreenshotMetadata metadata, CancellationToken cancellationToken)
            {
                try
                {
                    (CameraReelResponse cameraReelResponse, CameraReelStorageStatus cameraReelStorageStatus) =
                        await cameraReelService.UploadScreenshot(screenshot, metadata, cancellationToken);

                    storageStatus = cameraReelStorageStatus;
                    CameraReelModel.i.AddScreenshotAsFirst(cameraReelResponse);
                    CameraReelModel.i.SetStorageStatus(cameraReelStorageStatus.CurrentScreenshots, cameraReelStorageStatus.MaxScreenshots);
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

        public void ToggleScreenshotCamera(bool isEnabled = true)
        {
            if (isGuest) return;
            if (isEnabled == isScreencaptureCameraActive.Get()) return;

            UpdateStorageInfo();

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            ToggleExternalSystems(activateScreenshotCamera);
            ToggleCameraSystems(activateScreenshotCamera);

            isScreencaptureCameraActive.Set(activateScreenshotCamera);
        }

        private async void UpdateStorageInfo()
        {
            storageStatus = await cameraReelService.GetUserGalleryStorageInfo(playerId);
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera(!isScreencaptureCameraActive.Get());

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            playerId = player.ownPlayer.Get().id;
            UpdateStorageInfo();

            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());
            screencaptureCameraHUDController.SetVisibility(activateScreenshotCamera, storageStatus.HasFreeSpace);
        }

        private void ToggleExternalSystems(bool activateScreenshotCamera)
        {
            if (activateScreenshotCamera)
            {
                prevUiHiddenState = allUIHidden.Get();
                allUIHidden.Set(true);

                prevMouseLockState = cameraModeInputLocked.Get();
                cameraModeInputLocked.Set(false);

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

        private void EnableScreenshotCamera()
        {
            if (!isInstantiated)
                InstantiateCameraObjects();
            else
                screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        internal void InstantiateCameraObjects()
        {
            CreateHUD();
            CreateScreencaptureCamera(cameraController.GetCamera().transform);
            CreateScreenshotRecorder(screencaptureCameraHUDView.RefImage);

            isInstantiated = true;
        }

        private void CreateHUD()
        {
            screencaptureCameraHUDView = Instantiate(screencaptureCameraHUDViewPrefab);
            screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView, screencaptureCamera: this, inputActionsSchema);
            screencaptureCameraHUDController.Initialize();
        }

        private void CreateScreencaptureCamera(Transform playerCameraTransform)
        {
            characterCameraTransform = playerCameraTransform;
            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;
        }

        private void CreateScreenshotRecorder(Image refBoundariesImage) =>
            screenRecorderLazyValue ??= new ScreenRecorder(screencaptureCameraHUDView.RectTransform, refBoundariesImage.sprite, refBoundariesImage.rectTransform);
    }
}
