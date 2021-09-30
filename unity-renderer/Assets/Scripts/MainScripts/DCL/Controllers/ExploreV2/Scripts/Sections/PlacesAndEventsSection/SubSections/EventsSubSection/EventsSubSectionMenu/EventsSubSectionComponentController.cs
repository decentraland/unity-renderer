using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    event Action OnEventDetailOpen;
    event Action<EventCardComponentModel> OnEventDetailLoaded;

    void SetFeatureEvents(List<EventCardComponentModel> events);
    void SetTrendingEvents(List<EventCardComponentModel> events);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetGoingEvents(List<EventCardComponentModel> events);
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    public event Action OnEventDetailOpen;
    public event Action<EventCardComponentModel> OnEventDetailLoaded;

    internal IEventsSubSectionComponentView view;
    internal EventsSubSectionData mockedData;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view)
    {
        mockedData = GameObject.Instantiate(Resources.Load<EventsSubSectionData>("MockedData/ExploreV2EventsSubSectionData"));

        this.view = view;
        this.view.OnReady += LoadEventsMockedData;
    }

    public void SetFeatureEvents(List<EventCardComponentModel> events) { view.SetFeatureEvents(events); }

    public void SetTrendingEvents(List<EventCardComponentModel> events) { view.SetTrendingEvents(events); }

    public void SetUpcomingEvents(List<EventCardComponentModel> events) { view.SetUpcomingEvents(events); }

    public void SetGoingEvents(List<EventCardComponentModel> events) { view.SetGoingEvents(events); }

    public void Dispose() { view.OnReady -= LoadEventsMockedData; }

    // Temporal until connect with the events API
    private void LoadEventsMockedData()
    {
        SetFeatureEvents(mockedData.featureEvents);
        SetTrendingEvents(mockedData.trendingEvents);
        SetUpcomingEvents(mockedData.upcomingEvents);
        SetGoingEvents(mockedData.goingEvents);
    }
}