using DCL;
using DCL.Camera;
using DCL.Helpers;
using DCLServices.CameraReelService;
using MainScripts.DCL.InWorldCamera.Scripts;
using UnityEngine;

namespace UI.InWorldCamera.Scripts
{
    public class ScreenshotCamera : MonoBehaviour
    {
        [Header("EXTERNAL DEPENDENCIES")]
        [SerializeField] private DCLCharacterController characterController;
        [SerializeField] private CameraController cameraController;

        [Header("MAIN COMPONENTS")]
        [SerializeField] private Camera cameraPrefab;
        [SerializeField] private ScreenshotHUDView screenshotHUDViewPrefab;

        [Header("INPUT ACTIONS")]
        [SerializeField] private InputAction_Trigger cameraInputAction;
        [SerializeField] private InputAction_Trigger takeScreenshotAction;

        private bool isInstantiated;
        private bool isInScreenshotMode;

        private Camera screenshotCamera;
        private ScreenshotHUDView screenshotHUDView;

        private Transform characterCameraTransform;
        private IAvatarsLODController avatarsLODControllerLazyValue;

        private ScreenshotCapture screenshotCaptureLazyValue;
        private ICameraReelNetworkService cameraReelNetworkServiceLazyValue;

        private bool prevUiHiddenState;
        private bool prevMouseLockState;
        private bool prevMouseButtonCursorLockMode;
        private bool? isGuestLazyValue;

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

        private void Awake()
        {
            if (!featureFlags.IsFeatureEnabled("camera_reel") || isGuest)
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            cameraInputAction.OnTriggered += ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered += CaptureScreenshot;
        }

        private void OnDisable()
        {
            cameraInputAction.OnTriggered -= ToggleScreenshotCamera;
            takeScreenshotAction.OnTriggered -= CaptureScreenshot;
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _)
        {
            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            if (activateScreenshotCamera)
            {
                prevUiHiddenState = CommonScriptableObjects.allUIHidden.Get();
                CommonScriptableObjects.allUIHidden.Set(true);

                prevMouseLockState = CommonScriptableObjects.cameraModeInputLocked.Get();
                CommonScriptableObjects.cameraModeInputLocked.Set(false);

                prevMouseButtonCursorLockMode = DataStore.i.camera.leftMouseButtonCursorLock.Get();
                DataStore.i.camera.leftMouseButtonCursorLock.Set(true);
            }
            else
            {
                CommonScriptableObjects.allUIHidden.Set(prevUiHiddenState);
                CommonScriptableObjects.cameraModeInputLocked.Set(prevMouseLockState);
                DataStore.i.camera.leftMouseButtonCursorLock.Set(prevMouseButtonCursorLockMode);
            }

            CommonScriptableObjects.cameraBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(activateScreenshotCamera);
            CommonScriptableObjects.userMovementKeysBlocked.Set(activateScreenshotCamera);

            cameraController.SetCameraEnabledState(!activateScreenshotCamera);
            characterController.SetEnabled(!activateScreenshotCamera);

            if (activateScreenshotCamera)
                EnableScreenshotCamera();

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            screenshotHUDView.SwitchVisibility(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());

            CommonScriptableObjects.isScreenshotCameraActive.Set(activateScreenshotCamera);
            isInScreenshotMode = activateScreenshotCamera;
        }

        private async void CaptureScreenshot(DCLAction_Trigger _)
        {
            if (!isInScreenshotMode) return;

            CameraReelImageResponse response = await cameraReelNetworkService.UploadScreenshot
            (
                screenshot: screenshotCapture.CaptureScreenshot(),
                metadata: ScreenshotMetadata.Create(player, avatarsLODController, screenshotCamera)
            );

            // TODO(Vitaly): Remove this temporal solution when we get a proper UI for the camera reel
            Application.OpenURL($"https://reels.decentraland.org/{response.id}");
        }

        private void EnableScreenshotCamera()
        {
            if (!isInstantiated)
                InstantiateCameraObjects();
            else
                screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void InstantiateCameraObjects()
        {
            characterCameraTransform = cameraController.GetCamera().transform;

            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation);
            screenshotHUDView = Instantiate(screenshotHUDViewPrefab);

            screenshotCamera.gameObject.layer = characterController.gameObject.layer;

            screenshotCaptureLazyValue = new ScreenshotCapture(screenshotCamera, screenshotHUDView.RectTransform, screenshotHUDView.RefImage);

            isInstantiated = true;
        }
    }
}
