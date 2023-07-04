using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchSubSectionComponentView : BaseComponentView, ISearchSubSectionComponentView
{
    private const int MAX_EVENTS_COUNT = 5;

    public int CurrentTilesPerRow { get; }
    public int CurrentGoingTilesPerRow { get; }

    [SerializeField] private GameObject minimalSearchSection;
    [SerializeField] private GameObject fullSearchSection;
    [SerializeField] private GameObject fullSearchEventsSection;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform eventsParent;
    [SerializeField] private Transform fullEventsParent;
    [SerializeField] private EventCardComponentView eventPrefab;
    [SerializeField] private GameObject loadingEvents;
    [SerializeField] private GameObject loadingPlaces;
    [SerializeField] private Button showAllPlaces;
    [SerializeField] private Button showAllEvents;

    public event Action OnRequestAllEvents;

    private UnityObjectPool<EventCardComponentView> eventsPool;
    private List<EventCardComponentView> pooledEvents = new List<EventCardComponentView>();
    private UnityObjectPool<EventCardComponentView> fullEventsPool;
    private List<EventCardComponentView> pooledFullEvents = new List<EventCardComponentView>();

    public override void Awake()
    {
        InitializePools();
        showAllEvents.onClick.RemoveAllListeners();
        showAllEvents.onClick.AddListener(RequestAllEvents);
    }

    private void RequestAllEvents()
    {
        minimalSearchSection.SetActive(false);
        fullSearchSection.SetActive(true);
        fullSearchEventsSection.SetActive(true);
        OnRequestAllEvents?.Invoke();
    }

    public void SetAsLoading()
    {
        minimalSearchSection.SetActive(true);
        fullSearchSection.SetActive(false);
        eventsParent.gameObject.SetActive(false);
        loadingEvents.SetActive(true);
        loadingPlaces.SetActive(true);
    }

    public void ShowEvents(List<EventCardComponentModel> events)
    {
        ClearEventsPool();
        foreach (EventCardComponentModel eventCardComponentModel in events)
        {
            EventCardComponentView eventCardComponentView = eventsPool.Get();
            eventCardComponentView.model = eventCardComponentModel;
            eventCardComponentView.RefreshControl();
            pooledEvents.Add(eventCardComponentView);
        }
        eventsParent.gameObject.SetActive(true);
        loadingEvents.gameObject.SetActive(false);
    }

    public void ShowAllEvents(List<EventCardComponentModel> events)
    {
        ClearFullEventsPool();
        foreach (EventCardComponentModel eventCardComponentModel in events)
        {
            EventCardComponentView eventCardComponentView = fullEventsPool.Get();
            eventCardComponentView.model = eventCardComponentModel;
            eventCardComponentView.RefreshControl();
            pooledFullEvents.Add(eventCardComponentView);
        }
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

    public void SetAllAsLoading()
    {
    }

    public void SetActive(bool isActive)
    {
        if (canvas.enabled == isActive)
            return;
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
            OnDisable();
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
