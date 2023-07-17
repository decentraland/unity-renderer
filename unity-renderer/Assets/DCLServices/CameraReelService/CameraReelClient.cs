using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask GetScreenshot(string placeUUID, CancellationToken ct);

        UniTask PutScreenshot(string placeUUID, CancellationToken ct);
    }

    public class CameraReelClient : ICameraReelClient
    {
        private const string BASE_URL = "https://places.decentraland.org/api/places";

        private readonly IWebRequestController webRequestController;

        public CameraReelClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public UniTask GetScreenshot(string placeUUID, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask PutScreenshot(string placeUUID, CancellationToken ct) =>
            throw new NotImplementedException();
    }
}
