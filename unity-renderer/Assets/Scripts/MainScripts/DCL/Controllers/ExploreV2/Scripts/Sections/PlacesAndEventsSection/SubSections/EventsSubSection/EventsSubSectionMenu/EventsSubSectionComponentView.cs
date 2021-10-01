using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IEventsSubSectionComponentView
{
    event Action OnReady;

    void SetFeaturedEvents(List<EventCardComponentModel> events);
    void SetTrendingEvents(List<EventCardComponentModel> events);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetGoingEvents(List<EventCardComponentModel> events);
    void ShowEventModal(EventCardComponentModel eventInfo);
    void ShowMoreUpcomingEvents();
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GridContainerComponentView trendingEvents;
    [SerializeField] internal TMP_Text trendingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView upcomingEvents;
    [SerializeField] internal TMP_Text upcomingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView goingEvents;
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
        featuredEvents.gameObject.SetActive(events.Count != 0);
    }

    public void SetTrendingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        trendingEvents.SetItems(eventComponentsToAdd, false);
        trendingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetUpcomingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        upcomingEvents.SetItems(eventComponentsToAdd, false);
        upcomingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetGoingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEvents(events, eventCardPrefab);
        goingEvents.SetItems(eventComponentsToAdd, false);
        goingEventsNoDataText.gameObject.SetActive(events.Count == 0);
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