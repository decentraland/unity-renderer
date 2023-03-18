using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.CommonBehavior;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System.Threading;
using UnityEngine;

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

        public Vector2 Pivot { get; }

        private MapMarkerPoolableBehavior<HotUserMarkerObject> poolableBehavior;

        internal HotUserMarker(IUnityObjectPool<HotUserMarkerObject> pool, IMapCullingController mapCullingController, ICoordsUtils coordsUtils, Vector3Variable worldOffset)
        {
            this.coordsUtils = coordsUtils;
            this.worldOffset = worldOffset;
            this.cullingController = mapCullingController;
            this.Pivot = pool.Prefab.pivot;

            poolableBehavior = new MapMarkerPoolableBehavior<HotUserMarkerObject>(pool);
        }

        public void TrackPlayer(Player player)
        {
            cts = new CancellationTokenSource();

            CurrentPlayerId = player.id;
            TrackPosition(player, cts.Token).Forget();
        }

        private async UniTaskVoid TrackPosition(Player player, CancellationToken ct)
        {
            var startedTracking = false;

            // it takes the first value
            await foreach (var position in player.WorldPositionProp)
            {
                if (ct.IsCancellationRequested)
                    return;

                var gridPosition = Utils.WorldToGridPositionUnclamped(position + worldOffset.Get());
                poolableBehavior.SetCurrentPosition(coordsUtils.PivotPosition(this, coordsUtils.CoordsToPositionUnclamped(gridPosition)));

                if (startedTracking)
                    cullingController.SetTrackedObjectPositionDirty(this);
                else
                {
                    cullingController.StartTracking(this, this);
                    startedTracking = true;
                }
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
            OnMapObjectCulled(this);
            cullingController.StopTracking(this);
            ResetPlayer();
        }

        public void OnMapObjectBecameVisible(IHotUserMarker obj)
        {
            poolableBehavior.OnBecameVisible();
        }

        public void OnMapObjectCulled(IHotUserMarker obj)
        {
            poolableBehavior.OnBecameInvisible();
            // Keep tracking position
        }
    }
}
