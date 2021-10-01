using DCL;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    void SetFeaturedEvents(List<EventCardComponentModel> events);
    void SetTrendingEvents(List<EventCardComponentModel> events);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetGoingEvents(List<EventCardComponentModel> events);
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    internal IEventsSubSectionComponentView view;
    internal IEventsAPIController eventsAPIApiController;
    internal EventsSubSectionData mockedData;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI)
    {
        //mockedData = GameObject.Instantiate(Resources.Load<EventsSubSectionData>("MockedData/ExploreV2EventsSubSectionMockedData"));
        eventsAPIApiController = eventsAPI;

        this.view = view;
        this.view.OnReady += MakeFirstLoading;
    }

    public void SetFeaturedEvents(List<EventCardComponentModel> events) { view.SetFeaturedEvents(events); }

    public void SetTrendingEvents(List<EventCardComponentModel> events) { view.SetTrendingEvents(events); }

    public void SetUpcomingEvents(List<EventCardComponentModel> events) { view.SetUpcomingEvents(events); }

    public void SetGoingEvents(List<EventCardComponentModel> events) { view.SetGoingEvents(events); }

    public void Dispose() { view.OnReady -= MakeFirstLoading; }

    public void LoadFeaturedEvents() { SetFeaturedEvents(new List<EventCardComponentModel>()); }

    public void LoadTrendingEvents() { SetTrendingEvents(new List<EventCardComponentModel>()); }

    public void LoadUpcomingEvents()
    {
        SetUpcomingEvents(new List<EventCardComponentModel>());

        eventsAPIApiController.GetUpcomingEvents(
            (eventList) =>
            {
                List<EventCardComponentModel> eventsToAdd = new List<EventCardComponentModel>();
                foreach (EventFromAPIModel receivedEvent in eventList)
                {
                    EventCardComponentModel newEvent = CreateEventCardModel(receivedEvent);
                    eventsToAdd.Add(newEvent);
                    SetUpcomingEvents(eventsToAdd);
                }
            },
            (error) =>
            {
                Debug.LogError($"Error receiving events: {error}");
            });
    }

    public void LoadGoingEvents() { SetGoingEvents(new List<EventCardComponentModel>()); }

    internal void MakeFirstLoading()
    {
        LoadFeaturedEvents();
        LoadTrendingEvents();
        LoadUpcomingEvents();
        LoadGoingEvents();
    }

    internal EventCardComponentModel CreateEventCardModel(EventFromAPIModel eventFromAPI)
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