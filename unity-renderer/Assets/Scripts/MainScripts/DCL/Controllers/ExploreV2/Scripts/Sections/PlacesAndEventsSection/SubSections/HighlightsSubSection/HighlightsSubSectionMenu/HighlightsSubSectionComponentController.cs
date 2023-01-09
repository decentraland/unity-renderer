using DCL;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public class HighlightsSubSectionComponentController : IHighlightsSubSectionComponentController
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

    private readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<HotSceneInfo> placesFromAPI = new ();
    internal List<EventFromAPIModel> eventsFromAPI = new ();

    public HighlightsSubSectionComponentController(IHighlightsSubSectionComponentView view, IPlacesAPIController placesAPI, IEventsAPIController eventsAPI, IFriendsController friendsController, IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, dataStore.exploreV2, RequestAllPlacesAndEventsFromAPI);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnPlaceInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnPlaceJumpInClicked += JumpInToPlace;

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

        view.OnEventSubscribeEventClicked -= SubscribeToEvent;
        view.OnEventUnsubscribeEventClicked -= UnsubscribeToEvent;

        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;

        view.OnViewAllEventsClicked -= GoToEventsSubSection;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnHighlightsSubSectionEnable += RequestAllPlacesAndEvents;
        cardsReloader.Initialize();
    }

    private void RequestAllPlacesAndEvents()
    {
        if (cardsReloader.CanReload())
            cardsReloader.RequestAll();
    }

    internal void RequestAllPlacesAndEventsFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            OnCompleted: placeList =>
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
            });
    }

    internal void OnRequestedPlacesAndEventsUpdated()
    {
        friendsTrackerController.RemoveAllHandlers();

        List<PlaceCardComponentModel> trendingPlaces = PlacesAndEventsCardsFactory.CreatePlacesCards(FilterTrendingPlaces());
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(FilterTrendingEvents(trendingPlaces.Count));
        view.SetTrendingPlacesAndEvents(trendingPlaces, trendingEvents);

        view.SetFeaturedPlaces(PlacesAndEventsCardsFactory.CreatePlacesCards(FilterFeaturedPlaces()));
        view.SetLiveEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterLiveEvents()));
    }

    internal List<HotSceneInfo> FilterTrendingPlaces() => placesFromAPI.Take(DEFAULT_NUMBER_OF_TRENDING_PLACES).ToList();
    internal List<EventFromAPIModel> FilterLiveEvents() => eventsFromAPI.Where(x => x.live).Take(DEFAULT_NUMBER_OF_LIVE_EVENTS).ToList();
    internal List<EventFromAPIModel> FilterTrendingEvents(int amount) => eventsFromAPI.Where(e => e.highlighted).Take(amount).ToList();
    internal List<HotSceneInfo> FilterFeaturedPlaces()
    {
        List<HotSceneInfo> featuredPlaces;

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
            featuredPlaces = new List<HotSceneInfo>();

        return featuredPlaces;
    }

    private void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) =>
        friendsTrackerController.AddHandler(friendsHandler);

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Events);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        PlacesCardsConfigurator.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.name, placeFromAPI.baseCoords);
    }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        EventsCardsConfigurator.JumpInToEvent(eventFromAPI);
        view.HideEventModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]));
    }

    private static void SubscribeToEvent(string eventId) =>
        EventsSubSectionComponentController.SubscribeToEvent(eventId);

    private static void UnsubscribeToEvent(string eventId) =>
        EventsSubSectionComponentController.UnsubscribeToEvent(eventId);

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
