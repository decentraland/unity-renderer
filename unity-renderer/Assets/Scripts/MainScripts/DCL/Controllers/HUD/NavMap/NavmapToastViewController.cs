using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class NavmapToastViewController : IDisposable, INavmapToastViewController
    {
        private readonly MinimapMetadata minimapMetadata;
        private readonly NavmapToastView view;
        private readonly IPlaceCardComponentView placeCardModal;
        private readonly MapRenderImage mapRenderImage;
        private readonly float sqrDistanceToCloseView;

        private Vector2 lastClickPosition;
        private Vector2Int currentParcel;
        private readonly IPlacesAPIService placesAPIService;
        private readonly IPlacesAnalytics placesAnalytics;

        private readonly CancellationTokenSource disposingCts = new ();
        private CancellationTokenSource retrievingFavoritesCts;
        private bool showUntilClick;

        public NavmapToastViewController(
            MinimapMetadata minimapMetadata,
            NavmapToastView view,
            MapRenderImage mapRenderImage,
            IPlacesAPIService placesAPIService,
            IPlacesAnalytics placesAnalytics,
            IPlaceCardComponentView placeCardModal
            )
        {
            this.placesAPIService = placesAPIService;
            this.placesAnalytics = placesAnalytics;
            this.minimapMetadata = minimapMetadata;
            this.view = view;
            this.mapRenderImage = mapRenderImage;
            this.placeCardModal = placeCardModal;
            this.sqrDistanceToCloseView = view.distanceToCloseView * view.distanceToCloseView;

            this.view.OnFavoriteToggleClicked += OnFavoriteToggleClicked;
            this.view.OnGoto += JumpIn;
            this.view.OnInfoClick += EnablePlaceCardModal;

            this.placeCardModal.OnFavoriteChanged += OnFavoriteToggleClicked;
            this.placeCardModal.OnVoteChanged += ChangeVote;
        }

        private void EnablePlaceCardModal() =>
            placeCardModal.SetActive(true);

        private void JumpIn(int x, int y)
        {
            DataStore.i.HUDs.navmapVisible.Set(false);
            Environment.i.world.teleportController.Teleport(x, y);
        }

        public void Activate()
        {
            Unsubscribe();

            mapRenderImage.ParcelClicked += ShowPlaceToast;
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
            mapRenderImage.ParcelClicked -= ShowPlaceToast;
            mapRenderImage.Hovered -= OnHovered;
            mapRenderImage.DragStarted -= OnDragStarted;
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;

            view.OnFavoriteToggleClicked -= OnFavoriteToggleClicked;
            view.OnGoto -= JumpIn;

            placeCardModal.OnFavoriteChanged -= OnFavoriteToggleClicked;
        }

        public void CloseCurrentToast() =>
            view.Close();

        private void OnHovered(Vector2 localPosition)
        {
            if (!view.gameObject.activeSelf || showUntilClick)
                return;

            if (Vector2.SqrMagnitude(localPosition - lastClickPosition) >= sqrDistanceToCloseView)
                view.Close();
        }

        private void OnDragStarted()
        {
            if (!view.gameObject.activeSelf)
                return;

            showUntilClick = false;
            view.Close();
        }

        public void ShowPlaceToast(MapRenderImage.ParcelClickData parcelClickData, bool showUntilClick)
        {
            ShowPlaceToast(parcelClickData);
            this.showUntilClick = showUntilClick;
        }

        private void ShowPlaceToast(MapRenderImage.ParcelClickData parcelClickData)
        {
            showUntilClick = false;

            lastClickPosition = parcelClickData.WorldPosition;
            currentParcel = parcelClickData.Parcel;

            // transform coordinates from rect coordinates to parent of view coordinates
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

            RetrieveAdditionalData(retrievingFavoritesCts.Token).Forget();
        }

        private async UniTaskVoid RetrieveAdditionalData(CancellationToken ct)
        {
            try
            {
                view.SetFavoriteLoading(true);
                var place = await placesAPIService.GetPlace(currentParcel, ct);
                view.SetIsAPlace(true);
                bool isFavorite = await placesAPIService.IsFavoritePlace(place, ct);
                view.SetFavoriteLoading(false);
                view.SetCurrentFavoriteStatus(place.id, isFavorite);
                SetPlaceCardModalData(place);
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

        private void SetPlaceCardModalData(IHotScenesController.PlaceInfo place)
        {
            placeCardModal.SetCoords(currentParcel);
            placeCardModal.SetDeployedAt(place.deployed_at);
            placeCardModal.SetPlaceAuthor(place.contact_name);
            placeCardModal.SetPlaceName(place.title);
            placeCardModal.SetPlaceDescription(place.description);
            placeCardModal.SetUserVisits(place.user_visits);
            placeCardModal.SetNumberOfUsers(place.user_count);
            placeCardModal.SetUserRating(place.like_rate_as_float);
            placeCardModal.SetNumberOfFavorites(place.favorites);
            placeCardModal.SetTotalVotes(place.likes + place.dislikes);
            placeCardModal.SetFavoriteButton(place.user_favorite, place.id);
            placeCardModal.SetVoteButtons(place.user_like, place.user_dislike);
        }

        private void OnFavoriteToggleClicked(string uuid, bool isFavorite)
        {
            if(isFavorite)
                placesAnalytics.AddFavorite(uuid, IPlacesAnalytics.ActionSource.FromNavmap);
            else
                placesAnalytics.RemoveFavorite(uuid, IPlacesAnalytics.ActionSource.FromNavmap);

            placesAPIService.SetPlaceFavorite(uuid, isFavorite, default).Forget();
        }

        private void ChangeVote(string placeId, bool? isUpvote)
        {
            if (isUpvote != null)
            {
                if (isUpvote.Value)
                    placesAnalytics.Like(placeId, IPlacesAnalytics.ActionSource.FromFavorites);
                else
                    placesAnalytics.Dislike(placeId, IPlacesAnalytics.ActionSource.FromFavorites);
            }
            else
                placesAnalytics.RemoveVote(placeId, IPlacesAnalytics.ActionSource.FromFavorites);

            placesAPIService.SetPlaceVote(isUpvote, placeId, default).Forget();
        }

        public void Dispose()
        {
            disposingCts?.SafeCancelAndDispose();
            Unsubscribe();
        }
    }
}
