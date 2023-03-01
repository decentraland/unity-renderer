using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.CommonBehavior;
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

        private readonly IMapCullingController cullingController;

        private CancellationTokenSource cts;

        public string CurrentPlayerId { get; private set; }
        public Vector3 CurrentPosition => poolableBehavior.currentPosition;

        private MapMarkerPoolableBehavior<HotUserMarkerObject> poolableBehavior;

        internal HotUserMarker(IObjectPool<HotUserMarkerObject> pool, IMapCullingController mapCullingController, ICoordsUtils coordsUtils, Vector3Variable worldOffset)
        {
            this.coordsUtils = coordsUtils;
            this.worldOffset = worldOffset;
            this.cullingController = mapCullingController;

            poolableBehavior = new MapMarkerPoolableBehavior<HotUserMarkerObject>(pool);
        }

        public void TrackPlayer(Player player)
        {
            cts = new CancellationTokenSource();

            CurrentPlayerId = player.id;
            TrackPosition(player, cts.Token).Forget();
        }

        public void OnBecameInvisible()
        {
            poolableBehavior.OnBecameInvisible();

            // Keep tracking position
        }

        public void OnBecameVisible()
        {
            poolableBehavior.OnBecameVisible();
        }

        private async UniTaskVoid TrackPosition(Player player, CancellationToken ct)
        {
            // it takes the first value
            await foreach (var position in player.WorldPositionProp)
            {
                if (ct.IsCancellationRequested)
                    return;

                var gridPosition = Utils.WorldToGridPositionUnclamped(position + worldOffset.Get());
                poolableBehavior.SetCurrentPosition(coordsUtils.CoordsToPositionUnclamped(gridPosition));

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
