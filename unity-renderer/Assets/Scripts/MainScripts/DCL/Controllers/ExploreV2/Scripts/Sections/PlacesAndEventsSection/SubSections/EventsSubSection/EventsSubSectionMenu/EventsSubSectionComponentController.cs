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
    void LoadGoingEvents();
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();

    internal event Action OnEventsFromAPIUpdated;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;

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
            EventCardComponentModel eventCardModel = ParseEventFromAPI(receivedEvent);
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
            EventCardComponentModel eventCardModel = ParseEventFromAPI(receivedEvent);
            trendingEvents.Add(eventCardModel);
        }

        view.SetTrendingEventsAsLoading(false);
        view.SetTrendingEvents(trendingEvents);
    }

    public void LoadUpcomingEvents()
    {
        view.SetUpcomingEvents(new List<EventCardComponentModel>());

        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(3).ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ParseEventFromAPI(receivedEvent);
            upcomingEvents.Add(eventCardModel);
        }

        view.SetUpcomingEventsAsLoading(false);
        view.SetUpcomingEvents(upcomingEvents);
    }

    public void LoadGoingEvents()
    {
        view.SetGoingEventsAsLoading(false);
        view.SetGoingEvents(new List<EventCardComponentModel>());
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        OnEventsFromAPIUpdated -= UpdateEvents;
    }

    internal EventCardComponentModel ParseEventFromAPI(EventFromAPIModel eventFromAPI)
    {
        EventCardComponentModel result = new EventCardComponentModel();

        result.eventId = eventFromAPI.id;
        result.eventPicture = null;
        result.isLive = eventFromAPI.live;
        result.liveTagText = "LIVE";
        result.eventDateText = eventFromAPI.start_at;
        result.eventName = eventFromAPI.name;
        result.eventDescription = eventFromAPI.description;
        result.eventStartedIn = eventFromAPI.start_at;
        result.eventOrganizer = eventFromAPI.user;
        result.eventPlace = eventFromAPI.scene_name;
        result.subscribedUsers = eventFromAPI.total_attendees;
        result.isSubscribed = false;
        result.jumpInConfiguration = new JumpInConfig
        {
            coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]),
            serverName = string.IsNullOrEmpty(eventFromAPI.realm) ? "" : eventFromAPI.realm.Split('-')[0],
            layerName = ""
        };

        return result;
    }

    //// Temporal until connect with the events API
    //private void SetEventsMockedData()
    //{
    //    foreach (var eventCardModel in mockedData.featureEvents)
    //    {
    //        ConfigureEventCardButtons(eventCardModel);
    //    }
    //    SetFeatureEvents(mockedData.featureEvents);

    //    foreach (var eventCardModel in mockedData.trendingEvents)
    //    {
    //        ConfigureEventCardButtons(eventCardModel);
    //    }
    //    SetTrendingEvents(mockedData.trendingEvents);

    //    foreach (var eventCardModel in mockedData.upcomingEvents)
    //    {
    //        ConfigureEventCardButtons(eventCardModel);
    //    }
    //    SetUpcomingEvents(mockedData.upcomingEvents);

    //    foreach (var eventCardModel in mockedData.goingEvents)
    //    {
    //        ConfigureEventCardButtons(eventCardModel);
    //    }
    //    SetGoingEvents(mockedData.goingEvents);
    //}

    //// Temporal until connect with the events API
    //private void ConfigureEventCardButtons(EventCardComponentModel eventCardModel)
    //{
    //    eventCardModel.onInfoClick.AddListener(() => view.ShowEventModal(eventCardModel));
    //    eventCardModel.onSubscribeClick.AddListener(() => Debug.Log("SUBSCRIBED!"));
    //    eventCardModel.onUnsubscribeClick.AddListener(() => Debug.Log("UNSUBSCRIBED!"));
    //    eventCardModel.onJumpInClick.AddListener(() => DataStore.i.exploreV2.isOpen.Set(false));
    //}
}