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

    private readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<EventFromAPIModel> eventsFromAPI = new ();
    private int availableUISlots;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view, IEventsAPIController eventsAPI, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, dataStore.exploreV2, RequestAllEventsFromAPI);

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

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnEventsSubSectionEnable += RequestAllEvents;
        cardsReloader.Initialize();
    }

    public void RequestAllEvents()
    {
        if (cardsReloader.CanReload())
        {
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_UPCOMING_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsReloader.RequestAll();
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

        view.SetFeaturedEvents(LoadEvents(FilterFeaturedEvents()));
        view.SetTrendingEvents(LoadEvents(FilterTrendingEvents()));
        view.SetUpcomingEvents(LoadEvents(FilterUpcomingEvents()));
        view.SetGoingEvents(LoadEvents(FilterGoingEvents()));

        view.SetShowMoreUpcomingEventsButtonActive(availableUISlots < eventsFromAPI.Count);
    }

    public static List<EventCardComponentModel> LoadEvents(List<EventFromAPIModel> filteredEvents) =>
        CardsLoader.CreateModelsListFromAPI(filteredEvents, ExploreEventsUtils.CreateEventCardModelFromAPIEvent);

    public List<EventFromAPIModel> FilterFeaturedEvents()
    {
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.highlighted).ToList();

        if (eventsFiltered.Count == 0)
            eventsFiltered = eventsFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_EVENTS).ToList();

        return eventsFiltered;
    }
    public List<EventFromAPIModel> FilterTrendingEvents() => eventsFromAPI.Where(e => e.trending).ToList();
    private List<EventFromAPIModel> FilterUpcomingEvents() => eventsFromAPI.Take(availableUISlots).ToList();
    public List<EventFromAPIModel> FilterGoingEvents() => eventsFromAPI.Where(e => e.attending).ToList();

    public void ShowMoreUpcomingEvents()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentUpcomingEventsPerRow) * view.currentUpcomingEventsPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentUpcomingEventsPerRow * SHOW_MORE_UPCOMING_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<EventFromAPIModel> eventsFiltered = availableUISlots + numberOfItemsToAdd <= eventsFromAPI.Count
            ? eventsFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : eventsFromAPI.GetRange(availableUISlots, eventsFromAPI.Count - availableUISlots);

        view.AddUpcomingEvents(LoadEvents(eventsFiltered));

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
