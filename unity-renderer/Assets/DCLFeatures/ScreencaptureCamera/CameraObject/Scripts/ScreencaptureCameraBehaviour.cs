﻿using Cinemachine;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Camera;
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
        [SerializeField] private Camera mainCamera;
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

        internal ScreenRecorder screenRecorderValue;
        internal bool? isGuestLazyValue;
        internal BooleanVariable isScreencaptureCameraActive;

        internal Camera screenshotCamera;
        internal IAvatarsLODController avatarsLODControllerLazyValue;
        private float lastScreenshotTime = -Mathf.Infinity;

        private bool isInstantiated;
        private ScreencaptureCameraHUDView screencaptureCameraHUDView;
        private CancellationTokenSource uploadPictureCancellationToken;
        private ICameraReelStorageService cameraReelStorageServiceLazyValue;

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
        private Camera prevSkyboxCamera;
        private PlayerName playerName;
        private Transform characterCameraTransform => mainCamera.transform; //cameraController.GetCamera().transform;
        private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
        private Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
        private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
        private BaseVariable<Quaternion> cameraRotation => DataStore.i.camera.rotation;

        private bool isOnCooldown => Time.realtimeSinceStartup - lastScreenshotTime < SPLASH_FX_DURATION + IMAGE_TRANSITION_FX_DURATION + MIDDLE_PAUSE_FX_DURATION;

        private IAvatarsLODController avatarsLODController => avatarsLODControllerLazyValue ??= Environment.i.serviceLocator.Get<IAvatarsLODController>();

        private ICameraReelStorageService cameraReelStorageService => cameraReelStorageServiceLazyValue ??= Environment.i.serviceLocator.Get<ICameraReelStorageService>();

        private bool isGuest => isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

        private ICameraReelAnalyticsService analytics => Environment.i.serviceLocator.Get<ICameraReelAnalyticsService>();

        private DataStore_Player player => DataStore.i.player;

        private FeatureFlag featureFlags => DataStore.i.featureFlags.flags.Get();

        private ScreenRecorder screenRecorder
        {
            get
            {
                if (isInstantiated)
                    return screenRecorderValue;

                InstantiateCameraObjects();

                return screenRecorderValue;
            }
        }

        private void Awake()
        {
            storageStatus = new CameraReelStorageStatus(0, 0);
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
                enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(() => ToggleScreenshotCamera("Button"));
                CommonScriptableObjects.allUIHidden.OnChange += (isHidden, _) => enableCameraButtonCanvas.enabled = !isHidden;

                enabled = true;

                yield return new WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id));
                playerId = player.ownPlayer.Get().id;
                UpdateStorageInfo();
            }

            cameraTarget.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
            virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
        }

        private void Update()
        {
            if (!isScreencaptureCameraActive.Get()) return;

            cameraForward.Set(screenshotCamera.transform.forward);
            cameraRight.Set(screenshotCamera.transform.right);
            cameraRotation.Set(screenshotCamera.transform.rotation);
            cameraPosition.Set(screenshotCamera.transform.position);
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

            Texture2D screenshot = screenRecorder.CaptureScreenshot();

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

            UpdateStorageInfo();

            bool activateScreenshotCamera = !(isInstantiated && screenshotCamera.gameObject.activeSelf);

            Utils.UnlockCursor();

            ToggleExternalSystems(activateScreenshotCamera);
            ToggleCameraSystems(activateScreenshotCamera);

            isScreencaptureCameraActive.Set(activateScreenshotCamera);

            if (isEnabled && !string.IsNullOrEmpty(source))
                analytics.OpenCamera(source);
        }

        private async void UpdateStorageInfo()
        {
            storageStatus = await cameraReelStorageService.GetUserGalleryStorageInfo(playerId);
        }

        private void ToggleScreenshotCamera(DCLAction_Trigger _) =>
            ToggleScreenshotCamera("Shortcut", !isScreencaptureCameraActive.Get());

        private void ToggleCameraSystems(bool activateScreenshotCamera)
        {
            cameraController.SetCameraEnabledState(!activateScreenshotCamera);

            characterController.SetEnabled(!activateScreenshotCamera);

            cameraController.GetCamera().GetComponent<CinemachineBrain>().enabled = !activateScreenshotCamera;

            if (activateScreenshotCamera)
            {
                if (!isInstantiated)
                    InstantiateCameraObjects();

                CinemachineHardLockToTarget body = virtualCamera.GetCinemachineComponent<CinemachineHardLockToTarget>();
                body.m_Damping = 0;

                CinemachineSameAsFollowTarget composer = virtualCamera.GetCinemachineComponent<CinemachineSameAsFollowTarget>();
                composer.m_Damping = 0;

                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                virtualCamera.PreviousStateIsValid = false; // This forces the next camera update to snap
                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);

                cameraTarget.GetComponent<CharacterController>().enabled = false;
                cameraTarget.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                cameraTarget.GetComponent<CharacterController>().enabled = true;

                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                virtualCamera.Follow = cameraTarget.transform;
                virtualCamera.LookAt = cameraTarget.transform;
                virtualCamera.PreviousStateIsValid = false; // This forces the next camera update to snap
                screenshotCamera.GetComponent<CinemachineBrain>().ManualUpdate();
                body.m_Damping = 1;
                composer.m_Damping = 1;
                playerName.Show();
            }
            else
            {
                playerName.Hide();

                // screenshotCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                CinemachineHardLockToTarget body = virtualCamera.GetCinemachineComponent<CinemachineHardLockToTarget>();
                body.m_Damping = 0;

                CinemachineSameAsFollowTarget composer = virtualCamera.GetCinemachineComponent<CinemachineSameAsFollowTarget>();
                composer.m_Damping = 0;

                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                virtualCamera.PreviousStateIsValid = false; // This forces the next camera update to snap
                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);

                cameraTarget.GetComponent<CharacterController>().enabled = false;
                cameraTarget.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                cameraTarget.GetComponent<CharacterController>().enabled = true;

                virtualCamera.transform.SetPositionAndRotation(characterCameraTransform.position, characterCameraTransform.rotation);
                virtualCamera.Follow = cameraTarget.transform;
                virtualCamera.LookAt = cameraTarget.transform;
                virtualCamera.PreviousStateIsValid = false; // This forces the next camera update to snap

                body.m_Damping = 1;
                composer.m_Damping = 1;

                screenshotCamera.GetComponent<CinemachineBrain>().ManualUpdate();
            }

            screenshotCamera.gameObject.SetActive(activateScreenshotCamera);
            avatarsLODController.SetCamera(activateScreenshotCamera ? screenshotCamera : cameraController.GetCamera());
            screencaptureCameraHUDController.SetVisibility(activateScreenshotCamera, storageStatus.HasFreeSpace);

            if (activateScreenshotCamera)
            {
                prevSkyboxCamera = SkyboxController.i.SkyboxCamera.CurrentCamera;
                SkyboxController.i.AssignMainOverlayCamera(screenshotCamera.transform);
            }
            else { SkyboxController.i.AssignMainOverlayCamera(prevSkyboxCamera.transform); }
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

        internal void InstantiateCameraObjects()
        {
            CreateHUD();
            screenRecorderValue = new ScreenRecorder(screencaptureCameraHUDView.RectTransform);
            CreateScreencaptureCamera();
            CreatePlayerNameUI();

            isInstantiated = true;
        }

        private void CreatePlayerNameUI()
        {
            playerName = Instantiate(playerNamePrefab, characterController.transform);

            UserProfile userProfile = UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id);
            playerName.SetName(userProfile.userName, userProfile.hasClaimedName, userProfile.isGuest);

            PlayerAvatarController playerAvatar = characterController.GetComponent<PlayerAvatarController>();
            float height = playerAvatar.Avatar.extents.y - 0.85f;
            playerName.SetYOffset(Mathf.Max(MIN_PLAYERNAME_HEIGHT, height));
        }

        private void CreateHUD()
        {
            screencaptureCameraHUDView = Instantiate(screencaptureCameraHUDViewPrefab);
            screencaptureCameraHUDController = new ScreencaptureCameraHUDController(screencaptureCameraHUDView,
                screencaptureCameraBehaviour: this, inputActionsSchema, DataStore.i);
            screencaptureCameraHUDController.Initialize();
        }

        private void CreateScreencaptureCamera()
        {
            screenshotCamera = Instantiate(cameraPrefab, characterCameraTransform.position, characterCameraTransform.rotation, transform);
            screenshotCamera.gameObject.layer = characterController.gameObject.layer;
            screenshotCamera.GetComponent<ScreencaptureCameraMovement>().SetTarget(cameraTarget);
        }
    }
}