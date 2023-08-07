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
    private const string ALL_FILTER_ID = "all";
    private const string RECURRING_EVENT_FREQUENCY_FILTER_ID = "recurring_event";
    private const float TIME_MIN_VALUE = 0;
    private const float TIME_MAX_VALUE = 48;

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
        LoadFilteredEvents();
    }

    private void LoadFilteredEvents()
    {
        List<EventCardComponentModel> filteredEventCards = new ();

        bool anyFilterApplied =
            view.SelectedEventType != EventsType.Upcoming ||
            view.SelectedFrequency != ALL_FILTER_ID ||
            view.SelectedCategory != ALL_FILTER_ID ||
            view.SelectedLowTime > TIME_MIN_VALUE ||
            view.SelectedHighTime < TIME_MAX_VALUE;

        switch (view.SelectedEventType)
        {
            case EventsType.Upcoming:
                availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(
                    FilterUpcomingEvents(view.SelectedFrequency, view.SelectedCategory, view.SelectedLowTime, view.SelectedHighTime, anyFilterApplied));
                view.SetShowMoreEventsButtonActive(!anyFilterApplied && availableUISlots < eventsFromAPI.Count);
                break;
            case EventsType.Featured:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(
                    FilterFeaturedEvents(false, view.SelectedFrequency, view.SelectedCategory, view.SelectedLowTime, view.SelectedHighTime));
                view.SetShowMoreEventsButtonActive(false);
                break;
            case EventsType.Trending:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(
                    FilterTrendingEvents(view.SelectedFrequency, view.SelectedCategory, view.SelectedLowTime, view.SelectedHighTime));
                view.SetShowMoreEventsButtonActive(false);
                break;
            case EventsType.WantToGo:
                filteredEventCards = PlacesAndEventsCardsFactory.CreateEventsCards(
                    FilterWantToGoEvents(view.SelectedFrequency, view.SelectedCategory, view.SelectedLowTime, view.SelectedHighTime));
                view.SetShowMoreEventsButtonActive(false);
                break;
        }

        view.SetEvents(filteredEventCards);
    }

    internal List<EventFromAPIModel> FilterUpcomingEvents(
        string frequencyFilter = ALL_FILTER_ID,
        string categoryFilter = ALL_FILTER_ID,
        float lowTimeFilter = TIME_MIN_VALUE,
        float highTimeFilter = TIME_MAX_VALUE,
        bool takeAllResults = false)
    {
        return eventsFromAPI
              .Where(e =>
                   (frequencyFilter == ALL_FILTER_ID ||
                    (frequencyFilter == RECURRING_EVENT_FREQUENCY_FILTER_ID ?
                        e.duration > TimeSpan.FromDays(1).TotalMilliseconds || e.recurrent :
                        e.duration <= TimeSpan.FromDays(1).TotalMilliseconds)) &&
                   (categoryFilter == ALL_FILTER_ID || e.categories.Contains(categoryFilter)) &&
                   IsTimeInRange(e.start_at, lowTimeFilter, highTimeFilter))
                         .Take(takeAllResults ? eventsFromAPI.Count : availableUISlots)
                         .ToList();
    }

    internal List<EventFromAPIModel> FilterFeaturedEvents(
        bool showDefaultsIfNoData = true,
        string frequencyFilter = ALL_FILTER_ID,
        string categoryFilter = ALL_FILTER_ID,
        float lowTimeFilter = TIME_MIN_VALUE,
        float highTimeFilter = TIME_MAX_VALUE)
    {
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI
                                                .Where(e => e.highlighted &&
                                                            (frequencyFilter == ALL_FILTER_ID ||
                                                             (frequencyFilter == RECURRING_EVENT_FREQUENCY_FILTER_ID ?
                                                                 e.duration > TimeSpan.FromDays(1).TotalMilliseconds || e.recurrent :
                                                                 e.duration <= TimeSpan.FromDays(1).TotalMilliseconds)) &&
                                                            (categoryFilter == ALL_FILTER_ID || e.categories.Contains(categoryFilter)) &&
                                                            IsTimeInRange(e.start_at, lowTimeFilter, highTimeFilter))
                                                .ToList();

        if (eventsFiltered.Count == 0 && showDefaultsIfNoData)
            eventsFiltered = eventsFromAPI.Take(DEFAULT_NUMBER_OF_FEATURED_EVENTS).ToList();

        return eventsFiltered;
    }

    internal List<EventFromAPIModel> FilterTrendingEvents(
        string frequencyFilter = ALL_FILTER_ID,
        string categoryFilter = ALL_FILTER_ID,
        float lowTimeFilter = TIME_MIN_VALUE,
        float highTimeFilter = TIME_MAX_VALUE)
    {
        return eventsFromAPI
              .Where(e => e.trending &&
                          (frequencyFilter == ALL_FILTER_ID ||
                           (frequencyFilter == RECURRING_EVENT_FREQUENCY_FILTER_ID ?
                               e.duration > TimeSpan.FromDays(1).TotalMilliseconds || e.recurrent :
                               e.duration <= TimeSpan.FromDays(1).TotalMilliseconds)) &&
                          (categoryFilter == ALL_FILTER_ID || e.categories.Contains(categoryFilter)) &&
                          IsTimeInRange(e.start_at, lowTimeFilter, highTimeFilter))
              .ToList();
    }

    internal List<EventFromAPIModel> FilterWantToGoEvents(
        string frequencyFilter = ALL_FILTER_ID,
        string categoryFilter = ALL_FILTER_ID,
        float lowTimeFilter = TIME_MIN_VALUE,
        float highTimeFilter = TIME_MAX_VALUE)
    {
        return eventsFromAPI
              .Where(e => e.attending &&
                          (frequencyFilter == ALL_FILTER_ID ||
                           (frequencyFilter == RECURRING_EVENT_FREQUENCY_FILTER_ID ?
                               e.duration > TimeSpan.FromDays(1).TotalMilliseconds || e.recurrent :
                               e.duration <= TimeSpan.FromDays(1).TotalMilliseconds)) &&
                          (categoryFilter == ALL_FILTER_ID || e.categories.Contains(categoryFilter)) &&
                          IsTimeInRange(e.start_at, lowTimeFilter, highTimeFilter))
              .ToList();
    }

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

    private static bool IsTimeInRange(string dateTime, float lowTimeValue, float highTimeValue)
    {
        string startTimeString = ConvertToTimeString(lowTimeValue);
        string endTimeString = ConvertToTimeString(highTimeValue);

        TimeSpan startTime = lowTimeValue < TIME_MAX_VALUE ? TimeSpan.Parse(startTimeString) : new TimeSpan(1, 0, 0, 0);
        TimeSpan endTime = highTimeValue < TIME_MAX_VALUE ? TimeSpan.Parse(endTimeString) : new TimeSpan(1, 0, 0, 0);
        TimeSpan currentTime = Convert.ToDateTime(dateTime).ToUniversalTime().TimeOfDay;
        return currentTime >= startTime && currentTime <= endTime;
    }

    private static string ConvertToTimeString(float hours)
    {
        var wholeHours = (int)(hours / 2);
        int minutes = (int)(hours % 2) * 30;
        return $"{wholeHours:D2}:{minutes:D2}";
    }
}
