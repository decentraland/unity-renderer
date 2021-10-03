using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IEventsSubSectionComponentView
{
    event Action OnReady;

    void SetFeaturedEvents(List<EventCardComponentModel> events);
    void SetFeaturedEventsAsLoading(bool isVisible);
    void SetTrendingEvents(List<EventCardComponentModel> events);
    void SetTrendingEventsAsLoading(bool isVisible);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetUpcomingEventsAsLoading(bool isVisible);
    void ShowMoreUpcomingEvents();
    void SetGoingEvents(List<EventCardComponentModel> events);
    void SetGoingEventsAsLoading(bool isVisible);
    void ShowEventModal(EventCardComponentModel eventInfo);
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GameObject featuredEventsLoading;
    [SerializeField] internal GridContainerComponentView trendingEvents;
    [SerializeField] internal GameObject trendingEventsLoading;
    [SerializeField] internal TMP_Text trendingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView upcomingEvents;
    [SerializeField] internal GameObject upcomingEventsLoading;
    [SerializeField] internal TMP_Text upcomingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView goingEvents;
    [SerializeField] internal GameObject goingEventsLoading;
    [SerializeField] internal TMP_Text goingEventsNoDataText;
    [SerializeField] internal ButtonComponentView showMoreUpcomingEventsButton;

    public event Action OnReady;

    internal EventCardComponentView eventModal;

    public override void PostInitialization()
    {
        StartCoroutine(WaitForComponentsInitialization());

        eventModal = GameObject.Instantiate(eventCardModalPrefab);
        eventModal.gameObject.SetActive(false);
    }

    public override void RefreshControl()
    {
        featuredEvents.RefreshControl();
        trendingEvents.RefreshControl();
        upcomingEvents.RefreshControl();
        goingEvents.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        showMoreUpcomingEventsButton.onClick.RemoveAllListeners();
    }

    public IEnumerator WaitForComponentsInitialization()
    {
        yield return new WaitUntil(() => featuredEvents.isFullyInitialized &&
                                         trendingEvents.isFullyInitialized &&
                                         upcomingEvents.isFullyInitialized &&
                                         goingEvents.isFullyInitialized);

        RefreshControl();

        showMoreUpcomingEventsButton.onClick.AddListener(ShowMoreUpcomingEvents);

        OnReady?.Invoke();
    }

    public void SetFeaturedEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardLongPrefab);
        featuredEvents.SetItems(eventComponentsToAdd, false);
    }
    public void SetFeaturedEventsAsLoading(bool isVisible)
    {
        featuredEvents.gameObject.SetActive(!isVisible);
        featuredEventsLoading.SetActive(isVisible);
    }

    public void SetTrendingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        trendingEvents.SetItems(eventComponentsToAdd, false);
        trendingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetTrendingEventsAsLoading(bool isVisible)
    {
        trendingEvents.gameObject.SetActive(!isVisible);
        trendingEventsLoading.SetActive(isVisible);
        trendingEventsNoDataText.gameObject.SetActive(false);
    }

    public void SetUpcomingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        upcomingEvents.SetItems(eventComponentsToAdd, false);
        upcomingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetUpcomingEventsAsLoading(bool isVisible)
    {
        upcomingEvents.gameObject.SetActive(!isVisible);
        upcomingEventsLoading.SetActive(isVisible);
        upcomingEventsNoDataText.gameObject.SetActive(false);
    }

    public void SetGoingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        goingEvents.SetItems(eventComponentsToAdd, false);
        goingEventsNoDataText.gameObject.SetActive(goingEvents.gameObject.activeInHierarchy && events.Count == 0);
    }

    public void SetGoingEventsAsLoading(bool isVisible)
    {
        goingEvents.gameObject.SetActive(!isVisible);
        goingEventsLoading.SetActive(isVisible);
        goingEventsNoDataText.gameObject.SetActive(false);
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.gameObject.SetActive(true);
        eventModal.Configure(eventInfo);
    }

    public void ShowMoreUpcomingEvents()
    {
        // TBD...
    }

    internal List<BaseComponentView> IntantiateAndConfigureEvents(List<EventCardComponentModel> events, EventCardComponentView prefabToUse)
    {
        List<BaseComponentView> instantiatedEvents = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = GameObject.Instantiate(prefabToUse);
            eventGO.Configure(eventInfo);
            instantiatedEvents.Add(eventGO);
        }

        return instantiatedEvents;
    }
}