using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    internal const int SHOW_MORE_UPCOMING_ROWS_INCREMENT = 1;
    internal const string LIVE_TAG_TEXT = "LIVE";
    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal int currentUpcomingEventsShowed = 0;
    internal bool reloadEvents = false;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnInfoClicked += ShowEventDetailedInfo;
        this.view.OnJumpInClicked += JumpInToEvent;
        this.view.OnSubscribeEventClicked += SubscribeToEvent;
        this.view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        this.view.OnShowMoreUpcomingEventsClicked += ShowMoreUpcomingEvents;

        eventsAPIApiController = eventsAPI;
        OnEventsFromAPIUpdated += OnRequestedEventsUpdated;
    }

    internal void FirstLoading()
    {
        reloadEvents = true;
        RequestAllEvents();

        view.OnEventsSubSectionEnable += RequestAllEvents;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
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

        currentUpcomingEventsShowed = view.currentUpcomingEventsPerRow * INITIAL_NUMBER_OF_UPCOMING_ROWS;
        view.RestartScrollViewPosition();
        view.SetFeaturedEventsAsLoading(true);
        view.SetTrendingEventsAsLoading(true);
        view.SetUpcomingEventsAsLoading(true);
        view.SetShowMoreUpcomingEventsButtonActive(false);
        view.SetGoingEventsAsLoading(true);
        RequestAllEventsFromAPI();
        reloadEvents = false;
    }

    internal void RequestAllEventsFromAPI()
    {
        // First try with signed request
        eventsAPIApiController.GetAllEventsSigned(
            (eventList) =>
            {
                eventsFromAPI = eventList;
                OnEventsFromAPIUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.LogError($"Error receiving events from the API (with signed request): {error}");

                // If the signed request fails, tries with non-signed one (this is temporal)
                eventsAPIApiController.GetAllEvents(
                    (eventList) =>
                    {
                        eventsFromAPI = eventList;
                        OnEventsFromAPIUpdated?.Invoke();
                    },
                    (error) =>
                    {
                        Debug.LogError($"Error receiving events from the API (with non-signed request): {error}");
                    });
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
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            featuredEvents.Add(eventCardModel);
        }

        view.SetFeaturedEvents(featuredEvents);
        view.SetFeaturedEventsAsLoading(false);
    }

    public void LoadTrendingEvents()
    {
        List<EventCardComponentModel> trendingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.trending).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            trendingEvents.Add(eventCardModel);
        }

        view.SetTrendingEvents(trendingEvents);
        view.SetTrendingEventsAsLoading(false);
    }

    public void LoadUpcomingEvents()
    {
        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(currentUpcomingEventsShowed).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(eventCardModel);
        }

        view.SetUpcomingEvents(upcomingEvents);
        view.SetShowMoreUpcomingEventsButtonActive(currentUpcomingEventsShowed < eventsFromAPI.Count);
        view.SetUpcomingEventsAsLoading(false);
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
            EventCardComponentModel placeCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
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
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            goingEvents.Add(eventCardModel);
        }

        view.SetGoingEvents(goingEvents);
        view.SetGoingEventsAsLoading(false);
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
        DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
    }

    internal EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI)
    {
        EventCardComponentModel eventCardModel = new EventCardComponentModel();
        eventCardModel.eventId = eventFromAPI.id;
        eventCardModel.eventPictureUri = eventFromAPI.image;
        eventCardModel.isLive = eventFromAPI.live;
        eventCardModel.liveTagText = LIVE_TAG_TEXT;
        eventCardModel.eventDateText = FormatEventDate(eventFromAPI);
        eventCardModel.eventName = eventFromAPI.name;
        eventCardModel.eventDescription = eventFromAPI.description;
        eventCardModel.eventStartedIn = FormatEventStartDate(eventFromAPI);
        eventCardModel.eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI);
        eventCardModel.eventOrganizer = FormatEventOrganized(eventFromAPI);
        eventCardModel.eventPlace = FormatEventPlace(eventFromAPI);
        eventCardModel.subscribedUsers = eventFromAPI.total_attendees;
        eventCardModel.isSubscribed = false;
        eventCardModel.coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        eventCardModel.eventFromAPIInfo = eventFromAPI;

        // Card events
        return eventCardModel;
    }

    internal string FormatEventDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        return eventDateTime.ToString("MMMM d", new CultureInfo("en-US"));
    }

    internal string FormatEventStartDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        string formattedDate;
        if (eventFromAPI.live)
        {
            int daysAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalDays);
            int hoursAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalHours);

            if (daysAgo > 0)
                formattedDate = $"{daysAgo} days ago";
            else
                formattedDate = $"{hoursAgo} hr ago";
        }
        else
        {
            int daysToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalDays);
            int hoursToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalHours);

            if (daysToStart > 0)
                formattedDate = $"in {daysToStart} days";
            else
                formattedDate = $"in {hoursToStart} hours";
        }

        return formattedDate;
    }

    internal string FormatEventStartDateFromTo(EventFromAPIModel eventFromAPI)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        DateTime eventStartDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        DateTime eventEndDateTime = Convert.ToDateTime(eventFromAPI.finish_at).ToUniversalTime();
        string formattedDate = $"From {eventStartDateTime.ToString("dddd", cultureInfo)}, {eventStartDateTime.Day} {eventStartDateTime.ToString("MMM", cultureInfo)}" +
                               $" to {eventEndDateTime.ToString("dddd", cultureInfo)}, {eventEndDateTime.Day} {eventEndDateTime.ToString("MMM", cultureInfo)} UTC";

        return formattedDate;
    }

    internal string FormatEventOrganized(EventFromAPIModel eventFromAPI) { return $"Public, Organized by {eventFromAPI.user_name}"; }

    internal string FormatEventPlace(EventFromAPIModel eventFromAPI) { return string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name; }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel) { view.ShowEventModal(eventModel); }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        Vector2Int coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new string[] { "", "" } : eventFromAPI.realm.Split('-');
        string serverName = realmFromAPI[0];
        string layerName = realmFromAPI[1];

        if (string.IsNullOrEmpty(serverName))
            WebInterface.GoTo(coords.x, coords.y);
        else
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);

        view.HideEventModal();
        OnCloseExploreV2?.Invoke();
    }

    internal void SubscribeToEvent(string eventId)
    {
        eventsAPIApiController.RegisterAttendEvent(
            eventId,
            true,
            () =>
            {
                // Waiting for the new version of the Events API where we will be able to send a signed POST to register our user in an event.
            },
            (error) =>
            {
                Debug.LogError($"Error posting 'attend' message to the API: {error}");
            });
    }

    internal void UnsubscribeToEvent(string eventId)
    {
        eventsAPIApiController.RegisterAttendEvent(
            eventId,
            false,
            () =>
            {
                // Waiting for the new version of the Events API where we will be able to send a signed POST to unregister our user in an event.
            },
            (error) =>
            {
                Debug.LogError($"Error posting 'attend' message to the API: {error}");
            });
    }
}