using DCL;
using ExploreV2Analytics;
using System;
using DCL.Social.Friends;
using DCLServices.PlacesAPIService;
using Environment = DCL.Environment;

public interface IPlacesAndEventsSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action<bool> OnCloseExploreV2;
}

public class PlacesAndEventsSectionComponentController : IPlacesAndEventsSectionComponentController
{
    internal const float MIN_TIME_TO_CHECK_API = 60f;

    public event Action<bool> OnCloseExploreV2;

    internal IPlacesAndEventsSectionComponentView view;
    internal IHighlightsSubSectionComponentController highlightsSubSectionComponentController;
    internal IPlacesSubSectionComponentController placesSubSectionComponentController;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;
    internal IFavoritesSubSectionComponentController favoritesSubSectionComponentController;
    internal ISearchSubSectionComponentController searchSubSectionComponentController;
    private DataStore dataStore;

    internal BaseVariable<bool> placesAndEventsVisible => dataStore.exploreV2.placesAndEventsVisible;

    public PlacesAndEventsSectionComponentController(
        IPlacesAndEventsSectionComponentView view,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IPlacesAPIService placesAPIService,
        IPlacesAnalytics placesAnalytics
        )
    {
        this.view = view;
        this.dataStore = dataStore;

        EventsAPIController eventsAPI = new EventsAPIController();

        highlightsSubSectionComponentController = new HighlightsSubSectionComponentController(
            view.HighlightsSubSectionView,
            placesAPIService,
            eventsAPI,
            friendsController,
            exploreV2Analytics,
            placesAnalytics,
            dataStore);
        highlightsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;
        highlightsSubSectionComponentController.OnGoToEventsSubSection += GoToEventsSubSection;

        placesSubSectionComponentController = new PlacesSubSectionComponentController(
            view.PlacesSubSectionView,
            placesAPIService,
            friendsController,
            exploreV2Analytics,
            placesAnalytics,
            dataStore);
        placesSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.EventsSubSectionView,
            eventsAPI,
            exploreV2Analytics,
            dataStore,
            userProfileBridge);
        eventsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        favoritesSubSectionComponentController = new FavoritesesSubSectionComponentController(
            view.FavoritesSubSectionView,
            placesAPIService,
            friendsController,
            exploreV2Analytics,
            placesAnalytics,
            dataStore);
        favoritesSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        searchSubSectionComponentController = new SearchSubSectionComponentController(
            view.SearchSubSectionView,
            view.SearchBar,
            eventsAPI,
            placesAPIService,
            userProfileBridge,
            exploreV2Analytics,
            placesAnalytics,
            dataStore);
        searchSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

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

        favoritesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        favoritesSubSectionComponentController.Dispose();

        searchSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;

        placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
    }

    internal void PlacesAndEventsVisibleChanged(bool current, bool _)
    {
        view.EnableSearchBar(dataStore.featureFlags.flags.Get().IsFeatureEnabled("search_in_places"));
        view.SetActive(current);
    }
}
