using DCL;
using DCL.Interface;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Environment = DCL.Environment;

public class EventsSubSectionComponentController : IEventsSubSectionComponentController, IPlacesAndEventsAPIRequester
{
    public event Action OnCloseExploreV2;

    private const int DEFAULT_NUMBER_OF_FEATURED_EVENTS = 3;
    internal const int INITIAL_NUMBER_OF_UPCOMING_ROWS = 1;
    private const int SHOW_MORE_UPCOMING_ROWS_INCREMENT = 2;
    private const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";

    internal readonly IEventsSubSectionComponentView view;
    internal readonly IEventsAPIController eventsAPIApiController;
    private readonly DataStore dataStore;
    private readonly IExploreV2Analytics exploreV2Analytics;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<EventFromAPIModel> eventsFromAPI = new ();
    internal int availableUISlots;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowEventDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToEvent;

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
        view.OnJumpInClicked -= OnJumpInToEvent;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnShowMoreUpcomingEventsClicked -= ShowMoreUpcomingEvents;
        view.OnEventsSubSectionEnable -= RequestAllEvents;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnEventsSubSectionEnable += RequestAllEvents;
        cardsReloader.Initialize();
    }

    internal void RequestAllEvents()
    {
        if (cardsReloader.CanReload())
        {
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_UPCOMING_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsReloader.RequestAll();
        }
    }

    public void RequestAllFromAPI()
    {
        eventsAPIApiController.GetAllEvents(
            OnSuccess: OnRequestedEventsUpdated,
            OnFail: error => { Debug.LogError($"Error receiving events from the API: {error}"); });
    }

    internal void OnRequestedEventsUpdated(List<EventFromAPIModel> eventList)
    {
        eventsFromAPI = eventList;

        view.SetFeaturedEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterFeaturedEvents()));
        view.SetTrendingEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterTrendingEvents()));
        view.SetUpcomingEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterUpcomingEvents()));
        view.SetGoingEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterGoingEvents()));

        view.SetShowMoreUpcomingEventsButtonActive(availableUISlots < eventsFromAPI.Count);
    }

    internal List<EventFromAPIModel> FilterFeaturedEvents()
    {
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.highlighted).ToList();

        if (eventsFiltered.Count == 0)
            eventsFiltered = eventsFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_EVENTS).ToList();

        return eventsFiltered;
    }
    internal List<EventFromAPIModel> FilterTrendingEvents() => eventsFromAPI.Where(e => e.trending).ToList();
    internal List<EventFromAPIModel> FilterUpcomingEvents() => eventsFromAPI.Take(availableUISlots).ToList();
    internal List<EventFromAPIModel> FilterGoingEvents() => eventsFromAPI.Where(e => e.attending).ToList();

    public void ShowMoreUpcomingEvents()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentUpcomingEventsPerRow) * view.currentUpcomingEventsPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentUpcomingEventsPerRow * SHOW_MORE_UPCOMING_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<EventFromAPIModel> eventsFiltered = availableUISlots + numberOfItemsToAdd <= eventsFromAPI.Count
            ? eventsFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : eventsFromAPI.GetRange(availableUISlots, eventsFromAPI.Count - availableUISlots);

        view.AddUpcomingEvents(PlacesAndEventsCardsFactory.CreateEventsCards(eventsFiltered));

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

    internal void OnJumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        JumpInToEvent(eventFromAPI);
        view.HideEventModal();

        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]));
    }

    public static void SubscribeToEvent(string eventId)
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

    public static void UnsubscribeToEvent(string eventId)
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

    /// <summary>
    /// Makes a jump in to the event defined by the given place data from API.
    /// </summary>
    /// <param name="eventFromAPI">Event data from API.</param>
    public static void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        Vector2Int coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new[] { "", "" } : eventFromAPI.realm.Split('-');
        string serverName = realmFromAPI[0];
        string layerName = realmFromAPI[1];

        if (string.IsNullOrEmpty(serverName))
            Environment.i.world.teleportController.Teleport(coords.x, coords.y);
        else
            Environment.i.world.teleportController.JumpIn(coords.x, coords.y, serverName, layerName);
    }
}
