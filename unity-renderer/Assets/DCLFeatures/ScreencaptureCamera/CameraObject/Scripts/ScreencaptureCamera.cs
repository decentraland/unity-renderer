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
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreencaptureCamera : MonoBehaviour, IScreencaptureCamera
    {
        private const float COOLDOWN = 3f;

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
        private IScreenshotCameraService cameraReelServiceLazyValue;

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


        private bool isOnCooldown => Time.realtimeSinceStartup - lastScreenshotTime < COOLDOWN;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private IScreenshotCameraService cameraReelService => cameraReelServiceLazyValue ??= Environment.i.serviceLocator.Get<IScreenshotCameraService>();

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

        public event Action<bool> StateSwitched;

        private void Awake()
        {
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
                var enableCameraButtonCanvas = Instantiate(enableCameraButtonPrefab);
                enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(() => ToggleScreenshotCamera());
                CommonScriptableObjects.allUIHidden.OnChange +=  (isHidden, _) => enableCameraButtonCanvas.enabled = !isHidden;

                enabled = true;
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
            this.isScreencaptureCameraActive = isScreenshotCameraActive;
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
            if (!isScreenshotCameraActive.Get() || isGuest || isOnCooldown) return;

            lastScreenshotTime = Time.realtimeSinceStartup;
            Texture2D screenshot = screenshotCapture.CaptureScreenshot();
            screencaptureCameraHUDController.PlayScreenshotFX(screenshot, COOLDOWN/2, COOLDOWN/2);

            uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
            UploadScreenshotAsync(uploadPictureCancellationToken.Token).Forget();

            async UniTaskVoid UploadScreenshotAsync(CancellationToken cancellationToken)
            {
                (CameraReelResponse cameraReelResponse, CameraReelStorageStatus cameraReelStorageStatus) = await cameraReelService.UploadScreenshot(screenshot,
                    metadata: ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera),
                    ct: cancellationToken);

                CameraReelModel.i.AddScreenshotAsFirst(cameraReelResponse);
                CameraReelModel.i.SetStorageStatus(cameraReelStorageStatus.CurrentScreenshots, cameraReelStorageStatus.MaxScreenshots);
            }
        }

        public void ToggleScreenshotCamera(bool isEnabled = true)
        {
            if (isGuest) return;
            if (isEnabled == isScreencaptureCameraActive.Get()) return;

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            ToggleExternalSystems(activateScreenshotCamera);
            ToggleCameraSystems(activateScreenshotCamera);

            isScreencaptureCameraActive.Set(activateScreenshotCamera);
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera(!isScreencaptureCameraActive.Get());

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());

            StateSwitched?.Invoke(activateScreenshotCamera);
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
            screenRecorderLazyValue ??= new ScreenRecorder(screenshotCamera, screencaptureCameraHUDView.RectTransform, refBoundariesImage.sprite, refBoundariesImage.rectTransform);
    }
}
