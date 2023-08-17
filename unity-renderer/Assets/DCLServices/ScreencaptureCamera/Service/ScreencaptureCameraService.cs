using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace DCLServices.ScreencaptureCamera.Service
{
    public class ScreencaptureCameraService : IScreencaptureCameraService
    {
        private const string CONTROLLER_PATH = "ScreencaptureCameraController";
        private const string MAIN_BUTTON_PATH = "ScreencaptureMainButton";

        private readonly IAddressableResourceProvider resourceProvider;
        private readonly BaseVariable<FeatureFlag> featureFlags;
        private readonly DataStore_Player player;

        private ScreencaptureCameraBehaviour cameraBehaviour;
        private Canvas enableCameraButtonCanvas;

        private bool featureIsEnabled => featureFlags.Get().IsFeatureEnabled("camera_reel");
        private bool isGuest => false;// isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

        public ScreencaptureCameraService(IAddressableResourceProvider resourceProvider, BaseVariable<FeatureFlag> featureFlags, DataStore_Player player)
        {
            this.resourceProvider = resourceProvider;
            this.featureFlags = featureFlags;
            this.player = player;
        }

        public void Initialize() { }

        public void Dispose()
        {
            CommonScriptableObjects.allUIHidden.OnChange -= ToggleMainButtonVisibility;

            Object.Destroy(cameraBehaviour);
            Object.Destroy(enableCameraButtonCanvas.gameObject);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => featureFlags.Get().IsInitialized, cancellationToken: cancellationToken);
            if (!featureIsEnabled) return;

            await UniTask.WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id), cancellationToken: cancellationToken);
            if (isGuest) return;

            cameraBehaviour = await resourceProvider.Instantiate<ScreencaptureCameraBehaviour>(CONTROLLER_PATH, cancellationToken: cancellationToken);
            cameraBehaviour.Player = player;

            enableCameraButtonCanvas = await resourceProvider.Instantiate<Canvas>(MAIN_BUTTON_PATH, cancellationToken: cancellationToken);
            enableCameraButtonCanvas.GetComponentInChildren<Button>().onClick.AddListener(EnableScreenshotCamera);
            CommonScriptableObjects.allUIHidden.OnChange += ToggleMainButtonVisibility;
        }

        private void EnableScreenshotCamera() => cameraBehaviour.ToggleScreenshotCamera("Button");
        private void ToggleMainButtonVisibility(bool isHidden, bool _) => enableCameraButtonCanvas.enabled = !isHidden;
    }
}
