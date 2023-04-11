using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;
using System.Threading;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public class HighlightsSubSectionComponentController : IHighlightsSubSectionComponentController, IPlacesAndEventsAPIRequester
{
    public event Action OnCloseExploreV2;
    public event Action OnGoToEventsSubSection;

    internal const int DEFAULT_NUMBER_OF_TRENDING_PLACES = 10;
    private const int DEFAULT_NUMBER_OF_FEATURED_PLACES = 9;
    private const int DEFAULT_NUMBER_OF_LIVE_EVENTS = 3;

    internal readonly IHighlightsSubSectionComponentView view;
    internal readonly IPlacesAPIController placesAPIApiController;
    internal readonly IEventsAPIController eventsAPIApiController;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly DataStore dataStore;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<PlaceInfo> placesFromAPI = new ();
    internal List<EventFromAPIModel> eventsFromAPI = new ();
    private CancellationTokenSource cts = new ();

    public HighlightsSubSectionComponentController(
        IHighlightsSubSectionComponentView view,
        IPlacesAPIController placesAPI,
        IEventsAPIController eventsAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnPlaceInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnPlaceJumpInClicked += JumpInToPlace;
        this.view.OnFavoriteClicked += View_OnFavoritesClicked;

        this.view.OnEventInfoClicked += ShowEventDetailedInfo;
        this.view.OnEventJumpInClicked += JumpInToEvent;

        this.view.OnEventSubscribeEventClicked += SubscribeToEvent;
        this.view.OnEventUnsubscribeEventClicked += UnsubscribeToEvent;

        this.view.OnViewAllEventsClicked += GoToEventsSubSection;

        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        placesAPIApiController = placesAPI;
        eventsAPIApiController = eventsAPI;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;

        view.OnPlaceInfoClicked -= ShowPlaceDetailedInfo;
        view.OnEventInfoClicked -= ShowEventDetailedInfo;

        view.OnPlaceJumpInClicked -= JumpInToPlace;
        view.OnEventJumpInClicked -= JumpInToEvent;
        this.view.OnFavoriteClicked -= View_OnFavoritesClicked;

        view.OnEventSubscribeEventClicked -= SubscribeToEvent;
        view.OnEventUnsubscribeEventClicked -= UnsubscribeToEvent;

        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;

        view.OnViewAllEventsClicked -= GoToEventsSubSection;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }

    private void View_OnFavoritesClicked(string placeUUID, bool isFavorite)
    {
        if (isFavorite)
        {
            exploreV2Analytics.AddFavorite(placeUUID);
        }
        else
        {
            exploreV2Analytics.RemoveFavorite(placeUUID);
        }
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        placesAPIApiController.SetPlaceFavorite(placeUUID, isFavorite, cts.Token);
    }

    private void FirstLoading()
    {
        view.OnHighlightsSubSectionEnable += RequestAllPlacesAndEvents;
        cardsReloader.Initialize();
    }

    internal void RequestAllPlacesAndEvents()
    {
        if (cardsReloader.CanReload())
            cardsReloader.RequestAll();
    }

    public void RequestAllFromAPI()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        placesAPIApiController.GetAllPlacesFromPlacesAPI(
            OnCompleted: (placeList, total) =>
            {
                placesFromAPI = placeList;

                eventsAPIApiController.GetAllEvents(
                    OnSuccess: eventList =>
                    {
                        eventsFromAPI = eventList;
                        OnRequestedPlacesAndEventsUpdated();
                    },
                    OnFail: error =>
                    {
                        OnRequestedPlacesAndEventsUpdated();
                        Debug.LogError($"Error receiving events from the API: {error}");
                    });
            }, 0, 20, cts.Token);
    }

    internal void OnRequestedPlacesAndEventsUpdated()
    {
        friendsTrackerController.RemoveAllHandlers();

        List<PlaceCardComponentModel> trendingPlaces = PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(FilterTrendingPlaces());
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(FilterTrendingEvents(trendingPlaces.Count));
        view.SetTrendingPlacesAndEvents(trendingPlaces, trendingEvents);

        view.SetFeaturedPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(FilterFeaturedPlaces()));
        view.SetLiveEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterLiveEvents()));
    }

    internal List<PlaceInfo> FilterTrendingPlaces() => placesFromAPI.Take(DEFAULT_NUMBER_OF_TRENDING_PLACES).ToList();
    internal List<EventFromAPIModel> FilterLiveEvents() => eventsFromAPI.Where(x => x.live).Take(DEFAULT_NUMBER_OF_LIVE_EVENTS).ToList();
    internal List<EventFromAPIModel> FilterTrendingEvents(int amount) => eventsFromAPI.Where(e => e.highlighted).Take(amount).ToList();
    internal List<PlaceInfo> FilterFeaturedPlaces()
    {
        List<PlaceInfo> featuredPlaces;

        if (placesFromAPI.Count >= DEFAULT_NUMBER_OF_TRENDING_PLACES)
        {
            int numberOfPlaces = placesFromAPI.Count >= (DEFAULT_NUMBER_OF_TRENDING_PLACES + DEFAULT_NUMBER_OF_FEATURED_PLACES)
                ? DEFAULT_NUMBER_OF_FEATURED_PLACES
                : placesFromAPI.Count - DEFAULT_NUMBER_OF_TRENDING_PLACES;

            featuredPlaces = placesFromAPI
                            .GetRange(DEFAULT_NUMBER_OF_TRENDING_PLACES, numberOfPlaces)
                            .ToList();
        }
        else if (placesFromAPI.Count > 0)
            featuredPlaces = placesFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_PLACES).ToList();
        else
            featuredPlaces = new List<PlaceInfo>();

        return featuredPlaces;
    }

    private void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) =>
        friendsTrackerController.AddHandler(friendsHandler);

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.placeInfo.id, placeModel.placeName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Events);
    }

    internal void JumpInToPlace(PlaceInfo placeFromAPI)
    {
        PlacesSubSectionComponentController.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.title, Utils.ConvertStringToVector(placeFromAPI.base_position));
    }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        EventsSubSectionComponentController.JumpInToEvent(eventFromAPI);
        view.HideEventModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]));
    }

    private void SubscribeToEvent(string eventId) =>
        eventsAPIApiController.RegisterParticipation(eventId);

    private void UnsubscribeToEvent(string eventId) =>
        eventsAPIApiController.RemoveParticipation(eventId);

    internal void GoToEventsSubSection() =>
        OnGoToEventsSubSection?.Invoke();

    private void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (!string.IsNullOrEmpty(currentChannelId))
            return;

        view.HidePlaceModal();
        view.HideEventModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
    }
}
