using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using ExploreV2Analytics;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private readonly IPlacesAPIService placesAPIService;
    private readonly IWorldsAPIService worldsAPIService;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<EventFromAPIModel> eventsFromAPI = new ();
    internal int availableUISlots;

    private CancellationTokenSource getPlacesAssociatedToEventsCts;

    public EventsSubSectionComponentController(
        IEventsSubSectionComponentView view,
        IEventsAPIController eventsAPI,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IPlacesAPIService placesAPIService,
        IWorldsAPIService worldsAPIService)
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

        this.placesAPIService = placesAPIService;
        this.worldsAPIService = worldsAPIService;

        view.ConfigurePools();
    }

    private void ConnectWallet()
    {
        dataStore.HUDs.connectWalletModalVisible.Set(true);
    }

    public void Dispose()
    {
        getPlacesAssociatedToEventsCts.SafeCancelAndDispose();

        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowEventDetailedInfo;
        view.OnJumpInClicked -= OnJumpInToEvent;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnShowMoreEventsClicked -= ShowMoreEvents;
        view.OnEventsSubSectionEnable -= RequestAllEvents;
        view.OnEventTypeFiltersChanged -= ApplyEventTypeFilters;
        view.OnEventFrequencyFilterChanged -= ApplyEventFrequencyFilter;
        view.OnEventCategoryFilterChanged -= ApplyEventCategoryFilter;
        view.OnEventTimeFilterChanged -= ApplyEventTimeFilter;
        view.OnConnectWallet -= ConnectWallet;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnEventsSubSectionEnable += RequestAllEvents;
        view.OnEventTypeFiltersChanged += ApplyEventTypeFilters;
        view.OnEventFrequencyFilterChanged += ApplyEventFrequencyFilter;
        view.OnEventCategoryFilterChanged += ApplyEventCategoryFilter;
        view.OnEventTimeFilterChanged += ApplyEventTimeFilter;
        cardsReloader.Initialize();
    }

    internal void RequestAllEvents()
    {
        exploreV2Analytics.SendEventsTabOpen();

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

        getPlacesAssociatedToEventsCts = getPlacesAssociatedToEventsCts.SafeRestart();
        GetPlacesAssociatedToEventsAsync(getPlacesAssociatedToEventsCts.Token).Forget();

        async UniTaskVoid GetPlacesAssociatedToEventsAsync(CancellationToken ct)
        {
            // Land's events
            var landEventsFromAPI = eventsFromAPI.Where(e => !e.world).ToList();
            var coordsList = landEventsFromAPI.Select(e => new Vector2Int(e.coordinates[0], e.coordinates[1]));
            var places = await placesAPIService.GetPlacesByCoordsList(coordsList, ct);

            foreach (EventFromAPIModel landEventFromAPI in landEventsFromAPI)
            {
                Vector2Int landEventCoords = new Vector2Int(landEventFromAPI.coordinates[0], landEventFromAPI.coordinates[1]);
                foreach (IHotScenesController.PlaceInfo place in places)
                {
                    if (!place.Positions.Contains(landEventCoords))
                        continue;

                    landEventFromAPI.scene_name = place.title;
                    break;
                }
            }

            // World's events
            var worldEventsFromAPI = eventsFromAPI.Where(e => e.world).ToList();
            var worldNamesList = worldEventsFromAPI.Select(e => e.server);
            var worlds = await worldsAPIService.GetWorldsByNamesList(worldNamesList, ct);

            foreach (EventFromAPIModel worldEventFromAPI in worldEventsFromAPI)
            {
                foreach (WorldsResponse.WorldInfo world in worlds)
                {
                    if (world.world_name != worldEventFromAPI.server)
                        continue;

                    worldEventFromAPI.scene_name = world.title;
                    break;
                }
            }

            view.SetFeaturedEvents(PlacesAndEventsCardsFactory.CreateEventsCards(FilterFeaturedEvents()));
            LoadFilteredEvents();
            RequestAndLoadCategories();
        }
    }

    private void ApplyEventTypeFilters()
    {
        switch (view.SelectedEventType)
        {
            case EventsType.Featured:
                exploreV2Analytics.SendFilterEvents(FilterType.Featured);
                break;
            case EventsType.Trending:
                exploreV2Analytics.SendFilterEvents(FilterType.Trending);
                break;
            case EventsType.WantToGo:
                exploreV2Analytics.SendFilterEvents(FilterType.WantToGo);
                break;
        }

        LoadFilteredEvents();
    }

    private void ApplyEventFrequencyFilter()
    {
        exploreV2Analytics.SendFilterEvents(FilterType.Frequency, view.SelectedFrequency);
        LoadFilteredEvents();
    }

    private void ApplyEventCategoryFilter()
    {
        exploreV2Analytics.SendFilterEvents(FilterType.Category, view.SelectedCategory);
        LoadFilteredEvents();
    }

    private void ApplyEventTimeFilter()
    {
        exploreV2Analytics.SendFilterEvents(FilterType.Time);
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
                   FrequencyFilterQuery(e, frequencyFilter) &&
                   CategoryFilterQuery(e, categoryFilter) &&
                   TimeFilterQuery(e, lowTimeFilter, highTimeFilter))
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
                                                .Where(e =>
                                                     e.highlighted &&
                                                     FrequencyFilterQuery(e, frequencyFilter) &&
                                                     CategoryFilterQuery(e, categoryFilter) &&
                                                     TimeFilterQuery(e, lowTimeFilter, highTimeFilter))
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
              .Where(e =>
                   e.trending &&
                   FrequencyFilterQuery(e, frequencyFilter) &&
                   CategoryFilterQuery(e, categoryFilter) &&
                   TimeFilterQuery(e, lowTimeFilter, highTimeFilter))
              .ToList();
    }

    internal List<EventFromAPIModel> FilterWantToGoEvents(
        string frequencyFilter = ALL_FILTER_ID,
        string categoryFilter = ALL_FILTER_ID,
        float lowTimeFilter = TIME_MIN_VALUE,
        float highTimeFilter = TIME_MAX_VALUE)
    {
        return eventsFromAPI
              .Where(e =>
                   e.attending &&
                   FrequencyFilterQuery(e, frequencyFilter) &&
                   CategoryFilterQuery(e, categoryFilter) &&
                   TimeFilterQuery(e, lowTimeFilter, highTimeFilter))
              .ToList();
    }

    private static bool FrequencyFilterQuery(EventFromAPIModel e, string frequencyFilter) =>
        frequencyFilter == ALL_FILTER_ID ||
        (frequencyFilter == RECURRING_EVENT_FREQUENCY_FILTER_ID ?
            e.duration > TimeSpan.FromDays(1).TotalMilliseconds || e.recurrent :
            e.duration <= TimeSpan.FromDays(1).TotalMilliseconds);

    private static bool CategoryFilterQuery(EventFromAPIModel e, string categoryFilter) =>
        categoryFilter == ALL_FILTER_ID || e.categories.Contains(categoryFilter);

    private static bool TimeFilterQuery(EventFromAPIModel e, float lowTimeFilter, float highTimeFilter) =>
        IsTimeInRange(e.start_at, lowTimeFilter, highTimeFilter);

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

        if (string.IsNullOrEmpty(eventFromAPI.server))
            Environment.i.world.teleportController.Teleport(coords.x, coords.y);
        else
            Environment.i.world.teleportController.JumpIn(coords.x, coords.y, eventFromAPI.server);
    }

    private void RequestAndLoadCategories()
    {
        eventsAPIApiController.GetCategories(
            onSuccess: categoryList =>
            {
                List<CategoryFromAPIModel> categoriesInUse = new ();
                foreach (CategoryFromAPIModel category in categoryList)
                {
                    foreach (EventFromAPIModel loadedEvent in eventsFromAPI)
                    {
                        if (!loadedEvent.categories.Contains(category.name))
                            continue;

                        categoriesInUse.Add(category);
                        break;
                    }
                }

                view.SetCategories(PlacesAndEventsCardsFactory.ConvertCategoriesResponseToToggleModel(categoriesInUse));
            },
            onFail: error => { Debug.LogError($"Error receiving categories from the API: {error}"); });
    }

    private static bool IsTimeInRange(string dateTime, float lowTimeValue, float highTimeValue)
    {
        string startTimeString = ConvertToTimeString(lowTimeValue);
        string endTimeString = ConvertToTimeString(highTimeValue);

        TimeSpan startTime = lowTimeValue < TIME_MAX_VALUE ? TimeSpan.Parse(startTimeString) : TimeSpan.FromDays(1);
        TimeSpan endTime = highTimeValue < TIME_MAX_VALUE ? TimeSpan.Parse(endTimeString) : TimeSpan.FromDays(1);
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
