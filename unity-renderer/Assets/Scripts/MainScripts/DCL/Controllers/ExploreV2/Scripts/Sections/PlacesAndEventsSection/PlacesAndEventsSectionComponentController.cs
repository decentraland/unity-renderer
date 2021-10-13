using System;

public interface IPlacesAndEventsSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;
}

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    public event Action OnCloseExploreV2;

    internal IPlacesAndEventsSectionComponentView view;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    public PlacesAndEventsSectionComponentController(IPlacesAndEventsSectionComponentView view)
    {
        this.view = view;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.currentEventsSubSectionComponentView,
            new EventsAPIController());

        eventsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;
    }

    private void RequestExploreV2Closing() { OnCloseExploreV2?.Invoke(); }

    public void Dispose()
    {
        eventsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        eventsSubSectionComponentController.Dispose();
    }
}