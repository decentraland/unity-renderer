using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkService : IService
    {
        UniTask GetImage(string imageUUID, CancellationToken ct = default, bool renewCache = false);
    }

    public class CameraReelNetworkService : ICameraReelNetworkService
    {
        private readonly ICameraReelClient client;

        public CameraReelNetworkService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public async UniTask GetImage(string imageUUID, CancellationToken ct, bool renewCache = false)
        {
            Debug.Log("SERVICE - request");
            await client.GetImage("095c0e1a-63d5-4329-aad9-8e511704a971/metadata", ct);
            Debug.Log("SERVICE - response");
        }

        public void Dispose() { }

    }
}
