using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal class ScenesOfInterestMarkersController : MapLayerControllerBase, IMapCullingListener<ISceneOfInterestMarker>, IMapLayerController
    {
        internal const int PREWARM_PER_FRAME = 20;
        private const string EMPTY_PARCEL_NAME = "Empty parcel";

        internal delegate ISceneOfInterestMarker SceneOfInterestMarkerBuilder(
            IUnityObjectPool<SceneOfInterestMarkerObject> objectsPool,
            IMapCullingController cullingController);

        private readonly MinimapMetadata minimapMetadata;
        private readonly IUnityObjectPool<SceneOfInterestMarkerObject> objectsPool;
        private readonly SceneOfInterestMarkerBuilder builder;
        private readonly int prewarmCount;

        private readonly Dictionary<MinimapMetadata.MinimapSceneInfo, ISceneOfInterestMarker> markers = new ();

        private bool isEnabled;

        public IReadOnlyDictionary<MinimapMetadata.MinimapSceneInfo, ISceneOfInterestMarker> Markers => markers;

        public ScenesOfInterestMarkersController(MinimapMetadata minimapMetadata,
            IUnityObjectPool<SceneOfInterestMarkerObject> objectsPool, SceneOfInterestMarkerBuilder builder,
            int prewarmCount, Transform instantiationParent, ICoordsUtils coordsUtils, IMapCullingController cullingController)
            : base(instantiationParent, coordsUtils, cullingController)
        {
            this.minimapMetadata = minimapMetadata;
            this.objectsPool = objectsPool;
            this.builder = builder;
            this.prewarmCount = prewarmCount;
        }

        public UniTask Initialize(CancellationToken cancellationToken)
        {
            // non-blocking retrieval of scenes of interest happens independently on the minimap rendering
            foreach (MinimapMetadata.MinimapSceneInfo sceneInfo in minimapMetadata.SceneInfos)
                OnMinimapSceneInfoUpdated(sceneInfo);

            minimapMetadata.OnSceneInfoUpdated += OnMinimapSceneInfoUpdated;
            return objectsPool.PrewarmAsync(prewarmCount, PREWARM_PER_FRAME, LinkWithDisposeToken(cancellationToken).Token);
        }

        protected override void DisposeImpl()
        {
            objectsPool.Clear();

            foreach (ISceneOfInterestMarker marker in markers.Values)
                marker.Dispose();

            markers.Clear();

            minimapMetadata.OnSceneInfoUpdated -= OnMinimapSceneInfoUpdated;
        }

        public void OnMapObjectBecameVisible(ISceneOfInterestMarker marker)
        {
            marker.OnBecameVisible();
        }

        public void OnMapObjectCulled(ISceneOfInterestMarker marker)
        {
            marker.OnBecameInvisible();
        }

        private void OnMinimapSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            // Markers are not really updated, they can be just reported several times with essentially the same data

            if (!sceneInfo.isPOI)
                return;

            // if it was possible to update them then we need to cache by parcel coordinates instead
            // and recalculate the parcels centers accordingly
            if (markers.ContainsKey(sceneInfo))
                return;

            if (IsEmptyParcel(sceneInfo))
                return;

            var marker = builder(objectsPool, mapCullingController);

            var centerParcel = GetParcelsCenter(sceneInfo);
            var position = coordsUtils.CoordsToPosition(centerParcel, marker);

            marker.SetData(sceneInfo.name, position);

            markers.Add(sceneInfo, marker);

            if (isEnabled)
                mapCullingController.StartTracking(marker, this);
        }

        private static Vector2Int GetParcelsCenter(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            Vector2 centerTile = Vector2.zero;

            for (var i = 0; i < sceneInfo.parcels.Count; i++)
            {
                Vector2Int parcel = sceneInfo.parcels[i];
                centerTile += parcel;
            }

            centerTile /= sceneInfo.parcels.Count;
            float distance = float.PositiveInfinity;
            Vector2Int centerParcel = Vector2Int.zero;

            for (var i = 0; i < sceneInfo.parcels.Count; i++)
            {
                var parcel = sceneInfo.parcels[i];

                if (Vector2.Distance(centerTile, parcel) < distance)
                {
                    distance = Vector2Int.Distance(centerParcel, parcel);
                    centerParcel = parcel;
                }
            }

            return centerParcel;
        }

        private static bool IsEmptyParcel(MinimapMetadata.MinimapSceneInfo sceneInfo) =>
            sceneInfo.name is EMPTY_PARCEL_NAME;

        public UniTask Disable(CancellationToken cancellationToken)
        {
            // Make markers invisible to release everything to the pool and stop tracking
            foreach (ISceneOfInterestMarker marker in markers.Values)
            {
                mapCullingController.StopTracking(marker);
                marker.OnBecameInvisible();
            }

            isEnabled = false;

            return UniTask.CompletedTask;
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            foreach (ISceneOfInterestMarker marker in markers.Values)
                mapCullingController.StartTracking(marker, this);

            isEnabled = true;

            return UniTask.CompletedTask;
        }
    }
}
