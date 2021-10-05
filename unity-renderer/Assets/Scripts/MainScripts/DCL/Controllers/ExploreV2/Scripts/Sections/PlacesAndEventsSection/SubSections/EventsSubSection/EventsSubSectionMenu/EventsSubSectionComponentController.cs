using DCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public interface IEventsSubSectionComponentController : IDisposable
{
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
    internal event Action OnEventsFromAPIUpdated;

    internal const int INITIAL_NUMBER_OF_UPCOMING_EVENTS = 6;
    internal const int SHOW_MORE_UPCOMING_EVENTS_INCREMENT = 6;

    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal int currentUpcomingEventsShowed = INITIAL_NUMBER_OF_UPCOMING_EVENTS;
    internal bool reloadEvents = false;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
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

    private void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadEvents = true;
    }

    public void RequestAllEvents()
    {
        if (!reloadEvents)
            return;

        view.SetFeaturedEventsAsLoading(true);
        view.SetTrendingEventsAsLoading(true);
        view.SetUpcomingEventsAsLoading(true);
        view.SetGoingEventsAsLoading(true);

        RequestAllEventsFromAPI();

        reloadEvents = false;
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
        view.SetFeaturedEvents(new List<EventCardComponentModel>());

        List<EventCardComponentModel> featuredEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.highlighted).ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            featuredEvents.Add(eventCardModel);
        }

        view.SetFeaturedEventsAsLoading(false);
        view.SetFeaturedEvents(featuredEvents);
    }

    public void LoadTrendingEvents()
    {
        view.SetTrendingEvents(new List<EventCardComponentModel>());

        List<EventCardComponentModel> trendingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.trending).ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            trendingEvents.Add(eventCardModel);
        }

        view.SetTrendingEventsAsLoading(false);
        view.SetTrendingEvents(trendingEvents);
    }

    public void LoadUpcomingEvents()
    {
        view.SetUpcomingEvents(new List<EventCardComponentModel>());

        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(currentUpcomingEventsShowed).ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(eventCardModel);
        }

        view.SetUpcomingEventsAsLoading(false);
        view.SetUpcomingEvents(upcomingEvents);
        view.SetShowMoreUpcomingEventsButtonActive(eventsFromAPI.Count > currentUpcomingEventsShowed);
    }

    public void ShowMoreUpcomingEvents()
    {
        currentUpcomingEventsShowed += SHOW_MORE_UPCOMING_EVENTS_INCREMENT;
        LoadUpcomingEvents();
    }

    public void LoadGoingEvents()
    {
        view.SetGoingEventsAsLoading(false);
        view.SetGoingEvents(new List<EventCardComponentModel>());
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnShowMoreUpcomingEventsClicked -= ShowMoreUpcomingEvents;
        view.OnEventsSubSectionEnable -= RequestAllEvents;
        OnEventsFromAPIUpdated -= OnRequestedEventsUpdated;
        DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
    }

    internal EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI)
    {
        EventCardComponentModel eventCardModel = new EventCardComponentModel();

        // Card data
        eventCardModel.eventId = eventFromAPI.id;
        eventCardModel.eventPictureUri = eventFromAPI.image;
        eventCardModel.isLive = eventFromAPI.live;
        eventCardModel.liveTagText = "LIVE";
        eventCardModel.eventDateText = FormatEventDate(eventFromAPI);
        eventCardModel.eventName = eventFromAPI.name;
        eventCardModel.eventDescription = eventFromAPI.description;
        eventCardModel.eventStartedIn = FormatEventStartDate(eventFromAPI);
        eventCardModel.eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI);
        eventCardModel.eventOrganizer = FormatEventOrganized(eventFromAPI);
        eventCardModel.eventPlace = FormatEventPlace(eventFromAPI);
        eventCardModel.subscribedUsers = eventFromAPI.total_attendees;
        eventCardModel.isSubscribed = false;
        eventCardModel.jumpInConfiguration = GetJumpInConfigFromAPIEvent(eventFromAPI);

        // Card events
        ConfigureOnInfoActions(eventCardModel);
        ConfigureOnSubscribeActions(eventCardModel);
        ConfigureOnUnsubscribeActions(eventCardModel);

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

    internal JumpInConfig GetJumpInConfigFromAPIEvent(EventFromAPIModel eventFromAPI)
    {
        JumpInConfig result = new JumpInConfig();

        result.coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);

        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new string[] { "", "" } : eventFromAPI.realm.Split('-');
        result.serverName = realmFromAPI[0];
        result.layerName = realmFromAPI[1];

        return result;
    }

    internal void ConfigureOnInfoActions(EventCardComponentModel eventModel)
    {
        eventModel.onInfoClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        eventModel.onInfoClick.AddListener(() => view.ShowEventModal(eventModel));
    }

    internal void ConfigureOnSubscribeActions(EventCardComponentModel eventModel)
    {
        eventModel.onSubscribeClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        eventModel.onSubscribeClick.AddListener(() =>
        {
            eventsAPIApiController.RegisterAttendEvent(
                eventModel.eventId,
                true,
                () =>
                {
                    Debug.Log("I WILL ATTEND!!");
                },
                (error) =>
                {
                    Debug.LogError($"Error posting 'attend' message to the API: {error}");
                });
        });
    }

    internal void ConfigureOnUnsubscribeActions(EventCardComponentModel eventModel)
    {
        eventModel.onUnsubscribeClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        eventModel.onUnsubscribeClick.AddListener(() =>
        {
            eventsAPIApiController.RegisterAttendEvent(
                eventModel.eventId,
                false,
                () =>
                {
                    Debug.Log("I WILL NOT ATTEND!!");
                },
                (error) =>
                {
                    Debug.LogError($"Error posting 'attend' message to the API: {error}");
                });
        });
    }
}