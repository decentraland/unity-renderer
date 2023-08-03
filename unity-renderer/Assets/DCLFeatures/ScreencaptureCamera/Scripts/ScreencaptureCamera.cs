using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.CameraReelService;
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

        [Space, SerializeField] internal ScreencaptureCameraInputSchema inputActionsSchema;

        internal ScreenshotCapture screenshotCaptureLazyValue;
        internal bool? isGuestLazyValue;
        internal BooleanVariable isScreenshotCameraActive;

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

        private bool isOnCooldown => Time.realtimeSinceStartup - lastScreenshotTime < COOLDOWN;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private IScreenshotCameraService cameraReelService => cameraReelServiceLazyValue ??= Environment.i.serviceLocator.Get<IScreenshotCameraService>();

        private bool isGuest => isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

        private DataStore_Player player => DataStore.i.player;

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        private ScreenshotCapture screenshotCapture
        {
            get
            {
                if (isInstantiated)
                    return screenshotCaptureLazyValue;

                InstantiateCameraObjects();

                return screenshotCaptureLazyValue;
            }
        }

        private void Awake()
        {
            DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
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
                enabled = true;
        }

        internal void OnEnable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered += ToggleScreenshotCamera;
        }

        internal void OnDisable()
        {
            inputActionsSchema.ToggleScreenshotCameraAction.OnTriggered -= ToggleScreenshotCamera;
        }

        private void OnExploreV2Open(bool current, bool previous)
        {
            if (current && cameraReelServiceLazyValue == null)
            {
                cameraReelService.SetCamera(this);
                DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
            }
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            SetVisibility(!isScreenshotCameraActive.Get());

        public void SetVisibility(bool isVisible = true)
        {
            if (isGuest) return;
            if (isVisible == isScreenshotCameraActive.Get()) return;

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            ToggleExternalSystems(activateScreenshotCamera);
            ToggleCameraSystems(activateScreenshotCamera);

            isScreenshotCameraActive.Set(activateScreenshotCamera);
        }

        public void CaptureScreenshot()
        {
            if (!isScreenshotCameraActive.Get() || isGuest || isOnCooldown) return;

            lastScreenshotTime = Time.realtimeSinceStartup;
            Texture2D screenshot = screenshotCapture.CaptureScreenshot();
            screencaptureCameraHUDController.PlayScreenshotFX(screenshot, COOLDOWN/2, COOLDOWN/2);

            uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
            cameraReelService.UploadScreenshot(screenshot, metadata: ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera), ct: uploadPictureCancellationToken.Token).Forget();
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

            this.isScreenshotCameraActive = isScreenshotCameraActive;
        }

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            screencaptureCameraHUDView.SwitchVisibility(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());
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
            screencaptureCameraHUDView = Instantiate(screencaptureCameraHUDViewPrefab);
            screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView, this, inputActionsSchema);
            screencaptureCameraHUDController.Initialize();

            characterCameraTransform = cameraController.GetCamera().transform;
            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            Image refBoundariesImage = screencaptureCameraHUDView.RefImage;
            screenshotCaptureLazyValue ??= new ScreenshotCapture(screenshotCamera, screencaptureCameraHUDView.RectTransform, refBoundariesImage.sprite, refBoundariesImage.rectTransform);

            isInstantiated = true;
        }
    }
}
