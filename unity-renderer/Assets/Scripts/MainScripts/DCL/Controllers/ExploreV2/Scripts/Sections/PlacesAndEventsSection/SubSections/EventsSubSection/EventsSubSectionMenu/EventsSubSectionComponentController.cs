using DCL;
using DCL.Interface;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEventsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all events from the API.
    /// </summary>
    void RequestAllEvents();

    /// <summary>
    /// Load the featured events with the last requested ones.
    /// </summary>
    void LoadFeaturedEvents();

    /// <summary>
    /// Load the trending events with the last requested ones.
    /// </summary>
    void LoadTrendingEvents();

    /// <summary>
    /// Load the upcoming events with the last requested ones.
    /// </summary>
    void LoadUpcomingEvents();

    /// <summary>
    /// Increment the number of upcoming events loaded.
    /// </summary>
    void ShowMoreUpcomingEvents();

    /// <summary>
    /// Load the going events with the last requested ones.
    /// </summary>
    void LoadGoingEvents();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    internal event Action OnEventsFromAPIUpdated;

    internal const int DEFAULT_NUMBER_OF_FEATURED_EVENTS = 3;
    internal const int INITIAL_NUMBER_OF_UPCOMING_ROWS = 1;
    internal const int SHOW_MORE_UPCOMING_ROWS_INCREMENT = 2;
    internal const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";
    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal int currentUpcomingEventsShowed = 0;
    internal bool reloadEvents = false;
    internal IExploreV2Analytics exploreV2Analytics;
    internal float lastTimeAPIChecked = 0;
    private DataStore dataStore;

    public EventsSubSectionComponentController(
        IEventsSubSectionComponentView view,
        IEventsAPIController eventsAPI,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnInfoClicked += ShowEventDetailedInfo;
        this.view.OnJumpInClicked += JumpInToEvent;
        this.view.OnSubscribeEventClicked += SubscribeToEvent;
        this.view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        this.view.OnShowMoreUpcomingEventsClicked += ShowMoreUpcomingEvents;
        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        eventsAPIApiController = eventsAPI;
        OnEventsFromAPIUpdated += OnRequestedEventsUpdated;

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
    }

    internal void FirstLoading()
    {
        reloadEvents = true;
        lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        RequestAllEvents();

        view.OnEventsSubSectionEnable += RequestAllEvents;
        dataStore.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    internal void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadEvents = true;
    }

    public void RequestAllEvents()
    {
        if (!reloadEvents)
            return;

        view.RestartScrollViewPosition();

        if (Time.realtimeSinceStartup < lastTimeAPIChecked + PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API)
            return;

        currentUpcomingEventsShowed = view.currentUpcomingEventsPerRow * INITIAL_NUMBER_OF_UPCOMING_ROWS;
        
        view.SetAllEventGroupsAsLoading();
        view.SetShowMoreUpcomingEventsButtonActive(false);

        reloadEvents = false;
        lastTimeAPIChecked = Time.realtimeSinceStartup;

        if (!dataStore.exploreV2.isInShowAnimationTransiton.Get())
            RequestAllEventsFromAPI();
        else
            dataStore.exploreV2.isInShowAnimationTransiton.OnChange += IsInShowAnimationTransitonChanged;
    }

    internal void IsInShowAnimationTransitonChanged(bool current, bool previous)
    {
        dataStore.exploreV2.isInShowAnimationTransiton.OnChange -= IsInShowAnimationTransitonChanged;
        RequestAllEventsFromAPI();
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

    internal void OnRequestedEventsUpdated()
    {
        LoadFeaturedEvents();
        LoadTrendingEvents();
        LoadUpcomingEvents();
        LoadGoingEvents();
    }

    public void LoadFeaturedEvents()
    {
        List<EventCardComponentModel> featuredEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.highlighted).ToList();

        if (eventsFiltered.Count == 0)
            eventsFiltered = eventsFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_EVENTS).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            featuredEvents.Add(eventCardModel);
        }

        view.SetFeaturedEvents(featuredEvents);
    }

    public void LoadTrendingEvents()
    {
        List<EventCardComponentModel> trendingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.trending).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            trendingEvents.Add(eventCardModel);
        }

        view.SetTrendingEvents(trendingEvents);
    }

    public void LoadUpcomingEvents()
    {
        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(currentUpcomingEventsShowed).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(eventCardModel);
        }

        view.SetUpcomingEvents(upcomingEvents);
        view.SetShowMoreUpcomingEventsButtonActive(currentUpcomingEventsShowed < eventsFromAPI.Count);
    }

    public void ShowMoreUpcomingEvents()
    {
        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = new List<EventFromAPIModel>();
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)currentUpcomingEventsShowed / view.currentUpcomingEventsPerRow) * view.currentUpcomingEventsPerRow) - currentUpcomingEventsShowed;
        int numberOfItemsToAdd = view.currentUpcomingEventsPerRow * SHOW_MORE_UPCOMING_ROWS_INCREMENT + numberOfExtraItemsToAdd;

        if (currentUpcomingEventsShowed + numberOfItemsToAdd <= eventsFromAPI.Count)
            eventsFiltered = eventsFromAPI.GetRange(currentUpcomingEventsShowed, numberOfItemsToAdd);
        else
            eventsFiltered = eventsFromAPI.GetRange(currentUpcomingEventsShowed, eventsFromAPI.Count - currentUpcomingEventsShowed);

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel placeCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(placeCardModel);
        }

        view.AddUpcomingEvents(upcomingEvents);

        currentUpcomingEventsShowed += numberOfItemsToAdd;
        if (currentUpcomingEventsShowed > eventsFromAPI.Count)
            currentUpcomingEventsShowed = eventsFromAPI.Count;

        view.SetShowMoreUpcomingEventsButtonActive(currentUpcomingEventsShowed < eventsFromAPI.Count);
    }

    public void LoadGoingEvents()
    {
        List<EventCardComponentModel> goingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.attending).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            goingEvents.Add(eventCardModel);
        }

        view.SetGoingEvents(goingEvents);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowEventDetailedInfo;
        view.OnJumpInClicked -= JumpInToEvent;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnShowMoreUpcomingEventsClicked -= ShowMoreUpcomingEvents;
        view.OnEventsSubSectionEnable -= RequestAllEvents;
        OnEventsFromAPIUpdated -= OnRequestedEventsUpdated;
        dataStore.exploreV2.isOpen.OnChange -= OnExploreV2Open;
        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
    }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
    }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        ExploreEventsUtils.JumpInToEvent(eventFromAPI);
        view.HideEventModal();
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

    private void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (!string.IsNullOrEmpty(currentChannelId))
            return;

        view.HideEventModal();
        OnCloseExploreV2?.Invoke();
    }
}