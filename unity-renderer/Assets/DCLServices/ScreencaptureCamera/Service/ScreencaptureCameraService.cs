using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using System.Threading;

namespace DCLServices.ScreencaptureCamera.Service
{
    public class ScreencaptureCameraService : IScreencaptureCameraService
    {
        private const string CONTROLLER_PATH = "ScreencaptureCameraController";
        private readonly IAddressableResourceProvider resourceProvider;
        private readonly BaseVariable<FeatureFlag> featureFlags;
        private readonly DataStore_Player player;

        private ScreencaptureCameraBehaviour cameraBehaviour;

        private bool featureIsEnabled => featureFlags.Get().IsFeatureEnabled("camera_reel");
        private bool isGuest => false;// isGuestLazyValue ??= UserProfileController.userProfilesCatalog.Get(player.ownPlayer.Get().id).isGuest;

        public ScreencaptureCameraService(IAddressableResourceProvider resourceProvider, BaseVariable<FeatureFlag> featureFlags, DataStore_Player player)
        {
            this.resourceProvider = resourceProvider;
            this.featureFlags = featureFlags;
            this.player = player;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => featureFlags.Get().IsInitialized, cancellationToken: cancellationToken);
            if (!featureIsEnabled) return;

            await UniTask.WaitUntil(() => player.ownPlayer.Get() != null && !string.IsNullOrEmpty(player.ownPlayer.Get().id), cancellationToken: cancellationToken);
            if (isGuest) return;

            cameraBehaviour = await resourceProvider.Instantiate<ScreencaptureCameraBehaviour>(CONTROLLER_PATH, cancellationToken: cancellationToken);
        }
    }
}
