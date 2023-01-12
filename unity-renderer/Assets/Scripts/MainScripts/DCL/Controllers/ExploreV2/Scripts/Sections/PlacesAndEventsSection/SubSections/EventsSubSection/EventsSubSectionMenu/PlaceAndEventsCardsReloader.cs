using DCL;
using System;
using UnityEngine;

public class PlaceAndEventsCardsReloader : IDisposable
{
    private readonly IPlacesAndEventsSubSectionComponentView view;
    private readonly DataStore_ExploreV2 exploreV2Menu;

    private readonly Action requestAllFromAPI;

    internal bool firstLoading = true;
    internal bool reloadSubSection;
    private bool isWaitingAnimTransition;

    internal float lastTimeAPIChecked;

    public PlaceAndEventsCardsReloader(IPlacesAndEventsSubSectionComponentView view, DataStore_ExploreV2 exploreV2Menu, Action requestAllFromAPI)
    {
        this.view = view;
        this.exploreV2Menu = exploreV2Menu;

        this.requestAllFromAPI = requestAllFromAPI;
    }

    public void Initialize()
    {
        exploreV2Menu.isOpen.OnChange += OnExploreV2Open;
    }

    public void Dispose()
    {
        exploreV2Menu.isOpen.OnChange -= OnExploreV2Open;

        if (isWaitingAnimTransition)
            exploreV2Menu.isInShowAnimationTransiton.OnChange -= OnAnimationTransitionFinished;
    }

    internal void OnExploreV2Open(bool current, bool _)
    {
        if (!current)
            reloadSubSection = true;
    }

    public bool CanReload()
    {
        if (firstLoading)
        {
            firstLoading = false;
            reloadSubSection = true;
            lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;

            return true;
        }

        if (!reloadSubSection || lastTimeAPIChecked < Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API)
            return false;

        return true;
    }

    public void RequestAll()
    {
        view.RestartScrollViewPosition();
        view.SetAllAsLoading();

        reloadSubSection = false;
        lastTimeAPIChecked = Time.realtimeSinceStartup;

        if (!exploreV2Menu.isInShowAnimationTransiton.Get())
            requestAllFromAPI();
        else
        {
            exploreV2Menu.isInShowAnimationTransiton.OnChange += OnAnimationTransitionFinished;
            isWaitingAnimTransition = true;
        }
    }

    private void OnAnimationTransitionFinished(bool _, bool __)
    {
        exploreV2Menu.isInShowAnimationTransiton.OnChange -= OnAnimationTransitionFinished;
        isWaitingAnimTransition = false;

        requestAllFromAPI();
    }
}
