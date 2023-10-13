using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Favorites
{
    internal class FavoritesMarkerController : MapLayerControllerBase, IMapCullingListener<IFavoritesMarker>, IMapLayerController, IZoomScalingLayer
    {
        internal const int PREWARM_PER_FRAME = 20;
        private const string EMPTY_PARCEL_NAME = "Empty parcel";

        internal delegate IFavoritesMarker FavoritesMarkerBuilder(
            IUnityObjectPool<FavoriteMarkerObject> objectsPool,
            IMapCullingController cullingController);

        private readonly IPlacesAPIService placesAPIService;
        private readonly IUnityObjectPool<FavoriteMarkerObject> objectsPool;
        private readonly FavoritesMarkerBuilder builder;
        private readonly int prewarmCount;

        private readonly Dictionary<IHotScenesController.PlaceInfo, IFavoritesMarker> markers = new ();

        private bool isEnabled;

        public IReadOnlyDictionary<IHotScenesController.PlaceInfo, IFavoritesMarker> Markers => markers;

        public FavoritesMarkerController(IPlacesAPIService placesAPIService,
            IUnityObjectPool<FavoriteMarkerObject> objectsPool, FavoritesMarkerBuilder builder,
            int prewarmCount, Transform instantiationParent, ICoordsUtils coordsUtils, IMapCullingController cullingController)
            : base(instantiationParent, coordsUtils, cullingController)
        {
            this.placesAPIService = placesAPIService;
            this.objectsPool = objectsPool;
            this.builder = builder;
            this.prewarmCount = prewarmCount;
        }

        public UniTask Initialize(CancellationToken cancellationToken) =>
            objectsPool.PrewarmAsync(prewarmCount, PREWARM_PER_FRAME, LinkWithDisposeToken(cancellationToken).Token);

        protected override void DisposeImpl()
        {
            objectsPool.Clear();

            foreach (IFavoritesMarker marker in markers.Values)
                marker.Dispose();

            markers.Clear();
        }

        public void OnMapObjectBecameVisible(IFavoritesMarker marker)
        {
            marker.OnBecameVisible();
        }

        private async UniTaskVoid GetFavorites(CancellationToken cancellationToken)
        {
            foreach (IHotScenesController.PlaceInfo placeInfo in await placesAPIService.GetFavorites(-1, -1, cancellationToken))
                OnMinimapSceneInfoUpdated(placeInfo);
        }

        public void OnMapObjectCulled(IFavoritesMarker marker)
        {
            marker.OnBecameInvisible();
        }

        public void ApplyCameraZoom(float baseZoom, float zoom)
        {
            foreach (IFavoritesMarker marker in markers.Values)
                marker.SetZoom(coordsUtils.ParcelSize, baseZoom, zoom);
        }

        public void ResetToBaseScale()
        {
            foreach (var marker in markers.Values)
                marker.ResetScale(coordsUtils.ParcelSize);
        }

        private void OnMinimapSceneInfoUpdated(IHotScenesController.PlaceInfo sceneInfo)
        {
            // Markers are not really updated, they can be just reported several times with essentially the same data
            if (!sceneInfo.user_favorite)
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

            marker.SetData(sceneInfo.title, position);

            markers.Add(sceneInfo, marker);

            if (isEnabled)
                mapCullingController.StartTracking(marker, this);
        }

        private static Vector2Int GetParcelsCenter(IHotScenesController.PlaceInfo sceneInfo)
        {
            Vector2 centerTile = Vector2.zero;

            for (var i = 0; i < sceneInfo.Positions.Length; i++)
            {
                Vector2Int parcel = sceneInfo.Positions[i];
                centerTile += parcel;
            }

            centerTile /= sceneInfo.Positions.Length;
            float distance = float.PositiveInfinity;
            Vector2Int centerParcel = Vector2Int.zero;

            for (var i = 0; i < sceneInfo.Positions.Length; i++)
            {
                var parcel = sceneInfo.Positions[i];

                if (Vector2.Distance(centerTile, parcel) < distance)
                {
                    distance = Vector2Int.Distance(centerParcel, parcel);
                    centerParcel = parcel;
                }
            }

            return centerParcel;
        }

        private static bool IsEmptyParcel(IHotScenesController.PlaceInfo sceneInfo) =>
            sceneInfo.title is EMPTY_PARCEL_NAME;

        public UniTask Disable(CancellationToken cancellationToken)
        {
            // Make markers invisible to release everything to the pool and stop tracking
            foreach (IFavoritesMarker marker in markers.Values)
            {
                mapCullingController.StopTracking(marker);
                marker.OnBecameInvisible();
            }

            isEnabled = false;

            return UniTask.CompletedTask;
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            GetFavorites(CancellationToken.None).Forget();
            foreach (IFavoritesMarker marker in markers.Values)
                mapCullingController.StartTracking(marker, this);

            isEnabled = true;

            return UniTask.CompletedTask;
        }
    }
}
