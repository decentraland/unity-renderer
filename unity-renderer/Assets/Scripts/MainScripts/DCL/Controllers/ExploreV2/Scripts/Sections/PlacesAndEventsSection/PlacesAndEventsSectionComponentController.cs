using DCL;
using ExploreV2Analytics;
using System;

public interface IPlacesAndEventsSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action<bool> OnCloseExploreV2;
}

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    public event Action<bool> OnCloseExploreV2;

    internal IPlacesAndEventsSectionComponentView view;
    internal IHighlightsSubSectionComponentController highlightsSubSectionComponentController;
    internal IPlacesSubSectionComponentController placesSubSectionComponentController;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;

    internal BaseVariable<bool> placesAndEventsVisible => DataStore.i.exploreV2.placesAndEventsVisible;

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
        highlightsSubSectionComponentController.OnGoToEventsSubSection += GoToEventsSubSection;

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

        placesAndEventsVisible.OnChange += PlacesAndEventsVisibleChanged;
        PlacesAndEventsVisibleChanged(placesAndEventsVisible.Get(), false);
    }

    internal void RequestExploreV2Closing() { OnCloseExploreV2?.Invoke(false); }

    internal void GoToEventsSubSection() { view.GoToSubsection(PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX); }

    public void Dispose()
    {
        highlightsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        highlightsSubSectionComponentController.OnGoToEventsSubSection -= GoToEventsSubSection;
        highlightsSubSectionComponentController.Dispose();

        placesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        placesSubSectionComponentController.Dispose();

        eventsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        eventsSubSectionComponentController.Dispose();

        placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
    }

    internal void PlacesAndEventsVisibleChanged(bool current, bool previous) { view.SetActive(current); }
}