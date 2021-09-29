using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentView
{
    event Action OnReady;

    void SetFeatureEvents(List<EventCardComponentModel> events);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetGoingEvents(List<EventCardComponentModel> events);
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardPrefab;

    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GridContainerComponentView upcomingEvents;
    [SerializeField] internal GridContainerComponentView goingEvents;

    public event Action OnReady;

    public override void PostInitialization() { StartCoroutine(RefreshControlAfterAFrame()); }

    public override void RefreshControl()
    {
        featuredEvents.RefreshControl();
        upcomingEvents.RefreshControl();
        goingEvents.RefreshControl();
    }

    private IEnumerator RefreshControlAfterAFrame()
    {
        yield return null;
        RefreshControl();
        OnReady?.Invoke();
    }

    public void SetFeatureEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateEvents(events, eventCardLongPrefab);
        featuredEvents.SetItems(eventComponentsToAdd);
    }

    public void SetUpcomingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateEvents(events, eventCardPrefab);
        upcomingEvents.SetItems(eventComponentsToAdd);
    }

    public void SetGoingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = IntantiateEvents(events, eventCardPrefab);
        goingEvents.SetItems(eventComponentsToAdd);
    }

    internal List<BaseComponentView> IntantiateEvents(List<EventCardComponentModel> events, EventCardComponentView prefabToUse)
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