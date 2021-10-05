using System;

public interface IPlacesAndEventsSectionComponentController : IDisposable { }

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    internal IPlacesAndEventsSectionComponentView view;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    public PlacesAndEventsSectionComponentController(IPlacesAndEventsSectionComponentView view)
    {
        this.view = view;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.currentEventsSubSectionComponentView,
            new EventsAPIController());
    }

    public void Dispose() { eventsSubSectionComponentController.Dispose(); }
}