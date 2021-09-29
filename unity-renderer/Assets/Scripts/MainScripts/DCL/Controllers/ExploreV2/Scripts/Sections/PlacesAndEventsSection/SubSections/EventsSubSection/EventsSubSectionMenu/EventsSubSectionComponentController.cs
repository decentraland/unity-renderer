using System.Collections.Generic;

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

    public EventsSubSectionComponentController(IEventsSubSectionComponentView view) { this.view = view; }

    public void SetFeatureEvents(List<EventCardComponentModel> events) { view.SetFeatureEvents(events); }

    public void SetUpcomingEvents(List<EventCardComponentModel> events) { view.SetUpcomingEvents(events); }

    public void SetGoingEvents(List<EventCardComponentModel> events) { view.SetGoingEvents(events); }

    public void Dispose() { }
}