using DCL;
using ExploreV2Analytics;
using System;
using DCL.Social.Friends;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using MainScripts.DCL.Controllers.HotScenes;
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
    internal IPlacesSubSectionComponentController placesSubSectionComponentController;
    internal IWorldsSubSectionComponentController worldsSubSectionComponentController;
    internal IEventsSubSectionComponentController eventsSubSectionComponentController;
    internal IFavoritesSubSectionComponentController favoritesSubSectionComponentController;
    internal ISearchSubSectionComponentController searchSubSectionComponentController;
    private DataStore dataStore;
    private static Service<IHotScenesFetcher> hotScenesFetcher;

    internal BaseVariable<bool> placesAndEventsVisible => dataStore.exploreV2.placesAndEventsVisible;

    public PlacesAndEventsSectionComponentController(
        IPlacesAndEventsSectionComponentView view,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        IPlacesAPIService placesAPIService,
        IWorldsAPIService worldsAPIService,
        IPlacesAnalytics placesAnalytics
        )
    {
        this.view = view;
        this.dataStore = dataStore;

        EventsAPIController eventsAPI = new EventsAPIController();

        placesSubSectionComponentController = new PlacesSubSectionComponentController(
            view.PlacesSubSectionView,
            placesAPIService,
            friendsController,
            exploreV2Analytics,
            placesAnalytics,
            dataStore,
            userProfileBridge);
        placesSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        worldsSubSectionComponentController = new WorldsSubSectionComponentController(
            view.WorldsSubSectionView,
            placesAPIService,
            worldsAPIService,
            friendsController,
            exploreV2Analytics,
            placesAnalytics,
            dataStore,
            userProfileBridge);
        worldsSubSectionComponentController.OnCloseExploreV2 += RequestExploreV2Closing;

        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            view.EventsSubSectionView,
            eventsAPI,
            exploreV2Analytics,
            dataStore,
            userProfileBridge,
            placesAPIService,
            worldsAPIService);
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
            worldsAPIService,
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
        placesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        placesSubSectionComponentController.Dispose();

        worldsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        worldsSubSectionComponentController.Dispose();

        eventsSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        eventsSubSectionComponentController.Dispose();

        favoritesSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;
        favoritesSubSectionComponentController.Dispose();

        searchSubSectionComponentController.OnCloseExploreV2 -= RequestExploreV2Closing;

        placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
    }

    internal void PlacesAndEventsVisibleChanged(bool current, bool _)
    {
        if (current && hotScenesFetcher.Ref != null)
            hotScenesFetcher.Ref.SetUpdateMode(IHotScenesFetcher.UpdateMode.IMMEDIATELY_ONCE);

        view.EnableSearchBar(dataStore.featureFlags.flags.Get().IsFeatureEnabled("search_in_places"));
        view.SetActive(current);
    }
}
