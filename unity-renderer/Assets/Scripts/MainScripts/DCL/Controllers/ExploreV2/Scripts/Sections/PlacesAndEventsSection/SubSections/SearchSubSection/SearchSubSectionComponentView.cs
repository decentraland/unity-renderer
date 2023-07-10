using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils = DCL.Helpers.Utils;

public class SearchSubSectionComponentView : BaseComponentView, ISearchSubSectionComponentView
{
    private const int MAX_EVENTS_COUNT = 5;

    public int CurrentTilesPerRow { get; }
    public int CurrentGoingTilesPerRow { get; }

    [SerializeField] private GameObject minimalSearchSection;
    [SerializeField] private GameObject fullSearchSection;
    [SerializeField] private GameObject fullSearchEventsSection;
    [SerializeField] private GameObject normalHeader;
    [SerializeField] private GameObject searchHeader;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text searchTerm;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform eventsParent;
    [SerializeField] private RectTransform fullEventsParent;
    [SerializeField] private EventCardComponentView eventPrefab;
    [SerializeField] private GameObject loadingEvents;
    [SerializeField] private GameObject loadingPlaces;
    [SerializeField] private GameObject loadingAll;
    [SerializeField] private Button showAllPlaces;
    [SerializeField] private Button showAllEvents;
    [SerializeField] private Button showMore;
    [SerializeField] private Button backFromAllList;

    [SerializeField] private GameObject noEvents;
    [SerializeField] private GameObject noPlaces;
    [SerializeField] private TMP_Text noEventsText;
    [SerializeField] private TMP_Text noPlacesText;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    internal EventCardComponentView eventModal;
    public event Action<int> OnRequestAllEvents;
    public event Action OnBackFromSearch;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;

    private UnityObjectPool<EventCardComponentView> eventsPool;
    private List<EventCardComponentView> pooledEvents = new List<EventCardComponentView>();
    private UnityObjectPool<EventCardComponentView> fullEventsPool;
    private List<EventCardComponentView> pooledFullEvents = new List<EventCardComponentView>();
    private int currentPage = 0;

    public override void Awake()
    {
        InitializePools();
        showAllEvents.onClick.RemoveAllListeners();
        showAllEvents.onClick.AddListener(RequestAllEvents);
        showMore.onClick.RemoveAllListeners();
        showMore.onClick.AddListener(RequestAdditionalPage);
        backFromAllList.onClick.RemoveAllListeners();
        backFromAllList.onClick.AddListener(CloseFullList);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackFromSearch?.Invoke());
        noEvents.SetActive(false);
        noPlaces.SetActive(true);
        eventModal = PlacesAndEventsCardsFactory.GetEventCardTemplateHiddenLazy(eventCardModalPrefab);
    }

    private void RequestAdditionalPage()
    {
        currentPage++;
        OnRequestAllEvents?.Invoke(currentPage);
    }

    private void RequestAllEvents()
    {
        currentPage = 0;
        minimalSearchSection.SetActive(false);
        fullSearchSection.SetActive(true);
        fullSearchEventsSection.SetActive(true);
        loadingAll.SetActive(true);
        ClearFullEventsPool();
        OnRequestAllEvents?.Invoke(currentPage);
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

        if (events.Count == 0)
        {
            noEvents.SetActive(true);
            noEventsText.text = $"No events found for '{searchText}'";
        }
        else
        {
            noEvents.SetActive(false);
        }
    }

    private void ConfigureEventCardActions(EventCardComponentView view, EventCardComponentModel model)
    {
        view.onInfoClick.RemoveAllListeners();
        view.onSubscribeClick.RemoveAllListeners();
        view.onUnsubscribeClick.RemoveAllListeners();
        view.onJumpInClick.RemoveAllListeners();
        view.onInfoClick.AddListener(() => OnInfoClicked?.Invoke(model));
        view.onSubscribeClick.AddListener(() => OnSubscribeEventClicked?.Invoke(model.eventId));
        view.onUnsubscribeClick.AddListener(() => OnUnsubscribeEventClicked?.Invoke(model.eventId));
        view.onJumpInClick.AddListener(() => OnJumpInClicked?.Invoke(new EventFromAPIModel(){}));
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        EventsCardsConfigurator.Configure(eventModal, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked);
    }

    public void ShowAllEvents(List<EventCardComponentModel> events, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (EventCardComponentModel eventCardComponentModel in events)
        {
            EventCardComponentView eventCardComponentView = fullEventsPool.Get();
            eventCardComponentView.model = eventCardComponentModel;
            eventCardComponentView.RefreshControl();
            pooledFullEvents.Add(eventCardComponentView);
            ConfigureEventCardActions(eventCardComponentView, eventCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullEventsParent);
    }

    private void InitializePools()
    {
        eventsPool = new UnityObjectPool<EventCardComponentView>(eventPrefab, eventsParent);
        eventsPool.Prewarm(MAX_EVENTS_COUNT);
        fullEventsPool = new UnityObjectPool<EventCardComponentView>(eventPrefab, fullEventsParent);
        fullEventsPool.Prewarm(MAX_EVENTS_COUNT);
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
        loadingEvents.SetActive(true);
        loadingPlaces.SetActive(true);
    }

    public void SetHeaderEnabled(bool isEnabled, string searchText)
    {
        normalHeader.SetActive(!isEnabled);
        searchHeader.SetActive(isEnabled);
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
            SetHeaderEnabled(false, "");
        }
    }

    public override void RefreshControl()
    {
    }

    public override void Dispose()
    {
        ClearEventsPool();
        ClearFullEventsPool();
    }

    private void ClearEventsPool()
    {
        foreach (var pooledEvent in pooledEvents)
            eventsPool.Release(pooledEvent);
        pooledEvents.Clear();
    }

    private void ClearFullEventsPool(){
        foreach (var pooledEvent in pooledFullEvents)
                fullEventsPool.Release(pooledEvent);
        pooledFullEvents.Clear();
    }
}
