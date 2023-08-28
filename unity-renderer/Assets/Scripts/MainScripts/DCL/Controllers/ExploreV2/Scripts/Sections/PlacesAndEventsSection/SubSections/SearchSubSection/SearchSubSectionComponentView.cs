using DCL;
using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils = DCL.Helpers.Utils;

public class SearchSubSectionComponentView : BaseComponentView, ISearchSubSectionComponentView
{
    private const int MAX_POOL_COUNT = 6;
    internal const string WORLDS_SUBSECTION_FF = "enable_worlds_subsection";

    public int CurrentTilesPerRow { get; }
    public int CurrentGoingTilesPerRow { get; }

    [SerializeField] private GameObject minimalSearchSection;
    [SerializeField] private GameObject fullSearchSection;
    [SerializeField] private GameObject normalHeader;
    [SerializeField] private GameObject searchHeader;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text searchTerm;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject worldsSection;
    [SerializeField] private Transform eventsParent;
    [SerializeField] private Transform placesParent;
    [SerializeField] private Transform worldsParent;
    [SerializeField] private RectTransform fullEventsParent;
    [SerializeField] private RectTransform fullPlacesParent;
    [SerializeField] private RectTransform fullWorldsParent;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private EventCardComponentView eventPrefab;
    [SerializeField] private PlaceCardComponentView placePrefab;
    [SerializeField] private GameObject loadingEvents;
    [SerializeField] private GameObject loadingPlaces;
    [SerializeField] private GameObject loadingWorlds;
    [SerializeField] private GameObject loadingAll;
    [SerializeField] private Button showAllPlaces;
    [SerializeField] private Button showAllWorlds;
    [SerializeField] private Button showAllEvents;
    [SerializeField] private Button showMore;

    [SerializeField] internal GameObject noEvents;
    [SerializeField] internal GameObject noPlaces;
    [SerializeField] internal GameObject noWorlds;
    [SerializeField] internal GameObject noResults;
    [SerializeField] private TMP_Text noEventsText;
    [SerializeField] private TMP_Text noPlacesText;
    [SerializeField] private TMP_Text noWorldsText;
    [SerializeField] private TMP_Text noResultsText;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    internal EventCardComponentView eventModal;
    internal PlaceCardComponentView placeModal;
    public event Action<int> OnRequestAllEvents;
    public event Action<int> OnRequestAllPlaces;
    public event Action<int> OnRequestAllWorlds;
    public event Action OnBackFromSearch;
    public event Action<EventCardComponentModel, int> OnEventInfoClicked;
    public event Action<PlaceCardComponentModel, int> OnPlaceInfoClicked;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<string, bool> OnPlaceFavoriteChanged;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;

    private UnityObjectPool<EventCardComponentView> eventsPool;
    internal List<EventCardComponentView> pooledEvents = new List<EventCardComponentView>();
    private UnityObjectPool<EventCardComponentView> fullEventsPool;
    internal List<EventCardComponentView> pooledFullEvents = new List<EventCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> placesPool;
    internal List<PlaceCardComponentView> pooledPlaces = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> fullPlacesPool;
    internal List<PlaceCardComponentView> pooledFullPlaces = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> worldsPool;
    internal List<PlaceCardComponentView> pooledWorlds = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> fullWorldsPool;
    internal List<PlaceCardComponentView> pooledFullWorlds = new List<PlaceCardComponentView>();
    private int currentPage = 0;

    public override void Awake()
    {
        InitializePools();
        InitialiseButtonEvents();

        noEvents.SetActive(false);
        noPlaces.SetActive(false);
        eventModal = PlacesAndEventsCardsFactory.GetEventCardTemplateHiddenLazy(eventCardModalPrefab);
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);

