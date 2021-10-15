using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IEventsSubSectionComponentView
{
    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMoreUpcomingEventsClicked;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnEventsSubSectionEnable;

    /// <summary>
    /// Set the featured events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetFeaturedEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the featured events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetFeaturedEventsAsLoading(bool isVisible);

    /// <summary>
    /// Set the trending events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetTrendingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the trending events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetTrendingEventsAsLoading(bool isVisible);

    /// <summary>
    /// Set the upcoming events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetUpcomingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the upcoming events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetUpcomingEventsAsLoading(bool isVisible);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreUpcomingEventsButtonActive(bool isActive);

    /// <summary>
    /// Set the going events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetGoingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the going events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetGoingEventsAsLoading(bool isVisible);

    /// <summary>
    /// Shows the Event Card modal with the provided information.
    /// </summary>
    /// <param name="eventInfo">Event (model) to be loaded in the card.</param>
    void ShowEventModal(EventCardComponentModel eventInfo);

    /// <summary>
    /// Hides the Event Card modal.
    /// </summary>
    void HideEventModal();
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    internal const string FEATURED_EVENT_CARDS_POOL_NAME = "FeaturedEventCardsPool";
    internal const string TRENDING_EVENT_CARDS_POOL_NAME = "TrendingEventCardsPool";
    internal const string UPCOMING_EVENT_CARDS_POOL_NAME = "UpcomingEventCardsPool";
    internal const string GOING_EVENT_CARDS_POOL_NAME = "FeatureGoingEventCardsPool";

    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GameObject featuredEventsLoading;
    [SerializeField] internal GridContainerComponentView trendingEvents;
    [SerializeField] internal GameObject trendingEventsLoading;
    [SerializeField] internal TMP_Text trendingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView upcomingEvents;
    [SerializeField] internal GameObject upcomingEventsLoading;
    [SerializeField] internal TMP_Text upcomingEventsNoDataText;
    [SerializeField] internal GridContainerComponentView goingEvents;
    [SerializeField] internal GameObject goingEventsLoading;
    [SerializeField] internal TMP_Text goingEventsNoDataText;
    [SerializeField] internal GameObject showMoreUpcomingEventsButtonContainer;
    [SerializeField] internal ButtonComponentView showMoreUpcomingEventsButton;

    public event Action OnReady;
    public event Action OnShowMoreUpcomingEventsClicked;
    public event Action OnEventsSubSectionEnable;

    internal EventCardComponentView eventModal;
    internal Pool featuredEventCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool upcomingEventCardsPool;
    internal Pool goingEventCardsPool;

    private void OnEnable() { OnEventsSubSectionEnable?.Invoke(); }

    public override void PostInitialization()
    {
        ConfigureEventCardModal();
        ConfigureEventCardsPool(out featuredEventCardsPool, FEATURED_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, 10);
        ConfigureEventCardsPool(out trendingEventCardsPool, TRENDING_EVENT_CARDS_POOL_NAME, eventCardPrefab, 100);
        ConfigureEventCardsPool(out upcomingEventCardsPool, UPCOMING_EVENT_CARDS_POOL_NAME, eventCardPrefab, 100);
        ConfigureEventCardsPool(out goingEventCardsPool, GOING_EVENT_CARDS_POOL_NAME, eventCardPrefab, 100);
        StartCoroutine(WaitForComponentsInitialization());
    }

    internal IEnumerator WaitForComponentsInitialization()
    {
        yield return new WaitUntil(() => featuredEvents.isFullyInitialized &&
                                         trendingEvents.isFullyInitialized &&
                                         upcomingEvents.isFullyInitialized &&
                                         goingEvents.isFullyInitialized &&
                                         showMoreUpcomingEventsButton.isFullyInitialized);

        featuredEvents.RemoveItems();
        trendingEvents.RemoveItems();
        upcomingEvents.RemoveItems();
        goingEvents.RemoveItems();

        showMoreUpcomingEventsButton.onClick.RemoveAllListeners();
        showMoreUpcomingEventsButton.onClick.AddListener(() => OnShowMoreUpcomingEventsClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void RefreshControl()
    {
        featuredEvents.RefreshControl();
        trendingEvents.RefreshControl();
        upcomingEvents.RefreshControl();
        goingEvents.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        showMoreUpcomingEventsButton.onClick.RemoveAllListeners();
    }

    public void SetFeaturedEvents(List<EventCardComponentModel> events)
    {
        featuredEvents.ExtractItems();
        featuredEventCardsPool.ReleaseAll();
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEventCards(events, featuredEventCardsPool);
        featuredEvents.SetItems(eventComponentsToAdd, false);
        featuredEvents.gameObject.SetActive(events.Count > 0);
    }
    public void SetFeaturedEventsAsLoading(bool isVisible)
    {
        featuredEvents.gameObject.SetActive(!isVisible);
        featuredEventsLoading.SetActive(isVisible);
    }

    public void SetTrendingEvents(List<EventCardComponentModel> events)
    {
        trendingEvents.ExtractItems();
        trendingEventCardsPool.ReleaseAll();
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEventCards(events, trendingEventCardsPool);
        trendingEvents.SetItems(eventComponentsToAdd, false);
        trendingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetTrendingEventsAsLoading(bool isVisible)
    {
        trendingEvents.gameObject.SetActive(!isVisible);
        trendingEventsLoading.SetActive(isVisible);

        if (isVisible)
            trendingEventsNoDataText.gameObject.SetActive(false);
    }

    public void SetUpcomingEvents(List<EventCardComponentModel> events)
    {
        upcomingEvents.ExtractItems();
        upcomingEventCardsPool.ReleaseAll();
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEventCards(events, upcomingEventCardsPool);
        upcomingEvents.SetItems(eventComponentsToAdd, false);
        upcomingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetUpcomingEventsAsLoading(bool isVisible)
    {
        upcomingEvents.gameObject.SetActive(!isVisible);
        upcomingEventsLoading.SetActive(isVisible);

        if (isVisible)
            upcomingEventsNoDataText.gameObject.SetActive(false);
    }

    public void SetGoingEvents(List<EventCardComponentModel> events)
    {
        goingEvents.ExtractItems();
        goingEventCardsPool.ReleaseAll();
        List<BaseComponentView> eventComponentsToAdd = IntantiateAndConfigureEventCards(events, goingEventCardsPool);
        goingEvents.SetItems(eventComponentsToAdd, false);
        goingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetGoingEventsAsLoading(bool isVisible)
    {
        goingEvents.gameObject.SetActive(!isVisible);
        goingEventsLoading.SetActive(isVisible);

        if (isVisible)
            goingEventsNoDataText.gameObject.SetActive(false);
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.gameObject.SetActive(true);
        eventModal.Configure(eventInfo);
    }

    public void HideEventModal() { eventModal.gameObject.SetActive(false); }

    public void SetShowMoreUpcomingEventsButtonActive(bool isActive) { showMoreUpcomingEventsButtonContainer.gameObject.SetActive(isActive); }

    internal void ConfigureEventCardModal()
    {
        eventModal = GameObject.Instantiate(eventCardModalPrefab);
        eventModal.gameObject.SetActive(false);
    }

    internal void ConfigureEventCardsPool(out Pool pool, string poolName, EventCardComponentView eventCardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);
        if (pool == null)
        {
            pool = PoolManager.i.AddPool(
                poolName,
                Instantiate(eventCardPrefab).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);
        }
    }

    internal List<BaseComponentView> IntantiateAndConfigureEventCards(List<EventCardComponentModel> events, Pool pool)
    {
        List<BaseComponentView> instantiatedEvents = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = pool.Get().gameObject.GetComponent<EventCardComponentView>();
            eventGO.Configure(eventInfo);
            instantiatedEvents.Add(eventGO);
        }

        return instantiatedEvents;
    }
}