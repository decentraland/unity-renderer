using DCL;
using System;
using UnityEngine;

public class PlaceAndEventsCardsReloader : IDisposable
{
    private readonly IPlacesAndEventsSubSectionComponentView view;
    private readonly IPlacesAndEventsAPIRequester requester;
    private readonly DataStore_ExploreV2 exploreV2Menu;

    internal bool firstLoading = true;
    internal bool reloadSubSection;
    private bool isWaitingAnimTransition;

    internal float lastTimeAPIChecked;

    public PlaceAndEventsCardsReloader(IPlacesAndEventsSubSectionComponentView view, IPlacesAndEventsAPIRequester requester, DataStore_ExploreV2 exploreV2Menu)
    {
        this.view = view;
        this.exploreV2Menu = exploreV2Menu;
        this.requester = requester;
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
        if (!firstLoading)
            return reloadSubSection && !IsInCooldown();

        reloadSubSection = true;
        lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;

        return true;

        bool IsInCooldown() => lastTimeAPIChecked < Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
    }

    public void RequestAll()
    {
        view.RestartScrollViewPosition();
        view.SetAllAsLoading();

        reloadSubSection = false;
        lastTimeAPIChecked = Time.realtimeSinceStartup;

        if (!exploreV2Menu.isInShowAnimationTransiton.Get())
            requester.RequestAllFromAPI();
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

        requester.RequestAllFromAPI();
    }
}

public interface IPlacesAndEventsAPIRequester
{
    void RequestAllFromAPI();
}
