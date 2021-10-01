using DCL;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    void SetFeatureEvents(List<EventCardComponentModel> events);
    void SetTrendingEvents(List<EventCardComponentModel> events);
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
        mockedData = GameObject.Instantiate(Resources.Load<EventsSubSectionData>("MockedData/ExploreV2EventsSubSectionMockedData"));

        this.view = view;
        this.view.OnReady += SetEventsMockedData;
    }

    public void SetFeatureEvents(List<EventCardComponentModel> events) { view.SetFeatureEvents(events); }

    public void SetTrendingEvents(List<EventCardComponentModel> events) { view.SetTrendingEvents(events); }

    public void SetUpcomingEvents(List<EventCardComponentModel> events) { view.SetUpcomingEvents(events); }

    public void SetGoingEvents(List<EventCardComponentModel> events) { view.SetGoingEvents(events); }

    public void Dispose() { view.OnReady -= SetEventsMockedData; }

    // Temporal until connect with the events API
    private void SetEventsMockedData()
    {
        foreach (var eventCardModel in mockedData.featureEvents)
        {
            ConfigureEventCardButtons(eventCardModel);
        }
        SetFeatureEvents(mockedData.featureEvents);

        foreach (var eventCardModel in mockedData.trendingEvents)
        {
            ConfigureEventCardButtons(eventCardModel);
        }
        SetTrendingEvents(mockedData.trendingEvents);

        foreach (var eventCardModel in mockedData.upcomingEvents)
        {
            ConfigureEventCardButtons(eventCardModel);
        }
        SetUpcomingEvents(mockedData.upcomingEvents);

        foreach (var eventCardModel in mockedData.goingEvents)
        {
            ConfigureEventCardButtons(eventCardModel);
        }
        SetGoingEvents(mockedData.goingEvents);
    }

    // Temporal until connect with the events API
    private void ConfigureEventCardButtons(EventCardComponentModel eventCardModel)
    {
        eventCardModel.onInfoClick.AddListener(() => view.ShowEventModal(eventCardModel));
        eventCardModel.onSubscribeClick.AddListener(() => Debug.Log("SUBSCRIBED!"));
        eventCardModel.onUnsubscribeClick.AddListener(() => Debug.Log("UNSUBSCRIBED!"));
        eventCardModel.onJumpInClick.AddListener(() => DataStore.i.exploreV2.isOpen.Set(false));
    }
}