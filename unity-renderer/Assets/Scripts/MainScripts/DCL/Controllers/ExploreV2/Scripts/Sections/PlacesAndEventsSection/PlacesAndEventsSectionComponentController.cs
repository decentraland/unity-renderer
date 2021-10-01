public interface IPlacesAndEventsSectionComponentController
{
    void Dispose();
}

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    internal IPlacesAndEventsSectionComponentView view;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    public PlacesAndEventsSectionComponentController(IPlacesAndEventsSectionComponentView view)
    {
        this.view = view;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(view.currentEventsSubSectionComponentView);
    }

    public void Dispose() { eventsSubSectionComponentController.Dispose(); }
}