using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    void SetFeatureEvents(List<EventCardComponentModel> events);
    void SetUpcomingEvents(List<EventCardComponentModel> events);
    void SetGoingEvents(List<EventCardComponentModel> events);
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    internal IEventsSubSectionComponentView view;
    internal EventsSubSectionData mockedData;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view)
    {
        mockedData = GameObject.Instantiate(Resources.Load<EventsSubSectionData>("MockedData/ExploreV2EventsSubSectionData"));

        this.view = view;
        this.view.OnReady += LoadEventsMockedData;
    }

    public void SetFeatureEvents(List<EventCardComponentModel> events) { view.SetFeatureEvents(events); }

    public void SetUpcomingEvents(List<EventCardComponentModel> events) { view.SetUpcomingEvents(events); }

    public void SetGoingEvents(List<EventCardComponentModel> events) { view.SetGoingEvents(events); }

    public void Dispose() { view.OnReady -= LoadEventsMockedData; }

    // Temporal until connect with the events API
    private void LoadEventsMockedData()
    {
        SetFeatureEvents(mockedData.featureEvents);
        SetUpcomingEvents(mockedData.upcomingEvents);
        SetGoingEvents(mockedData.goingEvents);
    }
}