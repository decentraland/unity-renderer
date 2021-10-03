using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    void RequestAllEventsFromAPI();
    void LoadFeaturedEvents();
    void LoadTrendingEvents();
    void LoadUpcomingEvents();
    void ShowMoreUpcomingEvents();
    void LoadGoingEvents();
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    internal event Action OnEventsFromAPIUpdated;

    internal const int INITIAL_NUMBER_OF_UPCOMING_EVENTS = 6;
    internal const int SHOW_MORE_UPCOMING_EVENTS_INCREMENT = 3;

    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal int currentUpcomingEventsShowed = INITIAL_NUMBER_OF_UPCOMING_EVENTS;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnShowMoreUpcomingEventsClicked += ShowMoreUpcomingEvents;

        eventsAPIApiController = eventsAPI;
        OnEventsFromAPIUpdated += UpdateEvents;
    }

    internal void FirstLoading()
    {
        view.SetFeaturedEvents(new List<EventCardComponentModel>());
        view.SetFeaturedEventsAsLoading(true);

        view.SetTrendingEvents(new List<EventCardComponentModel>());
        view.SetTrendingEventsAsLoading(true);

        view.SetUpcomingEvents(new List<EventCardComponentModel>());
        view.SetUpcomingEventsAsLoading(true);

        view.SetGoingEvents(new List<EventCardComponentModel>());
        view.SetGoingEventsAsLoading(true);

        RequestAllEventsFromAPI();
    }

    public void RequestAllEventsFromAPI()
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

    internal void UpdateEvents()
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
        OnEventsFromAPIUpdated -= UpdateEvents;
    }

    internal EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI)
    {
        EventCardComponentModel eventCardModel = new EventCardComponentModel();

        // Card data
        eventCardModel.eventId = eventFromAPI.id;
        eventCardModel.eventPictureSprite = null;
        eventCardModel.eventPictureUri = eventFromAPI.image;
        eventCardModel.isLive = eventFromAPI.live;
        eventCardModel.liveTagText = "LIVE";
        eventCardModel.eventDateText = eventFromAPI.start_at;
        eventCardModel.eventName = eventFromAPI.name;
        eventCardModel.eventDescription = eventFromAPI.description;
        eventCardModel.eventStartedIn = eventFromAPI.start_at;
        eventCardModel.eventOrganizer = eventFromAPI.user_name;
        eventCardModel.eventPlace = eventFromAPI.scene_name;
        eventCardModel.subscribedUsers = eventFromAPI.total_attendees;
        eventCardModel.isSubscribed = false;
        eventCardModel.jumpInConfiguration = GetJumpInConfigFromAPIEvent(eventFromAPI);

        // Card events
        ConfigureOnInfoActions(eventCardModel);
        ConfigureOnSubscribeActions(eventCardModel);
        ConfigureOnUnsubscribeActions(eventCardModel);

        return eventCardModel;
    }

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