        //Temporary until the full feature is released
        worldsSection.SetActive(DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(WORLDS_SUBSECTION_FF));
    }

    private void InitialiseButtonEvents()
    {
        if (showAllEvents != null)
        {
            showAllEvents.onClick.RemoveAllListeners();
            showAllEvents.onClick.AddListener(RequestAllEvents);
        }

        if (showAllPlaces != null)
        {
            showAllPlaces.onClick.RemoveAllListeners();
            showAllPlaces.onClick.AddListener(RequestAllPlaces);
        }

        if (showAllWorlds != null)
        {
            showAllWorlds.onClick.RemoveAllListeners();
            showAllWorlds.onClick.AddListener(RequestAllWorlds);
        }

        if (showMore != null)
        {
            showMore.onClick.RemoveAllListeners();
            showMore.onClick.AddListener(RequestAdditionalPage);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonPressed);
        }
    }

    private void RequestAdditionalPage()
    {
        currentPage++;

        if(fullEventsParent.gameObject.activeSelf)
            OnRequestAllEvents?.Invoke(currentPage);
        else if(fullPlacesParent.gameObject.activeSelf)
            OnRequestAllPlaces?.Invoke(currentPage);
        else if(fullWorldsParent.gameObject.activeSelf)
            OnRequestAllWorlds?.Invoke(currentPage);
    }

    private void OnBackButtonPressed()
    {
        if (minimalSearchSection.activeSelf || noResults.activeSelf)
        {
            OnBackFromSearch?.Invoke();
        }
        else
        {
            minimalSearchSection.SetActive(true);
            fullSearchSection.SetActive(false);
        }
    }

    private void RequestAllEvents()
    {
        currentPage = 0;
        minimalSearchSection.SetActive(false);
        fullSearchSection.SetActive(true);
        fullEventsParent.gameObject.SetActive(true);
        fullPlacesParent.gameObject.SetActive(false);
        fullWorldsParent.gameObject.SetActive(false);
        loadingAll.SetActive(true);
        ClearFullEventsPool();
        OnRequestAllEvents?.Invoke(currentPage);
    }

    private void RequestAllPlaces()
    {
        currentPage = 0;
        minimalSearchSection.SetActive(false);
        fullSearchSection.SetActive(true);
        fullEventsParent.gameObject.SetActive(false);
        fullPlacesParent.gameObject.SetActive(true);
        fullWorldsParent.gameObject.SetActive(false);
        loadingAll.SetActive(true);
        ClearFullPlacesPool();
        OnRequestAllPlaces?.Invoke(currentPage);
    }

    private void RequestAllWorlds()
    {
        currentPage = 0;
        minimalSearchSection.SetActive(false);
        fullSearchSection.SetActive(true);
        fullEventsParent.gameObject.SetActive(false);
        fullPlacesParent.gameObject.SetActive(false);
        fullWorldsParent.gameObject.SetActive(true);
        loadingAll.SetActive(true);
        ClearFullWorldsPool();
        OnRequestAllWorlds?.Invoke(currentPage);
    }

    public void ShowEvents(List<EventCardComponentModel> events, string searchText)
    {
        ClearEventsPool();
        foreach (EventCardComponentModel eventCardComponentModel in events)
        {
            EventCardComponentView eventCardComponentView = eventsPool.Get();
            eventCardComponentView.model = eventCardComponentModel;
            eventCardComponentView.RefreshControl();
            pooledEvents.Add(eventCardComponentView);
            ConfigureEventCardActions(eventCardComponentView, eventCardComponentModel);
        }
        eventsParent.gameObject.SetActive(true);
        loadingEvents.gameObject.SetActive(false);

        showAllEvents.gameObject.SetActive(events.Count == 6);
        if (events.Count == 0)
        {
            noEvents.SetActive(true);
            noEventsText.text = $"No events found for '{searchText}'";
        }
        else
        {
            noEvents.SetActive(false);
        }
        CheckAndSetNoResults(searchText);
    }

    public void ShowPlaces(List<PlaceCardComponentModel> places, string searchText)
    {
        ClearPlacesPool();
        foreach (PlaceCardComponentModel placeCardComponentModel in places)
        {
            PlaceCardComponentView placeCardComponentView = placesPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            pooledPlaces.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        placesParent.gameObject.SetActive(true);
        loadingPlaces.gameObject.SetActive(false);

        showAllPlaces.gameObject.SetActive(places.Count == 6);
        if (places.Count == 0)
        {
            noPlaces.SetActive(true);
            noPlacesText.text = $"No places found for '{searchText}'";
        }
        else
        {
            noPlaces.SetActive(false);
        }
        CheckAndSetNoResults(searchText);
    }

    public void ShowWorlds(List<PlaceCardComponentModel> worlds, string searchText)
    {
        ClearWorldsPool();
        foreach (PlaceCardComponentModel placeCardComponentModel in worlds)
        {
            PlaceCardComponentView placeCardComponentView = worldsPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            pooledWorlds.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        worldsParent.gameObject.SetActive(true);
        loadingWorlds.gameObject.SetActive(false);

        showAllWorlds.gameObject.SetActive(worlds.Count == 6);
        if (worlds.Count == 0)
        {
            noWorlds.SetActive(true);
            noWorldsText.text = $"No wordls found for '{searchText}'";
        }
        else
        {
            noWorlds.SetActive(false);
        }
        CheckAndSetNoResults(searchText);
    }

    private void CheckAndSetNoResults(string searchText)
    {
        if (noPlaces.activeSelf && noEvents.activeSelf && noWorlds.activeSelf)
        {
            noResults.SetActive(true);
            minimalSearchSection.SetActive(false);
            noResultsText.text = $"No results found for '{searchText}'";
        }
        else
        {
            noResults.SetActive(false);
            if(minimalSearchSection.activeSelf == false)
                minimalSearchSection.SetActive(true);
        }
        Utils.ForceRebuildLayoutImmediate(gridContainer);
    }

    private void ConfigureEventCardActions(EventCardComponentView view, EventCardComponentModel model)
    {
        view.onInfoClick.RemoveAllListeners();
        view.onBackgroundClick.RemoveAllListeners();
        view.onSubscribeClick.RemoveAllListeners();
        view.onUnsubscribeClick.RemoveAllListeners();
        view.onJumpInClick.RemoveAllListeners();
        view.onSecondaryJumpInClick?.RemoveAllListeners();
        view.onInfoClick.AddListener(() => OnEventInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onBackgroundClick.AddListener(() => OnEventInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onSubscribeClick.AddListener(() => OnSubscribeEventClicked?.Invoke(model.eventId));
        view.onUnsubscribeClick.AddListener(() => OnUnsubscribeEventClicked?.Invoke(model.eventId));
        view.onJumpInClick.AddListener(() => OnEventJumpInClicked?.Invoke(model.eventFromAPIInfo));
        view.onSecondaryJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(model.eventFromAPIInfo));
    }

    private void ConfigurePlaceCardActions(PlaceCardComponentView view, PlaceCardComponentModel model)
    {
        view.onInfoClick.RemoveAllListeners();
        view.onBackgroundClick.RemoveAllListeners();
        view.onJumpInClick.RemoveAllListeners();
        view.OnFavoriteChanged -= ViewOnOnFavoriteChanged;
        view.OnVoteChanged -= ViewOnVoteChanged;
        view.onInfoClick.AddListener(()=>OnPlaceInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onBackgroundClick.AddListener(()=>OnPlaceInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onJumpInClick.AddListener(()=>OnPlaceJumpInClicked?.Invoke(model.placeInfo));
        view.OnFavoriteChanged += ViewOnOnFavoriteChanged;
        view.OnVoteChanged += ViewOnVoteChanged;
    }

    private void ViewOnVoteChanged(string arg1, bool? arg2)
    {
        OnVoteChanged?.Invoke(arg1, arg2);
    }

    private void ViewOnOnFavoriteChanged(string placeId, bool isFavorite)
    {
        OnPlaceFavoriteChanged?.Invoke(placeId, isFavorite);
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        EventsCardsConfigurator.Configure(eventModal, eventInfo, null, OnEventJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeModel)
    {
        placeModal.Show();
        PlacesCardsConfigurator.Configure(placeModal, placeModel, null, OnPlaceJumpInClicked, OnVoteChanged, OnPlaceFavoriteChanged);
    }

    public void ShowAllEvents(List<EventCardComponentModel> events, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (EventCardComponentModel eventCardComponentModel in events)
        {
            EventCardComponentView eventCardComponentView = fullEventsPool.Get();
            eventCardComponentView.model = eventCardComponentModel;
            eventCardComponentView.RefreshControl();
            eventCardComponentView.transform.SetAsLastSibling();
            pooledFullEvents.Add(eventCardComponentView);
            ConfigureEventCardActions(eventCardComponentView, eventCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullEventsParent);
    }

    public void ShowAllPlaces(List<PlaceCardComponentModel> places, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (PlaceCardComponentModel placeCardComponentModel in places)
        {
            PlaceCardComponentView placeCardComponentView = fullPlacesPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            placeCardComponentView.OnLoseFocus();
            placeCardComponentView.transform.SetAsLastSibling();
            pooledFullPlaces.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullPlacesParent);
    }

    public void ShowAllWorlds(List<PlaceCardComponentModel> worlds, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (PlaceCardComponentModel placeCardComponentModel in worlds)
        {
            PlaceCardComponentView placeCardComponentView = fullWorldsPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            placeCardComponentView.OnLoseFocus();
            placeCardComponentView.transform.SetAsLastSibling();
            pooledFullWorlds.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullWorldsParent);
    }

    private void InitializePools()
    {
        eventsPool = new UnityObjectPool<EventCardComponentView>(eventPrefab, eventsParent);
        eventsPool.Prewarm(MAX_POOL_COUNT);
        placesPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, placesParent);
        placesPool.Prewarm(MAX_POOL_COUNT);
        worldsPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, worldsParent);
        worldsPool.Prewarm(MAX_POOL_COUNT);
        fullEventsPool = new UnityObjectPool<EventCardComponentView>(eventPrefab, fullEventsParent);
        fullEventsPool.Prewarm(MAX_POOL_COUNT);
        fullPlacesPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, fullPlacesParent);
        fullPlacesPool.Prewarm(MAX_POOL_COUNT);
        fullWorldsPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, fullWorldsParent);
        fullWorldsPool.Prewarm(MAX_POOL_COUNT);
    }

    public void RestartScrollViewPosition()
    {
    }

    private void CloseFullList()
    {
        minimalSearchSection.SetActive(true);
        fullSearchSection.SetActive(false);
    }

    public void SetAllAsLoading()
    {
        CloseFullList();
        eventsParent.gameObject.SetActive(false);
        placesParent.gameObject.SetActive(false);
        worldsParent.gameObject.SetActive(false);
        loadingEvents.SetActive(true);
        loadingPlaces.SetActive(true);
        loadingWorlds.SetActive(true);
    }

    public void SetHeaderEnabled(string searchText)
    {
        normalHeader.SetActive(string.IsNullOrEmpty(searchText));
        searchHeader.SetActive(!string.IsNullOrEmpty(searchText));
        searchTerm.text = $"\"{searchText}\"";
    }

    public void SetActive(bool isActive)
    {
        if (canvas.enabled == isActive)
            return;
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
        {
            OnDisable();
        }
    }

    public override void RefreshControl()
    {
    }

    public override void Dispose()
    {
        ClearEventsPool();
        ClearFullEventsPool();
        ClearPlacesPool();
        ClearFullPlacesPool();
        ClearWorldsPool();
        ClearFullWorldsPool();
    }

    private void ClearEventsPool()
    {
        foreach (var pooledEvent in pooledEvents)
            eventsPool.Release(pooledEvent);
        pooledEvents.Clear();
    }

    private void ClearFullEventsPool()
    {
        foreach (var pooledEvent in pooledFullEvents)
            fullEventsPool.Release(pooledEvent);
        pooledFullEvents.Clear();
    }

    private void ClearPlacesPool()
    {
        foreach (var pooledEvent in pooledPlaces)
            placesPool.Release(pooledEvent);
        pooledPlaces.Clear();
    }

    private void ClearFullPlacesPool(){
        foreach (var pooledEvent in pooledFullPlaces)
            fullPlacesPool.Release(pooledEvent);
        pooledFullPlaces.Clear();
    }

    private void ClearWorldsPool()
    {
        foreach (var pooledWorld in pooledWorlds)
            worldsPool.Release(pooledWorld);
        pooledWorlds.Clear();
    }

    private void ClearFullWorldsPool(){
        foreach (var pooledWorld in pooledFullWorlds)
            fullWorldsPool.Release(pooledWorld);
        pooledFullWorlds.Clear();
    }
}
