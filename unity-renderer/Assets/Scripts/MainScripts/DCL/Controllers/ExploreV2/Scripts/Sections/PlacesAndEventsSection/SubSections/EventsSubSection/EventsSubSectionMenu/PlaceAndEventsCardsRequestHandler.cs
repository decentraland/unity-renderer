using DCL;
using System;
using UnityEngine;

public enum SubSectionType
{
    Highlights,
    Events,
    Places,
}

public interface ISubSectionComponentView
{
    void RestartScrollViewPosition();

    void SetAllAsLoading();

    void SetShowMoreButtonActive(bool isActive);

    int CurrentTilesPerRow { get; }
}

public class PlaceAndEventsCardsRequestHandler : IDisposable
{
    private readonly ISubSectionComponentView view;
    private readonly DataStore_ExploreV2 exploreV2Menu;

    private readonly int initialNumberOfRows;
    private readonly Action requestAllFromAPI;

    private bool firstLoading;
    private bool reloadSubSection;
    private bool isWaitingAnimTransition;

    private float lastTimeAPIChecked;

    public int CurrentCardsShown { get; set; }

    public PlaceAndEventsCardsRequestHandler(ISubSectionComponentView view, DataStore_ExploreV2 exploreV2Menu, int initialNumberOfRows, Action requestAllFromAPI)
    {
        this.view = view;
        this.exploreV2Menu = exploreV2Menu;

        this.initialNumberOfRows = initialNumberOfRows;
        this.requestAllFromAPI = requestAllFromAPI;

        firstLoading = true;
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

    private void OnExploreV2Open(bool current, bool _)
    {
        if (!current)
            reloadSubSection = true;
    }

    public void RequestAll()
    {
        if (firstLoading)
        {
            firstLoading = false;
            reloadSubSection = true;
            lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        }
        else if (!reloadSubSection || lastTimeAPIChecked < Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API)
            return;

        view.RestartScrollViewPosition();

        CurrentCardsShown = view.CurrentTilesPerRow * initialNumberOfRows;
        view.SetAllAsLoading();
        view.SetShowMoreButtonActive(false);

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
