using DCL;
using DCL.Camera;
using DCL.Helpers;
using DCLServices.CameraReelService;
using System.Collections;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ScreenshotCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        private const DCLAction_Trigger DUMMY_DCL_ACTION_TRIGGER = new ();

        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] internal DCLCharacterController characterController;
        [SerializeField] internal CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] internal Camera cameraPrefab;
        [SerializeField] internal ScreenshotHUDView screenshotHUDViewPrefab;

        [Header("INPUT ACTIONS")]
        [SerializeField] internal InputAction_Trigger cameraInputAction;
        [SerializeField] internal InputAction_Trigger takeScreenshotAction;

        internal ScreenshotCapture screenshotCaptureLazyValue;
        internal bool? isGuestLazyValue;
        internal BooleanVariable isScreenshotCameraActive;

        internal Camera screenshotCamera;
        internal IAvatarsLODController avatarsLODControllerLazyValue;

        private bool isInstantiated;
        private ScreenshotHUDView screenshotHUDView;

        private Transform characterCameraTransform;
        private ICameraReelNetworkService cameraReelNetworkServiceLazyValue;

        private bool prevUiHiddenState;
        private bool prevMouseLockState;
        private bool prevMouseButtonCursorLockMode;

        private BooleanVariable allUIHidden;
        private BooleanVariable cameraModeInputLocked;
        private BaseVariable<bool> cameraLeftMouseButtonCursorLock;
        private BooleanVariable cameraBlocked;
        private BooleanVariable featureKeyTriggersBlocked;
        private BooleanVariable userMovementKeysBlocked;

        private bool externalDependenciesSet;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();
        private ICameraReelNetworkService cameraReelNetworkService => cameraReelNetworkServiceLazyValue ??= Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
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

        internal void ToggleScreenshotCamera(DCLAction_Trigger _)
        {
            if (isGuest) return;

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            if (!externalDependenciesSet)
                SetExternalDependencies(CommonScriptableObjects.allUIHidden, CommonScriptableObjects.cameraModeInputLocked, DataStore.i.camera.leftMouseButtonCursorLock, CommonScriptableObjects.cameraBlocked, CommonScriptableObjects.featureKeyTriggersBlocked, CommonScriptableObjects.userMovementKeysBlocked, CommonScriptableObjects.isScreenshotCameraActive);

            ToggleExternalSystems(activateScreenshotCamera);
            ToggleCameraSystems(activateScreenshotCamera);

            isScreenshotCameraActive.Set(activateScreenshotCamera);
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

            externalDependenciesSet = true;
        }

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            screenshotHUDView.SwitchVisibility(activateScreenshotCamera);
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

        private async void CaptureScreenshot(DCLAction_Trigger _)
        {
            if (!isScreenshotCameraActive.Get() || isGuest) return;

            CameraReelResponse response = await cameraReelNetworkService.UploadScreenshot
            (
                image: screenshotCapture.CaptureScreenshot(),
                metadata: ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera)
            );

            // TODO(Vitaly): Remove this temporal solution when we get a proper UI for the camera reel
            { // temporal debug part
                Application.OpenURL($"https://reels.decentraland.org/{response.id}");
                Application.OpenURL(response.url);
                Application.OpenURL(response.thumbnailUrl);
            }
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
            characterCameraTransform = cameraController.GetCamera().transform;

            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);

            screenshotHUDView = Instantiate(screenshotHUDViewPrefab);

            screenshotHUDView.CloseButton?.onClick.AddListener(DisableScreenshotCameraMode);
            screenshotHUDView.TakeScreenshotButton?.onClick.AddListener(() => CaptureScreenshot(DUMMY_DCL_ACTION_TRIGGER));

            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            Image refBoundariesImage = screenshotHUDView.RefImage;
            screenshotCaptureLazyValue ??= new ScreenshotCapture(screenshotCamera, screenshotHUDView.RectTransform, refBoundariesImage.sprite, refBoundariesImage.rectTransform);

            isInstantiated = true;
        }

        private void DisableScreenshotCameraMode()
        {
            if (isScreenshotCameraActive.Get())
                ToggleScreenshotCamera(DUMMY_DCL_ACTION_TRIGGER);
        }
    }
}
