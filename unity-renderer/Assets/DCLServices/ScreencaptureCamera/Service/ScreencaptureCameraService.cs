using Cysharp.Threading.Tasks;
using DCL.Providers;
using DCLFeatures.ScreencaptureCamera.CameraObject;
using System.Threading;

namespace DCLServices.ScreencaptureCamera.Service
{
    public class ScreencaptureCameraService : IScreencaptureCameraService
    {
        private readonly IAddressableResourceProvider resourceProvider;
        private ScreencaptureCameraBehaviour cameraBehaviour;

        public ScreencaptureCameraService(IAddressableResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        public void Initialize() { }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            cameraBehaviour = await resourceProvider.Instantiate<ScreencaptureCameraBehaviour>("ScreencaptureCameraController", cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
