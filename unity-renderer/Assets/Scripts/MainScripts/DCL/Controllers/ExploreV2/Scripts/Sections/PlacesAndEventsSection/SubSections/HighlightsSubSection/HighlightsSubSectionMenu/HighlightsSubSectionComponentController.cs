using DCL;
using DCL.Interface;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public interface IHighlightsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// It will be triggered when any action is executed inside the highlights sub-section.
    /// </summary>
    event Action OnAnyActionExecuted;

    /// <summary>
    /// Request all places and events from the API.
    /// </summary>
    void RequestAllPlacesAndEvents();

    /// <summary>
    /// Load the promoted places with the last requested ones.
    /// </summary>
    void LoadPromotedPlaces();

    /// <summary>
    /// Load the promoted places with the last requested ones.
    /// </summary>
    void LoadFeaturedPlaces();

    /// <summary>
    /// Load the live events with the last requested ones.
    /// </summary>
    void LoadLiveEvents();
}

public class HighlightsSubSectionComponentController : IHighlightsSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    public event Action OnAnyActionExecuted;
    internal event Action OnPlacesFromAPIUpdated;
    internal event Action OnEventsFromAPIUpdated;

    internal const int DEFAULT_NUMBER_OF_PROMOTED_PLACES = 10;
    internal const int DEFAULT_NUMBER_OF_FEATURED_PLACES = 6;
    internal const int DEFAULT_NUMBER_OF_LIVE_EVENTS = 3;
    internal const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";

    internal IHighlightsSubSectionComponentView view;
    internal IPlacesAPIController placesAPIApiController;
    internal IEventsAPIController eventsAPIApiController;
    internal FriendTrackerController friendsTrackerController;
    internal List<HotSceneInfo> placesFromAPI = new List<HotSceneInfo>();
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal bool reloadHighlights = false;
    internal IExploreV2Analytics exploreV2Analytics;

    public HighlightsSubSectionComponentController(
        IHighlightsSubSectionComponentView view,
        IPlacesAPIController placesAPI,
        IEventsAPIController eventsAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnPlaceInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnEventInfoClicked += ShowEventDetailedInfo;
        this.view.OnPlaceJumpInClicked += JumpInToPlace;
        this.view.OnEventJumpInClicked += JumpInToEvent;
        this.view.OnEventSubscribeEventClicked += SubscribeToEvent;
        this.view.OnEventUnsubscribeEventClicked += UnsubscribeToEvent;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        placesAPIApiController = placesAPI;
        eventsAPIApiController = eventsAPI;
        OnPlacesFromAPIUpdated += OnRequestedPlacesUpdated;
        OnEventsFromAPIUpdated += OnRequestedEventsUpdated;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;
    }

    internal void FirstLoading()
    {
        reloadHighlights = true;
        RequestAllPlacesAndEvents();

        view.OnHighlightsSubSectionEnable += RequestAllPlacesAndEvents;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    internal void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadHighlights = true;
    }

    public void RequestAllPlacesAndEvents()
    {
        if (!reloadHighlights)
            return;

        view.RestartScrollViewPosition();
        view.SetPromotedPlacessAsLoading(true);
        view.SetFeaturedPlacesAsLoading(true);
        view.SetLiveAsLoading(true);
        RequestAllPlacesFromAPI();
        RequestAllEventsFromAPI();
        reloadHighlights = false;
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

    internal void RequestAllEventsFromAPI()
    {
        eventsAPIApiController.GetAllEvents(
            (eventList) =>
            {
                eventsFromAPI = eventList;
                OnEventsFromAPIUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.LogError($"Error receiving events from the API: {error}");
            });
    }

    internal void OnRequestedPlacesUpdated()
    {
        friendsTrackerController.RemoveAllHandlers();

        LoadPromotedPlaces();
        LoadFeaturedPlaces();
    }

    internal void OnRequestedEventsUpdated() { LoadLiveEvents(); }

    public void LoadPromotedPlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI
                                            .Where(x => !string.IsNullOrEmpty(x.thumbnail))
                                            .Take(DEFAULT_NUMBER_OF_PROMOTED_PLACES)
                                            .ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesHelpers.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetPromotedPlaces(places);
        view.SetPromotedPlacessAsLoading(false);
    }

    public void LoadFeaturedPlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI
                                            .Take(DEFAULT_NUMBER_OF_FEATURED_PLACES)
                                            .ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesHelpers.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetFeaturedPlaces(places);
        view.SetFeaturedPlacesAsLoading(false);
    }

    public void LoadLiveEvents()
    {
        List<EventCardComponentModel> events = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(x => x.live)
                                                              .Take(DEFAULT_NUMBER_OF_LIVE_EVENTS)
                                                              .ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsHelpers.CreateEventCardModelFromAPIEvent(receivedEvent);
            events.Add(eventCardModel);
        }

        view.SetLiveEvents(events);
        view.SetLiveAsLoading(false);
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
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        OnAnyActionExecuted?.Invoke();
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        ExplorePlacesHelpers.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();
        OnCloseExploreV2?.Invoke();
        OnAnyActionExecuted?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.name, placeFromAPI.baseCoords);
    }

    internal void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) { friendsTrackerController.AddHandler(friendsHandler); }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        OnAnyActionExecuted?.Invoke();
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
    }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        ExploreEventsHelpers.JumpInToEvent(eventFromAPI);
        view.HideEventModal();
        OnCloseExploreV2?.Invoke();
        OnAnyActionExecuted?.Invoke();
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]));
    }

    internal void SubscribeToEvent(string eventId)
    {
        // TODO (Santi): Remove when the RegisterAttendEvent POST is available.
        WebInterface.OpenURL(string.Format(EVENT_DETAIL_URL, eventId));

        // TODO (Santi): Waiting for the new version of the Events API where we will be able to send a signed POST to register our user in an event.
        //eventsAPIApiController.RegisterAttendEvent(
        //    eventId,
        //    true,
        //    () =>
        //    {
        //        // ...
        //    },
        //    (error) =>
        //    {
        //        Debug.LogError($"Error posting 'attend' message to the API: {error}");
        //    });

        OnAnyActionExecuted?.Invoke();
    }

    internal void UnsubscribeToEvent(string eventId)
    {
        // TODO (Santi): Remove when the RegisterAttendEvent POST is available.
        WebInterface.OpenURL(string.Format(EVENT_DETAIL_URL, eventId));

        // TODO (Santi): Waiting for the new version of the Events API where we will be able to send a signed POST to unregister our user in an event.
        //eventsAPIApiController.RegisterAttendEvent(
        //    eventId,
        //    false,
        //    () =>
        //    {
        //        // ...
        //    },
        //    (error) =>
        //    {
        //        Debug.LogError($"Error posting 'attend' message to the API: {error}");
        //    });

        OnAnyActionExecuted?.Invoke();
    }
}