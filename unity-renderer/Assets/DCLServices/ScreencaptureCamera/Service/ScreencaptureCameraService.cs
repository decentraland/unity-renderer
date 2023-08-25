using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DCLServices.ScreencaptureCamera.Service
{
    public class ScreencaptureCameraService : IScreencaptureCameraService
    {
        private const string CONTROLLER_PATH = "ScreencaptureCameraController";
        private const string MAIN_BUTTON_PATH = "ScreencaptureMainButton";

        private readonly IAddressableResourceProvider resourceProvider;
        private readonly BaseVariable<FeatureFlag> featureFlags;
        private readonly DataStore_Player player;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ScreencaptureCameraExternalDependencies externalDependencies;

        private ScreencaptureCameraBehaviour cameraBehaviour;
        private Canvas enableCameraButtonCanvas;

        private bool featureIsEnabled => featureFlags.Get().IsFeatureEnabled("camera_reel");
        private bool isGuest => userProfileBridge.GetOwn().isGuest;

        public ScreencaptureCameraService(IAddressableResourceProvider resourceProvider, BaseVariable<FeatureFlag> featureFlags, DataStore_Player player, IUserProfileBridge userProfileBridge, ScreencaptureCameraExternalDependencies externalDependencies)
        {
            this.resourceProvider = resourceProvider;
            this.featureFlags = featureFlags;
            this.player = player;
            this.userProfileBridge = userProfileBridge;
            this.externalDependencies = externalDependencies;
        }

        public void Initialize() { }

        public void Dispose()
        {
            externalDependencies.AllUIHidden.OnChange -= ToggleMainButtonVisibility;

            if(cameraBehaviour != null)
                Object.Destroy(cameraBehaviour);

            if(enableCameraButtonCanvas != null)
                Object.Destroy(enableCameraButtonCanvas.gameObject);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => featureFlags.Get().IsInitialized, cancellationToken: cancellationToken);
            if (!featureIsEnabled) return;

            await UniTask.WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id), cancellationToken: cancellationToken);
            if (isGuest) return;

            await InitializeCameraBehaviour(cancellationToken);
            await InitializeMainHUDButton(cancellationToken);
        }

        private async Task InitializeCameraBehaviour(CancellationToken cancellationToken)
        {
            cameraBehaviour = await resourceProvider.Instantiate<ScreencaptureCameraBehaviour>(CONTROLLER_PATH, cancellationToken: cancellationToken);

            cameraBehaviour.Player = player;

            cameraBehaviour.SetExternalDependencies(externalDependencies.AllUIHidden,
                externalDependencies.CameraModeInputLocked, externalDependencies.CameraLeftMouseButtonCursorLock, externalDependencies.CameraBlocked,
                externalDependencies.FeatureKeyTriggersBlocked, externalDependencies.UserMovementKeysBlocked, externalDependencies.IsScreenshotCameraActive);
        }

        private void EnableScreencaptureCamera(DCLAction_Trigger action)
        {
            EnableScreenshotCameraFromButton();
        }

        private async Task InitializeMainHUDButton(CancellationToken cancellationToken)
        {
            enableCameraButtonCanvas = await resourceProvider.Instantiate<Canvas>(MAIN_BUTTON_PATH, cancellationToken: cancellationToken);

            enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(EnableScreenshotCameraFromButton);
            externalDependencies.AllUIHidden.OnChange += ToggleMainButtonVisibility;
        }

        public void EnableScreencaptureCamera(string source) =>
            cameraBehaviour.ToggleScreenshotCamera(source);

        private void EnableScreenshotCameraFromButton() =>
            cameraBehaviour.ToggleScreenshotCamera("Button");

        private void ToggleMainButtonVisibility(bool isHidden, bool _) =>
            enableCameraButtonCanvas.enabled = !isHidden;
    }

    public struct ScreencaptureCameraExternalDependencies
    {
        public readonly BooleanVariable AllUIHidden;
        public readonly BooleanVariable CameraModeInputLocked;
        public readonly BaseVariable<bool> CameraLeftMouseButtonCursorLock;
        public readonly BooleanVariable CameraBlocked;
        public readonly BooleanVariable FeatureKeyTriggersBlocked;
        public readonly BooleanVariable UserMovementKeysBlocked;
        public readonly BooleanVariable IsScreenshotCameraActive;

        public ScreencaptureCameraExternalDependencies(BooleanVariable allUIHidden, BooleanVariable cameraModeInputLocked, BaseVariable<bool> cameraLeftMouseButtonCursorLock, BooleanVariable cameraBlocked, BooleanVariable featureKeyTriggersBlocked,
            BooleanVariable userMovementKeysBlocked, BooleanVariable isScreenshotCameraActive)
        {
            AllUIHidden = allUIHidden;
            CameraModeInputLocked = cameraModeInputLocked;
            CameraLeftMouseButtonCursorLock = cameraLeftMouseButtonCursorLock;
            CameraBlocked = cameraBlocked;
            FeatureKeyTriggersBlocked = featureKeyTriggersBlocked;
            UserMovementKeysBlocked = userMovementKeysBlocked;
            IsScreenshotCameraActive = isScreenshotCameraActive;
        }
    }
}
