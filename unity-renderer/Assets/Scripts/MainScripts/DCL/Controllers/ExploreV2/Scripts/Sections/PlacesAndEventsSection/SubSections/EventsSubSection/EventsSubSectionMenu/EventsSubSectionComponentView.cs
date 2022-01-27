using DCL;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEventsSubSectionComponentView
{
    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the info button is clicked.
    /// </summary>
    event Action<EventCardComponentModel> OnInfoClicked;

    /// <summary>
    /// It will be triggered when the JumpIn button is clicked.
    /// </summary>
    event Action<EventFromAPIModel> OnJumpInClicked;

    /// <summary>
    /// It will be triggered when the subscribe event button is clicked.
    /// </summary>
    event Action<string> OnSubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    event Action<string> OnUnsubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMoreUpcomingEventsClicked;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnEventsSubSectionEnable;

    /// <summary>
    /// Number of events per row that fit with the current upcoming events grid configuration.
    /// </summary>
    int currentUpcomingEventsPerRow { get; }

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
    /// Activates/deactivates the featured events component.
    /// </summary>
    /// <param name="isActive"></param>
    void SetFeaturedEventsActive(bool isActive);

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
    /// Add a list of events in the events component.
    /// </summary>
    /// <param name="places">List of events (model) to be added.</param>
    void AddUpcomingEvents(List<EventCardComponentModel> events);

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

    /// <summary>
    /// Set the current scroll view position to 1.
    /// </summary>
    void RestartScrollViewPosition();

    /// <summary>
    /// Configure the needed pools for the events instantiation.
    /// </summary>
    void ConfigurePools();
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    internal const string FEATURED_EVENT_CARDS_POOL_NAME = "Events_FeaturedEventCardsPool";
    internal const int FEATURED_EVENT_CARDS_POOL_PREWARM = 10;
    internal const string TRENDING_EVENT_CARDS_POOL_NAME = "Events_TrendingEventCardsPool";
    internal const int TRENDING_EVENT_CARDS_POOL_PREWARM = 12;
    internal const string UPCOMING_EVENT_CARDS_POOL_NAME = "Events_UpcomingEventCardsPool";
    internal const int UPCOMING_EVENT_CARDS_POOL_PREWARM = 9;
    internal const string GOING_EVENT_CARDS_POOL_NAME = "Events_FeatureGoingEventCardsPool";
    internal const int GOING_EVENT_CARDS_POOL_PREWARM = 9;

    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
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
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;
    public event Action OnShowMoreUpcomingEventsClicked;
    public event Action OnEventsSubSectionEnable;

    internal EventCardComponentView eventModal;
    internal Pool featuredEventCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool upcomingEventCardsPool;
    internal Pool goingEventCardsPool;

    public int currentUpcomingEventsPerRow => upcomingEvents.currentItemsPerRow;

    public override void OnEnable() { OnEventsSubSectionEnable?.Invoke(); }

    public void ConfigurePools()
    {
        ExploreEventsUtils.ConfigureEventCardsPool(out featuredEventCardsPool, FEATURED_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, FEATURED_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out trendingEventCardsPool, TRENDING_EVENT_CARDS_POOL_NAME, eventCardPrefab, TRENDING_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out upcomingEventCardsPool, UPCOMING_EVENT_CARDS_POOL_NAME, eventCardPrefab, UPCOMING_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out goingEventCardsPool, GOING_EVENT_CARDS_POOL_NAME, eventCardPrefab, GOING_EVENT_CARDS_POOL_PREWARM);
    }

    public override void Start()
    {
        eventModal = ExploreEventsUtils.ConfigureEventCardModal(eventCardModalPrefab);

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

        featuredEvents.Dispose();
        upcomingEvents.Dispose();
        trendingEvents.Dispose();
        goingEvents.Dispose();

        if (eventModal != null)
        {
            eventModal.Dispose();
            Destroy(eventModal.gameObject);
        }
    }

    public void SetFeaturedEvents(List<EventCardComponentModel> events)
    {
        featuredEvents.ExtractItems();
        featuredEventCardsPool.ReleaseAll();

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            featuredEventCardsPool,
            OnInfoClicked,
            OnJumpInClicked,
            OnSubscribeEventClicked,
            OnUnsubscribeEventClicked);

        featuredEvents.SetItems(eventComponentsToAdd);
        SetFeaturedEventsActive(events.Count > 0);
    }

    public void SetFeaturedEventsAsLoading(bool isVisible)
    {
        SetFeaturedEventsActive(!isVisible);
        featuredEventsLoading.SetActive(isVisible);
    }

    public void SetFeaturedEventsActive(bool isActive) { featuredEvents.gameObject.SetActive(isActive); }

    public void SetTrendingEvents(List<EventCardComponentModel> events)
    {
        trendingEvents.ExtractItems();
        trendingEventCardsPool.ReleaseAll();

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            trendingEventCardsPool,
            OnInfoClicked,
            OnJumpInClicked,
            OnSubscribeEventClicked,
            OnUnsubscribeEventClicked);

        trendingEvents.SetItems(eventComponentsToAdd);
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

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            upcomingEventCardsPool,
            OnInfoClicked,
            OnJumpInClicked,
            OnSubscribeEventClicked,
            OnUnsubscribeEventClicked);

        upcomingEvents.SetItems(eventComponentsToAdd);
        upcomingEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void AddUpcomingEvents(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            upcomingEventCardsPool,
            OnInfoClicked,
            OnJumpInClicked,
            OnSubscribeEventClicked,
            OnUnsubscribeEventClicked);

        foreach (var eventToAdd in eventComponentsToAdd)
        {
            upcomingEvents.AddItem(eventToAdd);
        }
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

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            goingEventCardsPool,
            OnInfoClicked,
            OnJumpInClicked,
            OnSubscribeEventClicked,
            OnUnsubscribeEventClicked);

        goingEvents.SetItems(eventComponentsToAdd);
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
        eventModal.Show();
        ExploreEventsUtils.ConfigureEventCard(eventModal, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked);
    }

    public void HideEventModal() { eventModal.Hide(); }

    public void RestartScrollViewPosition() { scrollView.verticalNormalizedPosition = 1; }

    public void SetShowMoreUpcomingEventsButtonActive(bool isActive) { showMoreUpcomingEventsButtonContainer.gameObject.SetActive(isActive); }
}