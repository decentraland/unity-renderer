using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    internal class HotUserMarker : IHotUserMarker
    {
        private readonly ICoordsUtils coordsUtils;
        private readonly Vector3Variable worldOffset;

        private readonly IObjectPool<HotUserMarkerObject> objectPool;
        private readonly IMapCullingController cullingController;

        private CancellationTokenSource cts;
        public string CurrentPlayerId { get; private set; }
        public Vector3 CurrentPosition { get; private set; }

        private HotUserMarkerObject instance;

        private bool isVisible;

        internal HotUserMarker(IObjectPool<HotUserMarkerObject> pool, IMapCullingController mapCullingController, ICoordsUtils coordsUtils, Vector3Variable worldOffset)
        {
            this.objectPool = pool;
            this.coordsUtils = coordsUtils;
            this.worldOffset = worldOffset;
            this.cullingController = mapCullingController;
        }

        public void TrackPlayer(Player player)
        {
            cts = new CancellationTokenSource();

            CurrentPlayerId = player.id;
            TrackPosition(player, cts.Token).Forget();
        }

        public void OnBecameInvisible()
        {
            if (instance)
            {
                objectPool.Release(instance);
                instance = null;
            }

            isVisible = false;

            // Keep tracking position
        }

        public void OnBecameVisible()
        {
            isVisible = true;
            instance = objectPool.Get();
            instance.transform.localPosition = CurrentPosition;
        }

        private async UniTaskVoid TrackPosition(Player player, CancellationToken ct)
        {
            // it takes the first value
            await foreach (var position in player.WorldPositionProp)
            {
                if (ct.IsCancellationRequested)
                    return;

                var gridPosition = Utils.WorldToGridPositionUnclamped(position + worldOffset.Get());
                CurrentPosition = coordsUtils.CoordsToPositionUnclamped(gridPosition);

                if (isVisible)
                    instance.transform.localPosition = CurrentPosition;

                cullingController.SetTrackedObjectPositionDirty(this);
            }
        }

        private void ResetPlayer()
        {
            CurrentPlayerId = null;
            cts?.Cancel();
            cts?.Dispose();
        }

        public void Dispose()
        {
            OnBecameInvisible();
            cullingController.StopTracking(this);
            ResetPlayer();
        }
    }
}
