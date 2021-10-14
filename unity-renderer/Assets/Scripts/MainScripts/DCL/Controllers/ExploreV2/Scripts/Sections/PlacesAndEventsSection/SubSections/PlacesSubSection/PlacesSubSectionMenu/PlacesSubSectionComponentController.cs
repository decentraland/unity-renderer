using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public interface IPlacesSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all places from the API.
    /// </summary>
    void RequestAllPlaces();

    /// <summary>
    /// Load the places with the last requested ones.
    /// </summary>
    void LoadPlaces();
}

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    internal event Action OnPlacesFromAPIUpdated;

    internal IPlacesSubSectionComponentView view;
    internal IPlacesAPIController placesAPIApiController;
    internal FriendTrackerController friendsTrackerController;
    internal List<HotSceneInfo> placesFromAPI = new List<HotSceneInfo>();
    internal bool reloadPlaces = false;

    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view, IPlacesAPIController placesAPI, IFriendsController friendsController)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        placesAPIApiController = placesAPI;
        OnPlacesFromAPIUpdated += OnRequestedPlacesUpdated;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);
    }

    private void FirstLoading()
    {
        reloadPlaces = true;
        RequestAllPlaces();

        view.OnPlacesSubSectionEnable += RequestAllPlaces;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    private void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadPlaces = true;
    }

    public void RequestAllPlaces()
    {
        if (!reloadPlaces)
            return;

        view.SetPlacesAsLoading(true);
        RequestAllPlacesFromAPI();
        reloadPlaces = false;
    }

    internal void RequestAllPlacesFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            (placeList) =>
            {
                placesFromAPI = placeList;
                OnPlacesFromAPIUpdated?.Invoke();
            });
    }

    internal void OnRequestedPlacesUpdated() { LoadPlaces(); }

    public void LoadPlaces()
    {
        friendsTrackerController.RemoveAllHandlers();

        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI.Take(50).ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetPlaces(places);
        view.SetPlacesAsLoading(false);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnPlacesSubSectionEnable -= RequestAllPlaces;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        OnPlacesFromAPIUpdated -= OnRequestedPlacesUpdated;
        DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
    }

    internal PlaceCardComponentModel CreatePlaceCardModelFromAPIPlace(HotSceneInfo placeFromAPI)
    {
        PlaceCardComponentModel placeCardModel = new PlaceCardComponentModel();

        // Card data
        placeCardModel.placePictureUri = placeFromAPI.thumbnail;
        placeCardModel.placeName = placeFromAPI.name;
        placeCardModel.placeDescription = FormatDescription(placeFromAPI);
        placeCardModel.placeAuthor = FormatAuthorName(placeFromAPI);
        placeCardModel.numberOfUsers = placeFromAPI.usersTotalCount;
        placeCardModel.parcels = placeFromAPI.parcels;
        placeCardModel.hotSceneInfo = placeFromAPI;

        // Card events
        ConfigureOnJumpInActions(placeCardModel, placeFromAPI);
        ConfigureOnInfoActions(placeCardModel);

        return placeCardModel;
    }

    internal string FormatDescription(HotSceneInfo placeFromAPI) { return string.IsNullOrEmpty(placeFromAPI.description) ? "The author hasn't written a description yet." : placeFromAPI.description; }

    internal string FormatAuthorName(HotSceneInfo placeFromAPI) { return $"Author <b>{placeFromAPI.creator}</b>"; }

    internal void ConfigureOnJumpInActions(PlaceCardComponentModel placeModel, HotSceneInfo placeFromAPI)
    {
        placeModel.onJumpInClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        placeModel.onJumpInClick.AddListener(RequestExploreV2Closing);
        placeModel.onJumpInClick.AddListener(() => JumpInToPlace(placeFromAPI));
    }

    internal void ConfigureOnInfoActions(PlaceCardComponentModel placeModel)
    {
        placeModel.onInfoClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        placeModel.onInfoClick.AddListener(() => view.ShowPlaceModal(placeModel));
    }

    internal static void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        placeFromAPI.realms = placeFromAPI.realms.OrderByDescending(x => x.usersCount).ToArray();

        Vector2Int coords = placeFromAPI.baseCoords;
        string serverName = placeFromAPI.realms.Length == 0 ? "" : placeFromAPI.realms[0].serverName;
        string layerName = placeFromAPI.realms.Length == 0 ? "" : placeFromAPI.realms[0].layer;

        if (string.IsNullOrEmpty(serverName))
            WebInterface.GoTo(coords.x, coords.y);
        else
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);
    }

    internal void RequestExploreV2Closing()
    {
        view.HidePlaceModal();
        OnCloseExploreV2?.Invoke();
    }

    internal void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) { friendsTrackerController.AddHandler(friendsHandler); }
}