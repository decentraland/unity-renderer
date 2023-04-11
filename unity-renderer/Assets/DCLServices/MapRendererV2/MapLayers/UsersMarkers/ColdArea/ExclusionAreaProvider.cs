using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal struct ExclusionAreaProvider
    {
        private ExclusionArea exclusionArea;

        private readonly int commsRadiusThreshold;
        private readonly KernelConfig kernelConfig;

        public ExclusionAreaProvider(KernelConfig kernelConfig, int commsRadiusThreshold) : this()
        {
            this.kernelConfig = kernelConfig;
            this.commsRadiusThreshold = commsRadiusThreshold;
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            var kernelConfigPromise = kernelConfig.EnsureConfigInitialized();
            await kernelConfigPromise.WithCancellation(cancellationToken);
            exclusionArea.Radius = (int)kernelConfigPromise.value.comms.commRadius + commsRadiusThreshold;
        }

        public void SetExclusionAreaCenter(Vector2Int center)
        {
            exclusionArea.Position = center;
        }

        public readonly bool Contains(in Vector2Int coords) =>
            exclusionArea.Contains(in coords);
    }
}
