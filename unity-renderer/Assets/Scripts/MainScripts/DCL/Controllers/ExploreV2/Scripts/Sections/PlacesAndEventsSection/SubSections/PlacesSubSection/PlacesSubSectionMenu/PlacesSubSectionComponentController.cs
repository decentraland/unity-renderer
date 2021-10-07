using DCL;
using System;
using System.Collections.Generic;

public interface IPlacesSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all places from the API.
    /// </summary>
    void RequestAllPlaces();

    /// <summary>
    /// Load the places with the last requested ones.
    /// </summary>
    void LoadPlaces();
}

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    internal event Action OnPlacesFromAPIUpdated;

    internal IPlacesSubSectionComponentView view;
    internal bool reloadPlaces = false;

    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;

        OnPlacesFromAPIUpdated += OnRequestedPlacesUpdated;
    }

    private void FirstLoading()
    {
        reloadPlaces = true;
        RequestAllPlaces();

        view.OnPlacesSubSectionEnable += RequestAllPlaces;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    private void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadPlaces = true;
    }

    public void RequestAllPlaces()
    {
        // TODO: Remove it!
        return;

        if (!reloadPlaces)
            return;

        view.SetPlacesAsLoading(true);

        RequestAllPlacesFromAPI();

        reloadPlaces = false;
    }

    internal void RequestAllPlacesFromAPI()
    {
        // TODO: Implement the call to the API...
        OnPlacesFromAPIUpdated?.Invoke();
    }

    internal void OnRequestedPlacesUpdated() { LoadPlaces(); }

    public void LoadPlaces()
    {
        view.SetPlaces(new List<PlaceCardComponentModel>());

        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        // TODO: Implement the loading from the API's received data...

        view.SetPlacesAsLoading(false);
        view.SetPlaces(places);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnPlacesSubSectionEnable -= RequestAllPlaces;
        OnPlacesFromAPIUpdated -= OnRequestedPlacesUpdated;
        DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
    }
}