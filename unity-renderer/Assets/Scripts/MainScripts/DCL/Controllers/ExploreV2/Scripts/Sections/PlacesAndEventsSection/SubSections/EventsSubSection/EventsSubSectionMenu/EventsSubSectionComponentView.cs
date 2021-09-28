using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentView
{
    event Action OnFeatureEventsComponentReady;
    event Action OnUpcomingEventsComponentReady;

    EventCardComponentView currentFeatureEventPrefab { get; }
    EventCardComponentView currentEventPrefab { get; }
    ICarouselComponentView currentFeaturedEvents { get; }
    IGridContainerComponentView currentUpcomingEvents { get; }
}

public class EventsSubSectionComponentView : MonoBehaviour, IEventsSubSectionComponentView
{
    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView featureEventPrefab;
    [SerializeField] internal EventCardComponentView eventPrefab;

    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GridContainerComponentView upcomingEvents;

    public event Action OnFeatureEventsComponentReady;
    public event Action OnUpcomingEventsComponentReady;

    public EventCardComponentView currentFeatureEventPrefab => featureEventPrefab;
    public EventCardComponentView currentEventPrefab => eventPrefab;
    public ICarouselComponentView currentFeaturedEvents => featuredEvents;
    public IGridContainerComponentView currentUpcomingEvents => upcomingEvents;

    private void Awake()
    {
        featuredEvents.OnFullyInitialized += OnFeatureEventsComponentInitialized;
        upcomingEvents.OnFullyInitialized += UpcomingEvents_OnFullyInitialized;
    }

    private void OnDestroy()
    {
        featuredEvents.OnFullyInitialized -= OnFeatureEventsComponentInitialized;
        upcomingEvents.OnFullyInitialized -= UpcomingEvents_OnFullyInitialized;
    }

    private void OnFeatureEventsComponentInitialized() { OnFeatureEventsComponentReady?.Invoke(); }

    private void UpcomingEvents_OnFullyInitialized() { OnUpcomingEventsComponentReady?.Invoke(); }
}