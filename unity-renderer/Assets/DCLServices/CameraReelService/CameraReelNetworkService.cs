using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkService : IService
    {
        UniTask GetScreenshot(string placeUUID, CancellationToken ct, bool renewCache = false);

        UniTask PutScreenshot(string placeUUID, CancellationToken ct, bool renewCache = false);
    }

    public class CameraReelNetworkService : ICameraReelNetworkService
    {
        private readonly ICameraReelClient client;

        public CameraReelNetworkService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public UniTask GetScreenshot(string placeUUID, CancellationToken ct, bool renewCache = false) =>
            throw new NotImplementedException();

        public UniTask PutScreenshot(string placeUUID, CancellationToken ct, bool renewCache = false) =>
            throw new NotImplementedException();
    }
}
