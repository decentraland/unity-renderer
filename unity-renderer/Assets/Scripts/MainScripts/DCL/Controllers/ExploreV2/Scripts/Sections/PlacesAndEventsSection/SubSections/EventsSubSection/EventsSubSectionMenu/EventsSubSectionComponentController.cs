using DCL;
using DCL.Interface;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    public event Action OnCloseExploreV2;

    private const int DEFAULT_NUMBER_OF_FEATURED_EVENTS = 3;
    private const int INITIAL_NUMBER_OF_UPCOMING_ROWS = 1;
    private const int SHOW_MORE_UPCOMING_ROWS_INCREMENT = 2;
    private const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";

    internal readonly IEventsSubSectionComponentView view;
    internal readonly IEventsAPIController eventsAPIApiController;
    private readonly DataStore dataStore;
    private readonly IExploreV2Analytics exploreV2Analytics;

    private readonly PlaceAndEventsCardsRequestHandler cardsRequestHandler;

    internal List<EventFromAPIModel> eventsFromAPI = new ();
    private int availableUISlots;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsRequestHandler = new PlaceAndEventsCardsRequestHandler(view, dataStore.exploreV2, RequestAllEventsFromAPI);

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

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
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

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsRequestHandler.Dispose();
    }

    private void FirstLoading()
    {
        view.OnEventsSubSectionEnable += RequestAllEvents;
        cardsRequestHandler.Initialize();
    }

    public void RequestAllEvents()
    {
        if (cardsRequestHandler.CanReload())
        {
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_UPCOMING_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsRequestHandler.RequestAll();
        }
    }

    internal void RequestAllEventsFromAPI()
    {
        eventsAPIApiController.GetAllEvents(
            OnSuccess: OnRequestedEventsUpdated,
            OnFail: error => { Debug.LogError($"Error receiving events from the API: {error}"); });
    }

    private void OnRequestedEventsUpdated(List<EventFromAPIModel> eventList)
    {
        eventsFromAPI = eventList;

        LoadFeaturedEvents();
        LoadTrendingEvents();
        LoadUpcomingEvents();
        LoadGoingEvents();

        view.SetShowMoreUpcomingEventsButtonActive(availableUISlots < eventsFromAPI.Count);
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
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(availableUISlots).ToList();

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(eventCardModel);
        }

        view.SetUpcomingEvents(upcomingEvents);
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

    public void ShowMoreUpcomingEvents()
    {
        List<EventCardComponentModel> upcomingEvents = new List<EventCardComponentModel>();

        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentUpcomingEventsPerRow) * view.currentUpcomingEventsPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentUpcomingEventsPerRow * SHOW_MORE_UPCOMING_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<EventFromAPIModel> eventsFiltered = availableUISlots + numberOfItemsToAdd <= eventsFromAPI.Count
            ? eventsFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : eventsFromAPI.GetRange(availableUISlots, eventsFromAPI.Count - availableUISlots);

        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel placeCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(receivedEvent);
            upcomingEvents.Add(placeCardModel);
        }

        view.AddUpcomingEvents(upcomingEvents);

        availableUISlots += numberOfItemsToAdd;

        if (availableUISlots > eventsFromAPI.Count)
            availableUISlots = eventsFromAPI.Count;

        view.SetShowMoreUpcomingEventsButtonActive(availableUISlots < eventsFromAPI.Count);
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

    private static void SubscribeToEvent(string eventId)
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

    private static void UnsubscribeToEvent(string eventId)
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
