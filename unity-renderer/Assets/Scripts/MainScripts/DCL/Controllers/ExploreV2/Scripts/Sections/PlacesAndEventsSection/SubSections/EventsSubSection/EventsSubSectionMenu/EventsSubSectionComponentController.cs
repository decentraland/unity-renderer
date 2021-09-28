using System.Collections.Generic;
using UnityEngine;

public interface IEventsSubSectionComponentController
{
    void Dispose();
}

public class EventsSubSectionComponentController : IEventsSubSectionComponentController
{
    internal IEventsSubSectionComponentView view;

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view)
    {
        this.view = view;

        this.view.OnFeatureEventsComponentReady += MockFeatureEvents;
    }

    private void MockFeatureEvents()
    {
        //SetFeatureEvents(new List<EventCardComponentModel>
        //{
        //    new EventCardComponentModel
        //    {
        //        eventName = "SantiTest1"
        //    },
        //    new EventCardComponentModel
        //    {
        //        eventName = "SantiTest2"
        //    }
        //});
    }

    public void SetFeatureEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventCardsToAdd = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = GameObject.Instantiate(view.currentFeatureEventPrefab);
            eventGO.name = eventInfo.eventName;
            eventGO.Configure(eventInfo);
            eventCardsToAdd.Add(eventGO);
        }

        view.currentFeaturedEvents.SetItems(eventCardsToAdd);
    }

    public void Dispose() { view.OnFeatureEventsComponentReady -= MockFeatureEvents; }
}