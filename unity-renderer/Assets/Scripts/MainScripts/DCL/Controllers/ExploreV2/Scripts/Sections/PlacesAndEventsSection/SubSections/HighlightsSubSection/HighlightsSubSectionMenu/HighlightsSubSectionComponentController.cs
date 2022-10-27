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
    /// It will be triggered when the sub-section want to request to go to the Events sub-section.
    /// </summary>
    event Action OnGoToEventsSubSection;

    /// <summary>
    /// Request all places and events from the API.
    /// </summary>
    void RequestAllPlacesAndEvents();

    /// <summary>
    /// Load the trending places and events with the last requested ones.
    /// </summary>
    void LoadTrendingPlacesAndEvents();

    /// <summary>
    /// Load the featured places with the last requested ones.
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
    public event Action OnGoToEventsSubSection;

    internal const int DEFAULT_NUMBER_OF_TRENDING_PLACES = 10;
    internal const int DEFAULT_NUMBER_OF_FEATURED_PLACES = 9;
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
    internal float lastTimeAPIChecked = 0;
    private DataStore dataStore;
    
    public HighlightsSubSectionComponentController(
        IHighlightsSubSectionComponentView view,
        IPlacesAPIController placesAPI,
        IEventsAPIController eventsAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore)
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
        this.view.OnViewAllEventsClicked += GoToEventsSubSection;
        
        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        placesAPIApiController = placesAPI;
        eventsAPIApiController = eventsAPI;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
    }

    internal void FirstLoading()
    {
        reloadHighlights = true;
        lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        RequestAllPlacesAndEvents();

        view.OnHighlightsSubSectionEnable += RequestAllPlacesAndEvents;
        dataStore.exploreV2.isOpen.OnChange += OnExploreV2Open;
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

        if (Time.realtimeSinceStartup < lastTimeAPIChecked + PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API)
            return;

        view.SetTrendingPlacesAndEventsAsLoading(true);
        view.SetFeaturedPlacesAsLoading(true);
        view.SetLiveAsLoading(true);
        
        reloadHighlights = false;
        lastTimeAPIChecked = Time.realtimeSinceStartup;

        if (!dataStore.exploreV2.isInShowAnimationTransiton.Get())
            RequestAllPlacesAndEventsFromAPI();
        else
            dataStore.exploreV2.isInShowAnimationTransiton.OnChange += IsInShowAnimationTransitonChanged;
    }

    internal void IsInShowAnimationTransitonChanged(bool current, bool previous)
    {
        dataStore.exploreV2.isInShowAnimationTransiton.OnChange -= IsInShowAnimationTransitonChanged;
        RequestAllPlacesAndEventsFromAPI();
    }

    internal void RequestAllPlacesAndEventsFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            (placeList) =>
            {
                placesFromAPI = placeList;
                eventsAPIApiController.GetAllEvents(
                    (eventList) =>
                    {
                        eventsFromAPI = eventList;
                        OnRequestedPlacesAndEventsUpdated();
                    },
                    (error) =>
                    {
                        OnRequestedPlacesAndEventsUpdated();
                        Debug.LogError($"Error receiving events from the API: {error}");
                    });
            });
    }

    internal void OnRequestedPlacesAndEventsUpdated()
    {
        friendsTrackerController.RemoveAllHandlers();

        LoadTrendingPlacesAndEvents();
        LoadFeaturedPlaces();
        LoadLiveEvents();
    }

    public void LoadTrendingPlacesAndEvents()
    {
        // Places
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI
                                            .Take(DEFAULT_NUMBER_OF_TRENDING_PLACES)
                                            .ToList();

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        // Events
        List<EventCardComponentModel> events = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI
            .Where(e => e.highlighted)
            .Take(placesFiltered.Count)
            .ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            events.Add(eventCardModel);
        }

        view.SetTrendingPlacesAndEvents(places, events);
    }

    public void LoadFeaturedPlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered;
        if (placesFromAPI.Count >= DEFAULT_NUMBER_OF_TRENDING_PLACES)
        {
            int numberOfPlaces = placesFromAPI.Count >= (DEFAULT_NUMBER_OF_TRENDING_PLACES + DEFAULT_NUMBER_OF_FEATURED_PLACES)
                ? DEFAULT_NUMBER_OF_FEATURED_PLACES
                : placesFromAPI.Count - DEFAULT_NUMBER_OF_TRENDING_PLACES;

            placesFiltered = placesFromAPI
                             .GetRange(DEFAULT_NUMBER_OF_TRENDING_PLACES, numberOfPlaces)
                             .ToList();
        }
        else if (placesFromAPI.Count > 0)
        {
            placesFiltered = placesFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_PLACES).ToList();
        }
        else
        {
            placesFiltered = new List<HotSceneInfo>();
        }

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetFeaturedPlaces(places);
    }

    public void LoadLiveEvents()
    {
        List<EventCardComponentModel> events = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(x => x.live)
                                                              .Take(DEFAULT_NUMBER_OF_LIVE_EVENTS)
                                                              .ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            events.Add(eventCardModel);
        }

        view.SetLiveEvents(events);
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
        
        dataStore.exploreV2.isOpen.OnChange -= OnExploreV2Open;
        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        ExplorePlacesUtils.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.name, placeFromAPI.baseCoords);
    }

    internal void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) { friendsTrackerController.AddHandler(friendsHandler); }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Events);
    }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        ExploreEventsUtils.JumpInToEvent(eventFromAPI);
        view.HideEventModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
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
    }

    internal void GoToEventsSubSection() { OnGoToEventsSubSection?.Invoke(); }
    
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