using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.PlacesAPIService;
using System;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class NavmapToastViewController : IDisposable
    {
        private readonly MinimapMetadata minimapMetadata;
        private readonly NavmapToastView view;
        private readonly MapRenderImage mapRenderImage;
        private readonly float sqrDistanceToCloseView;

        private Vector2 lastClickPosition;
        private Vector2Int currentParcel;
        private IPlacesAPIService placesAPIService;
        private readonly IPlacesAnalytics placesAnalytics;

        private CancellationTokenSource disposingCts = new CancellationTokenSource();
        private CancellationTokenSource retrievingFavoritesCts;

        public NavmapToastViewController(
            MinimapMetadata minimapMetadata,
            NavmapToastView view,
            MapRenderImage mapRenderImage,
            IPlacesAPIService placesAPIService,
            IPlacesAnalytics placesAnalytics
            )
        {
            this.placesAPIService = placesAPIService;
            this.placesAnalytics = placesAnalytics;
            this.minimapMetadata = minimapMetadata;
            this.view = view;
            this.mapRenderImage = mapRenderImage;
            this.sqrDistanceToCloseView = view.distanceToCloseView * view.distanceToCloseView;
            this.view.OnFavoriteToggleClicked += OnFavoriteToggleClicked;
        }

        public void Activate()
        {
            Unsubscribe();

            mapRenderImage.ParcelClicked += OnParcelClicked;
            mapRenderImage.Hovered += OnHovered;
            mapRenderImage.DragStarted += OnDragStarted;
            minimapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
        }

        public void Deactivate()
        {
            view.Close();
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            mapRenderImage.ParcelClicked -= OnParcelClicked;
            mapRenderImage.Hovered -= OnHovered;
            mapRenderImage.DragStarted -= OnDragStarted;
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
        }

        private void OnHovered(Vector2 localPosition)
        {
            if (!view.gameObject.activeSelf)
                return;

            if (Vector2.SqrMagnitude(localPosition - lastClickPosition) >= sqrDistanceToCloseView)
                view.Close();
        }

        private void OnDragStarted()
        {
            if (!view.gameObject.activeSelf)
                return;

            view.Close();
        }

        private void OnParcelClicked(MapRenderImage.ParcelClickData parcelClickData)
        {
            lastClickPosition = parcelClickData.WorldPosition;
            currentParcel = parcelClickData.Parcel;

            // transform coordinates from rect coordinates to parent of view coordinates
            //currentPosition = GetViewLocalPosition(lastClickPosition);
            view.Open(currentParcel, lastClickPosition);
            RetrieveFavoriteState();
        }

        private void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!view.gameObject.activeInHierarchy)
                return;

            var updatedCurrentLocationInfo = false;
            foreach (Vector2Int parcel in sceneInfo.parcels)
            {
                if (parcel == currentParcel)
                {
                    updatedCurrentLocationInfo = true;
                    break;
                }
            }

            if (updatedCurrentLocationInfo)
                view.Populate(currentParcel, lastClickPosition, sceneInfo);
        }

        private void RetrieveFavoriteState()
        {
            retrievingFavoritesCts?.SafeCancelAndDispose();
            retrievingFavoritesCts = CancellationTokenSource.CreateLinkedTokenSource(disposingCts.Token);

            RetrieveFavoriteStateAsync(retrievingFavoritesCts.Token).Forget();
        }

        private async UniTaskVoid RetrieveFavoriteStateAsync(CancellationToken ct)
        {
            try
            {
                view.SetFavoriteLoading(true);
                var place = await placesAPIService.GetPlace(currentParcel, ct);
                view.SetIsAPlace(true);
                bool isFavorite = await placesAPIService.IsFavoritePlace(place, ct);
                view.SetFavoriteLoading(false);
                view.SetCurrentFavoriteStatus(place.id, isFavorite);
            }
            catch (NotAPlaceException)
            {
                view.SetIsAPlace(false);
            }
            catch (OperationCanceledException)
            {
                view.SetFavoriteLoading(true);
            }
        }

        private void OnFavoriteToggleClicked(string uuid, bool isFavorite)
        {
            if(isFavorite)
                placesAnalytics.AddFavorite(uuid, IPlacesAnalytics.ActionSource.FromNavmap);
            else
                placesAnalytics.RemoveFavorite(uuid, IPlacesAnalytics.ActionSource.FromNavmap);

            placesAPIService.SetPlaceFavorite(uuid, isFavorite, default).Forget();
        }

        public void Dispose()
        {
            disposingCts?.SafeCancelAndDispose();
            Unsubscribe();
        }
    }
}
