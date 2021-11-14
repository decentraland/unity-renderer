using ExploreV2Analytics;
using System;

public interface IPlacesAndEventsSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// It will be triggered when any action is executed inside the places and events section.
    /// </summary>
    event Action OnAnyActionExecuted;
}

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    public event Action OnCloseExploreV2;
    public event Action OnAnyActionExecuted;

    internal IPlacesAndEventsSectionComponentView view;
    internal IPlacesSubSectionComponentController placesSubSectionComponentController;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    public PlacesAndEventsSectionComponentController(IPlacesAndEventsSectionComponentView view, IExploreV2Analytics exploreV2Analytics)
    {
        this.view = view;

        placesSubSectionComponentController = new PlacesSubSectionComponentController(
            view.currentPlacesSubSectionComponentView,
            new PlacesAPIController(),
            FriendsController.i,
            exploreV2Analytics);

        placesSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.currentEventsSubSectionComponentView,
            new EventsAPIController(),
            exploreV2Analytics);

        eventsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        view.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
        placesSubSectionComponentController.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
        eventsSubSectionComponentController.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
    }

    internal void RequestExploreV2Closing() { OnCloseExploreV2?.Invoke(); }

    internal void OnAnyActionExecutedInAnySubSection() { OnAnyActionExecuted?.Invoke(); }

    public void Dispose()
    {
        view.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;

        placesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        placesSubSectionComponentController.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;
        placesSubSectionComponentController.Dispose();

        eventsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        eventsSubSectionComponentController.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;
        eventsSubSectionComponentController.Dispose();
    }
}