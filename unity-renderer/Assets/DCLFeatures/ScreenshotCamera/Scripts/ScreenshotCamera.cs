using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace DCLFeatures.ScreenshotCamera
{
    public class ScreenshotCamera : MonoBehaviour, IScreenshotCamera
    {
        private const string UPLOADING_ERROR_MESSAGE = "There was an unexpected error when uploading the picture. Try again later.";
        private const float COOLDOWN = 3f;

        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] internal DCLCharacterController characterController;
        [SerializeField] internal CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] internal Camera cameraPrefab;
        [FormerlySerializedAs("screenshotHUDViewPrefab")] [SerializeField] internal ScreenshotCameraHUDView screenshotCameraHUDViewPrefab;

        [Header("INPUT ACTIONS")]
        [SerializeField] internal InputAction_Trigger cameraInputAction;
        [SerializeField] internal InputAction_Trigger takeScreenshotAction;

        internal ScreenshotCapture screenshotCaptureLazyValue;
        internal bool? isGuestLazyValue;
        internal BooleanVariable isScreenshotCameraActive;

        internal Camera screenshotCamera;
        internal IAvatarsLODController avatarsLODControllerLazyValue;
        private float lastScreenshotTime = -Mathf.Infinity;

        private bool isInstantiated;
        private ScreenshotCameraHUDView screenshotCameraHUDView;
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
        private ScreenshotCameraHUDController screenshotCameraHUDController;

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
            cameraInputAction.OnTriggered += ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered += CaptureScreenshot;
        }

        internal void OnDisable()
        {
            cameraInputAction.OnTriggered -= ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered -= CaptureScreenshot;
        }

        private void OnExploreV2Open(bool current, bool previous)
        {
            if (current && cameraReelServiceLazyValue == null)
            {
                cameraReelService.SetCamera(this);
                DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
            }
        }

        public void ToggleVisibility(bool _ = true)
        {
            if (isGuest) return;

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
            var metadata = ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera);

            ScreenshotFX(screenshot);

            uploadPictureCancellationToken = uploadPictureCancellationToken.SafeRestart();
            UploadScreenshotAsync(screenshot, metadata, uploadPictureCancellationToken.Token).Forget();

            async UniTaskVoid UploadScreenshotAsync(Texture2D image, ScreenshotMetadata data, CancellationToken cancellationToken)
            {
                try { await cameraReelService.UploadScreenshot(image, data, ct: cancellationToken); }
                catch (OperationCanceledException) { }
                catch (ScreenshotLimitReachedException)
                {
                    DataStore.i.notifications.DefaultErrorNotification.Set(
                        "You can't take more pictures because you have reached the storage limit of the camera reel.\nTo make room we recommend you to download your photos and then delete them.",
                        true);
                }
                catch (Exception) { DataStore.i.notifications.DefaultErrorNotification.Set(UPLOADING_ERROR_MESSAGE, true); }
            }
        }

        private void CaptureScreenshot(DCLAction_Trigger _) =>
            CaptureScreenshot();

        private void ScreenshotFX(Texture2D image)
        {
            AudioScriptableObjects.takeScreenshot.Play();
            screenshotCameraHUDView.ScreenshotCaptureAnimation(image, splashDuration: COOLDOWN / 2, transitionDuration: COOLDOWN / 2);
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleVisibility();

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
            screenshotCameraHUDView.SwitchVisibility(activateScreenshotCamera);
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
            screenshotCameraHUDView = Instantiate(screenshotCameraHUDViewPrefab);
            screenshotCameraHUDController = new ScreenshotCameraHUDController(screenshotCameraHUDView, this);
            screenshotCameraHUDController.Initialize();

            characterCameraTransform = cameraController.GetCamera().transform;
            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            Image refBoundariesImage = screenshotCameraHUDView.RefImage;
            screenshotCaptureLazyValue ??= new ScreenshotCapture(screenshotCamera, screenshotCameraHUDView.RectTransform, refBoundariesImage.sprite, refBoundariesImage.rectTransform);

            isInstantiated = true;
        }

        public void DisableScreenshotCameraMode()
        {
            if (isScreenshotCameraActive.Get())
                ToggleVisibility();
        }
    }
}
