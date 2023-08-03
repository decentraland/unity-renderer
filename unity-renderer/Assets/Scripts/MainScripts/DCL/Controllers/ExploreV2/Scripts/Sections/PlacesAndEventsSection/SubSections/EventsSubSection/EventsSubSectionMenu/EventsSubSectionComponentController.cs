using DCL;
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
    internal const int INITIAL_NUMBER_OF_ROWS = 4;
    private const int SHOW_MORE_ROWS_INCREMENT = 2;

    internal readonly IEventsSubSectionComponentView view;
    internal readonly IEventsAPIController eventsAPIApiController;
    private readonly DataStore dataStore;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IUserProfileBridge userProfileBridge;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<EventFromAPIModel> eventsFromAPI = new ();
    internal int availableUISlots;

    public EventsSubSectionComponentController(
        IEventsSubSectionComponentView view,
        IEventsAPIController eventsAPI,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore,
        IUserProfileBridge userProfileBridge)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowEventDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToEvent;

        this.view.OnSubscribeEventClicked += SubscribeToEvent;
        this.view.OnUnsubscribeEventClicked += UnsubscribeToEvent;

        this.view.OnShowMoreEventsClicked += ShowMoreEvents;
        this.view.OnConnectWallet += ConnectWallet;

        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        eventsAPIApiController = eventsAPI;

        this.exploreV2Analytics = exploreV2Analytics;
        this.userProfileBridge = userProfileBridge;

        view.ConfigurePools();
    }

    private void ConnectWallet()
    {
        dataStore.HUDs.connectWalletModalVisible.Set(true);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;

        view.OnInfoClicked -= ShowEventDetailedInfo;
        view.OnJumpInClicked -= OnJumpInToEvent;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnShowMoreEventsClicked -= ShowMoreEvents;
        view.OnEventsSubSectionEnable -= RequestAllEvents;
        view.OnFiltersChanged -= LoadFilteredEvents;
        view.OnConnectWallet -= ConnectWallet;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnEventsSubSectionEnable += RequestAllEvents;
        view.OnFiltersChanged += LoadFilteredEvents;
        cardsReloader.Initialize();
    }

    internal void RequestAllEvents()
    {
        RequestAndLoadCategories();
        view.SetIsGuestUser(userProfileBridge.GetOwn().isGuest);
        if (cardsReloader.CanReload())
        {
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
            view.SetShowMoreEventsButtonActive(false);
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
        view.SetEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterUpcomingEvents()));
        view.SetShowMoreEventsButtonActive(availableUISlots < eventsFromAPI.Count);
    }

    private void LoadFilteredEvents()
    {
        List<EventCardComponentModel> filteredEventCards = new ();

        switch (view.SelectedEventType)
        {
            case EventsType.Upcoming:
                availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(FilterUpcomingEvents());
                view.SetShowMoreEventsButtonActive(availableUISlots < eventsFromAPI.Count);
                break;
            case EventsType.Featured:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(FilterFeaturedEvents());
                view.SetShowMoreEventsButtonActive(false);
                break;
            case EventsType.Trending:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(FilterTrendingEvents());
                view.SetShowMoreEventsButtonActive(false);
                break;
            case EventsType.WantToGo:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(FilterWantToGoEvents());
                view.SetShowMoreEventsButtonActive(false);
                break;
        }

        view.SetEvents(filteredEventCards);
    }

    internal List<EventFromAPIModel> FilterUpcomingEvents() => eventsFromAPI.Take(availableUISlots).ToList();

    internal List<EventFromAPIModel> FilterFeaturedEvents()
    {
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Where(e => e.highlighted).ToList();

        if (eventsFiltered.Count == 0)
            eventsFiltered = eventsFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_EVENTS).ToList();

        return eventsFiltered;
    }

    internal List<EventFromAPIModel> FilterTrendingEvents() => eventsFromAPI.Where(e => e.trending).ToList();

    internal List<EventFromAPIModel> FilterWantToGoEvents() => eventsFromAPI.Where(e => e.attending).ToList();


    public void ShowMoreEvents()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentEventsPerRow) * view.currentEventsPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentEventsPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<EventFromAPIModel> eventsFiltered = availableUISlots + numberOfItemsToAdd <= eventsFromAPI.Count
            ? eventsFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : eventsFromAPI.GetRange(availableUISlots, eventsFromAPI.Count - availableUISlots);

        view.AddEvents(PlacesAndEventsCardsFactory.CreateEventsCards(eventsFiltered));

        availableUISlots += numberOfItemsToAdd;
        if (availableUISlots > eventsFromAPI.Count)
            availableUISlots = eventsFromAPI.Count;

        view.SetShowMoreEventsButtonActive(availableUISlots < eventsFromAPI.Count);
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

    private void SubscribeToEvent(string eventId)
    {
        if (userProfileBridge.GetOwn().isGuest)
            ConnectWallet();
        else
        {
            eventsAPIApiController.RegisterParticipation(eventId);
            exploreV2Analytics.SendParticipateEvent(eventId);
        }
    }

    private void UnsubscribeToEvent(string eventId)
    {
        eventsAPIApiController.RemoveParticipation(eventId);
        exploreV2Analytics.SendRemoveParticipateEvent(eventId);
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

    private void RequestAndLoadCategories()
    {
        eventsAPIApiController.GetCategories(
            OnSuccess: eventList => view.SetCategories(PlacesAndEventsCardsFactory.ConvertCategoriesResponseToToggleModel(eventList)),
            OnFail: error => { Debug.LogError($"Error receiving categories from the API: {error}"); });
    }
}
