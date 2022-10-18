using DCL;
using System;
using System.Collections;
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
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetTrendingEvents(List<EventCardComponentModel> events);

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
    
    /// <summary>
    /// Show loading bar for all events groups
    /// </summary>
    void SetAllEventGroupsAsLoading();
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

    [SerializeField] private Canvas canvas;

    public event Action OnReady;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;
    public event Action OnShowMoreUpcomingEventsClicked;
    public event Action OnEventsSubSectionEnable;

    internal Pool featuredEventCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool upcomingEventCardsPool;
    internal Pool goingEventCardsPool;
    
    internal EventCardComponentView eventModal;

    public int currentUpcomingEventsPerRow => upcomingEvents.currentItemsPerRow;
    
    public void SetTrendingEvents(List<EventCardComponentModel> events) => 
        SetEvents(events, trendingEvents, trendingEventCardsPool, trendingEventsLoading, trendingEventsNoDataText);

    public void SetGoingEvents(List<EventCardComponentModel> events) => 
        SetEvents(events, goingEvents, goingEventCardsPool, goingEventsLoading, goingEventsNoDataText);

    public void SetUpcomingEvents(List<EventCardComponentModel> events) => 
        SetEvents(events, upcomingEvents, upcomingEventCardsPool, upcomingEventsLoading, upcomingEventsNoDataText);

    private void SetEvents(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool eventCardsPool, GameObject loadingBar, TMP_Text eventsNoDataText)
    {
        SetEventsGroupAsLoading(false, eventsGrid.gameObject, loadingBar);
        eventsNoDataText.gameObject.SetActive(events.Count == 0);

        eventCardsPool.ReleaseAll();

        eventsGrid.ExtractItems();
        eventsGrid.RemoveItems();
        
        StartCoroutine(SetEventsIteratively(events, eventsGrid, eventCardsPool));
    }
    
    public void AddUpcomingEvents(List<EventCardComponentModel> events) => 
        StartCoroutine(SetEventsIteratively(events, upcomingEvents, upcomingEventCardsPool));

    private IEnumerator SetEventsIteratively(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool pool)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            eventsGrid.AddItem(
                ExploreEventsUtils.InstantiateConfiguredEventCard(eventInfo, pool,
                    OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked));
            yield return null;
        }
        
        eventsGrid.SetItemSizeForModel();
        pool.IterativePrewarm(events.Count);
    }
    
    public void SetFeaturedEvents(List<EventCardComponentModel> events)
    {
        SetEventsGroupAsLoading(false, featuredEvents.gameObject, featuredEventsLoading);
        featuredEvents.gameObject.SetActive(events.Count > 0);

        featuredEventCardsPool.ReleaseAll();

        featuredEvents.ExtractItems();

        featuredEvents.SetItems(
            ExploreEventsUtils.InstantiateAndConfigureEventCards(events, featuredEventCardsPool,
                OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked)
        );
        
        featuredEventCardsPool.IterativePrewarm(events.Count);
    }

    public void SetAllEventGroupsAsLoading()
    {
        SetEventsGroupAsLoading(isVisible: true, featuredEvents.gameObject, featuredEventsLoading);
        SetEventsGroupAsLoading(isVisible: true, goingEvents.gameObject, goingEventsLoading);
        SetEventsGroupAsLoading(isVisible: true, trendingEvents.gameObject, trendingEventsLoading);
        SetEventsGroupAsLoading(isVisible: true, upcomingEvents.gameObject, upcomingEventsLoading);
        
        goingEventsNoDataText.gameObject.SetActive(false);
        trendingEventsNoDataText.gameObject.SetActive(false);
        upcomingEventsNoDataText.gameObject.SetActive(false);
    }

    internal void SetEventsGroupAsLoading(bool isVisible, GameObject gridGroup, GameObject loadingBar)
    {
        gridGroup.SetActive(!isVisible);
        loadingBar.SetActive(isVisible);
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

    public override void OnEnable() { OnEventsSubSectionEnable?.Invoke(); }

    public void ConfigurePools()
    {
        ExploreEventsUtils.ConfigureEventCardsPool(out featuredEventCardsPool, FEATURED_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, FEATURED_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out trendingEventCardsPool, TRENDING_EVENT_CARDS_POOL_NAME, eventCardPrefab, TRENDING_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out upcomingEventCardsPool, UPCOMING_EVENT_CARDS_POOL_NAME, eventCardPrefab, UPCOMING_EVENT_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out goingEventCardsPool, GOING_EVENT_CARDS_POOL_NAME, eventCardPrefab, GOING_EVENT_CARDS_POOL_PREWARM);
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

    public void SetActive(bool isActive)
    {
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
            OnDisable();
    }

    public void SetShowMoreUpcomingEventsButtonActive(bool isActive) =>
        showMoreUpcomingEventsButtonContainer.gameObject.SetActive(isActive);

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        ExploreEventsUtils.ConfigureEventCard(eventModal, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked);
    }

    public void HideEventModal()
    {
        if (eventModal == null)
            return;

        eventModal.Hide();
    }

    public void RestartScrollViewPosition() { scrollView.verticalNormalizedPosition = 1; }
    
    // public void SetFeaturedEventsAsLoading(bool isVisible) => 
    //     SetEventsGroupAsLoading(isVisible, featuredEvents.gameObject, featuredEventsLoading);
    //
    // public void SetGoingEventsAsLoading(bool isVisible) =>
    //     SetEventsGroupAsLoading(isVisible, goingEvents.gameObject, goingEventsLoading, goingEventsNoDataText);
    //
    // public void SetTrendingEventsAsLoading(bool isVisible) =>
    //     SetEventsGroupAsLoading(isVisible, trendingEvents.gameObject, trendingEventsLoading, trendingEventsNoDataText);
    //
    // public void SetUpcomingEventsAsLoading(bool isVisible) =>
    //     SetEventsGroupAsLoading(isVisible, upcomingEvents.gameObject, upcomingEventsLoading, upcomingEventsNoDataText);
}