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
    internal IHighlightsSubSectionComponentController highlightsSubSectionComponentController;
    internal IPlacesSubSectionComponentController placesSubSectionComponentController;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    public PlacesAndEventsSectionComponentController(IPlacesAndEventsSectionComponentView view, IExploreV2Analytics exploreV2Analytics)
    {
        this.view = view;

        PlacesAPIController placesAPI = new PlacesAPIController();
        EventsAPIController eventsAPI = new EventsAPIController();

        highlightsSubSectionComponentController = new HighlightsSubSectionComponentController(
            view.currentHighlightsSubSectionComponentView,
            placesAPI,
            eventsAPI,
            FriendsController.i,
            exploreV2Analytics);

        highlightsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        placesSubSectionComponentController = new PlacesSubSectionComponentController(
            view.currentPlacesSubSectionComponentView,
            placesAPI,
            FriendsController.i,
            exploreV2Analytics);

        placesSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.currentEventsSubSectionComponentView,
            eventsAPI,
            exploreV2Analytics);

        eventsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        view.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
        highlightsSubSectionComponentController.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
        placesSubSectionComponentController.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
        eventsSubSectionComponentController.OnAnyActionExecuted += OnAnyActionExecutedInAnySubSection;
    }

    internal void RequestExploreV2Closing() { OnCloseExploreV2?.Invoke(); }

    internal void OnAnyActionExecutedInAnySubSection() { OnAnyActionExecuted?.Invoke(); }

    public void Dispose()
    {
        view.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;

        highlightsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        highlightsSubSectionComponentController.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;
        highlightsSubSectionComponentController.Dispose();

        placesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        placesSubSectionComponentController.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;
        placesSubSectionComponentController.Dispose();

        eventsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        eventsSubSectionComponentController.OnAnyActionExecuted -= OnAnyActionExecutedInAnySubSection;
        eventsSubSectionComponentController.Dispose();
    }
}