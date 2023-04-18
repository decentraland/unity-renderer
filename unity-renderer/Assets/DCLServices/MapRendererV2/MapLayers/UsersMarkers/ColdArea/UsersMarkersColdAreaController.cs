using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Controllers.HotScenes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    /// <summary>
    /// This controller is responsible for updating User Markers outside of the comms area.
    /// Whereas the comms area is the region around the player where avatars (and therefore the users) are fetched according to the completely different logic
    /// </summary>
    internal partial class UsersMarkersColdAreaController : MapLayerControllerBase, IMapLayerController, IMapCullingListener<IColdUserMarker>
    {
        internal delegate IColdUserMarker ColdUserMarkerBuilder(
            ColdUserMarkerObject prefab,
            Transform parent);

        internal const int CREATE_PER_BATCH = 20;
        internal const int COMMS_RADIUS_THRESHOLD = 2;

        private IHotScenesFetcher hotScenesFetcher;

        private readonly ColdUserMarkerObject prefab;
        private readonly int maxMarkers;
        private readonly ColdUserMarkerBuilder builder;
        private readonly BaseVariable<string> realmName;
        private readonly Vector2IntVariable userPosition;

        private ExclusionAreaProvider exclusionArea;
        private ColdUserMarkersStorage storage;

        private CancellationTokenSource cancellationTokenSource;

        public UsersMarkersColdAreaController(Transform parent, ColdUserMarkerObject prefab, ColdUserMarkerBuilder builder,
            IHotScenesFetcher hotScenesFetcher, BaseVariable<string> realmName, Vector2IntVariable userPosition,
            KernelConfig kernelConfig, ICoordsUtils coordsUtils, IMapCullingController cullingController, int maxMarkers)
            : base(parent, coordsUtils, cullingController)
        {
            this.prefab = prefab;
            this.maxMarkers = maxMarkers;
            this.builder = builder;
            this.realmName = realmName;
            this.userPosition = userPosition;
            this.hotScenesFetcher = hotScenesFetcher;

            exclusionArea = new ExclusionAreaProvider(kernelConfig, COMMS_RADIUS_THRESHOLD);
        }

        public async UniTask Initialize(CancellationToken cancellationToken)
        {
            cancellationTokenSource = LinkWithDisposeToken(cancellationToken);
            cancellationToken = cancellationTokenSource.Token;

            var instances = new IColdUserMarker[maxMarkers];

            async UniTask InstantiateMarkers(CancellationToken ct)
            {
                for (var i = 0; i < maxMarkers;)
                {
                    var batchSize = Mathf.Min(maxMarkers - i, CREATE_PER_BATCH);

                    for (var j = 0; j < batchSize; j++)
                    {
                        var marker = builder(prefab, instantiationParent);
                        marker.SetActive(false);
                        instances[i] = marker;
                        i++;
                    }

                    // NextFrame does not work in Tests (see the implementation, there is a special case if Application is not running)
                    await UniTask.DelayFrame(1, cancellationToken: ct);
                }

                storage = new ColdUserMarkersStorage(instances, SetData);
            }

            await UniTask.WhenAll(InstantiateMarkers(cancellationToken), exclusionArea.Initialize(cancellationToken));

            realmName.OnChange += OnRealmNameChange;
            userPosition.OnChange += OnUserPositionChange;
        }

        private void SetData(IColdUserMarker marker, (string realmServer, Vector2Int coords) data)
        {
            var randomizedCoords = new Vector2Int(
                RandomizeCoord(data.coords.x),
                RandomizeCoord(data.coords.y));

            var position = coordsUtils.CoordsToPosition(randomizedCoords, marker);
            marker.SetData(data.realmServer, realmName.Get(), data.coords, position);
            ResolveVisibility(marker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RandomizeCoord(int coord) =>
            Mathf.RoundToInt(coord + Random.Range(-0.5f, 0.5f));

        private void ResolveVisibility(IColdUserMarker marker)
        {
            marker.SetActive(!exclusionArea.Contains(marker.Coords));
        }

        protected override void DisposeImpl()
        {
            realmName.OnChange -= OnRealmNameChange;
            userPosition.OnChange -= OnUserPositionChange;
            storage.Dispose();
        }

        private async UniTaskVoid ColdAreasUpdateLoop(CancellationToken cancellationToken)
        {
            await foreach (var data in hotScenesFetcher.ScenesInfo)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                RenewSceneInfos(data);
            }
        }

        private void RenewSceneInfos(IReadOnlyList<IHotScenesController.HotSceneInfo> sceneInfos)
        {
            // it is forbidden to declare Spans variables in the async flow so we have to have a separate sync method for that
            storage.Update(sceneInfos, out var newRents, out var recycledRents);

            foreach (var marker in recycledRents)
                mapCullingController.StopTracking(marker);

            foreach (var marker in newRents)
            {
                mapCullingController.StartTracking(marker, this);
                mapCullingController.SetTrackedObjectPositionDirty(marker);
            }
        }

        private void OnRealmNameChange(string current, string previous)
        {
            if (!string.IsNullOrEmpty(current))
                ResolveRealm(current);
        }

        private void ResolveRealm(string realmName)
        {
            foreach (var marker in storage.Markers)
                marker.OnRealmChanged(realmName);
        }

        private void OnUserPositionChange(Vector2Int current, Vector2Int previous)
        {
            exclusionArea.SetExclusionAreaCenter(current);

            foreach (IColdUserMarker userMarker in storage.Markers)
                ResolveVisibility(userMarker);
        }

        public void OnMapObjectBecameVisible(IColdUserMarker marker)
        {
            marker.SetCulled(false);
        }

        public void OnMapObjectCulled(IColdUserMarker marker)
        {
            marker.SetCulled(true);
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            hotScenesFetcher.SetUpdateMode(IHotScenesFetcher.UpdateMode.FOREGROUND);
            ColdAreasUpdateLoop(LinkWithDisposeToken(cancellationToken).Token).Forget();
            return UniTask.CompletedTask;
        }

        public UniTask Disable(CancellationToken cancellationToken)
        {
            hotScenesFetcher.SetUpdateMode(IHotScenesFetcher.UpdateMode.BACKGROUND);

            // cancellation of `ColdAreasUpdateLoop` is handled by the cancellation token
            return UniTask.CompletedTask;
        }
    }
}
