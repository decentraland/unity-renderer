using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    internal const string FEATURED_EVENT_CARDS_POOL_NAME = "Events_FeaturedEventCardsPool";
    private const int FEATURED_EVENT_CARDS_POOL_PREWARM = 10;
    private const string TRENDING_EVENT_CARDS_POOL_NAME = "Events_TrendingEventCardsPool";
    private const int TRENDING_EVENT_CARDS_POOL_PREWARM = 12;
    private const string UPCOMING_EVENT_CARDS_POOL_NAME = "Events_UpcomingEventCardsPool";
    private const int UPCOMING_EVENT_CARDS_POOL_PREWARM = 9;
    private const string GOING_EVENT_CARDS_POOL_NAME = "Events_FeatureGoingEventCardsPool";
    private const int GOING_EVENT_CARDS_POOL_PREWARM = 9;

    private readonly Queue<Func<UniTask>> cardsVisualUpdateBuffer = new Queue<Func<UniTask>>();
    private readonly Queue<Func<UniTask>> poolsPrewarmAsyncsBuffer = new Queue<Func<UniTask>>();
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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
    
    internal Pool featuredEventCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool upcomingEventCardsPool;
    internal Pool goingEventCardsPool;
    
    internal EventCardComponentView eventModal;

    private Canvas trendingEventsCanvas;
    private Canvas upcomingEventsCanvas;
    private Canvas goingEventsCanvas;

    private bool isUpdatingCardsVisual;
    
    public int currentUpcomingEventsPerRow => upcomingEvents.currentItemsPerRow;
    
    public event Action OnReady;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;
    public event Action OnShowMoreUpcomingEventsClicked;
    public event Action OnEventsSubSectionEnable;

    public override void Awake()
    {
        base.Awake();
        trendingEventsCanvas = trendingEvents.GetComponent<Canvas>();
        upcomingEventsCanvas = upcomingEvents.GetComponent<Canvas>();
        goingEventsCanvas = goingEvents.GetComponent<Canvas>();
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

    public override void OnEnable()
    {
        OnEventsSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        cancellationTokenSource.Cancel();
        
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
    
    public void SetAllEventGroupsAsLoading()
    {
        SetFeaturedEventsGroupAsLoading();
        
        SetEventsGroupAsLoading(isVisible: true, goingEventsCanvas, goingEventsLoading);
        SetEventsGroupAsLoading(isVisible: true, trendingEventsCanvas, trendingEventsLoading);
        SetEventsGroupAsLoading(isVisible: true, upcomingEventsCanvas, upcomingEventsLoading);

        goingEventsNoDataText.gameObject.SetActive(false);
        trendingEventsNoDataText.gameObject.SetActive(false);
        upcomingEventsNoDataText.gameObject.SetActive(false);
    }
    
    internal void SetFeaturedEventsGroupAsLoading()
    {
        featuredEvents.gameObject.SetActive(false);
        featuredEventsLoading.SetActive(true);
    }

    public void SetEventsGroupAsLoading(bool isVisible, Canvas gridCanvas, GameObject loadingBar)
    {
        gridCanvas.enabled = !isVisible;
        loadingBar.SetActive(isVisible);
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
        if (eventModal != null)
            eventModal.Hide();
    }

    public void RestartScrollViewPosition() => 
        scrollView.verticalNormalizedPosition = 1;
    
    public void SetTrendingEvents(List<EventCardComponentModel> events) =>
        SetEvents(events, trendingEvents, trendingEventsCanvas, trendingEventCardsPool, trendingEventsLoading, trendingEventsNoDataText);

    public void SetGoingEvents(List<EventCardComponentModel> events) =>
        SetEvents(events, goingEvents, goingEventsCanvas, goingEventCardsPool, goingEventsLoading, goingEventsNoDataText);

    public void SetUpcomingEvents(List<EventCardComponentModel> events) =>
        SetEvents(events, upcomingEvents, upcomingEventsCanvas, upcomingEventCardsPool, upcomingEventsLoading, upcomingEventsNoDataText);

    private void SetEvents(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Canvas gridCanvas, Pool eventCardsPool, GameObject loadingBar, TMP_Text eventsNoDataText)
    {
        SetEventsGroupAsLoading(false, gridCanvas, loadingBar);
        eventsNoDataText.gameObject.SetActive(events.Count == 0);

        eventCardsPool.ReleaseAll();

        eventsGrid.ExtractItems();
        eventsGrid.RemoveItems();

        cardsVisualUpdateBuffer.Enqueue(() => SetEventsAsync(events, eventsGrid, eventCardsPool, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    public void AddUpcomingEvents(List<EventCardComponentModel> events)
    {
        cardsVisualUpdateBuffer.Enqueue(() => SetEventsAsync(events, upcomingEvents, upcomingEventCardsPool, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetEventsAsync(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool pool, CancellationToken cancellationToken)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            eventsGrid.AddItem(
                ExploreEventsUtils.InstantiateConfiguredEventCard(eventInfo, pool,
                    OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked));
            await UniTask.NextFrame(cancellationToken);
        }

        eventsGrid.SetItemSizeForModel();
        poolsPrewarmAsyncsBuffer.Enqueue(() => pool.PrewarmAsync(events.Count, cancellationToken));
    }

    public void SetFeaturedEvents(List<EventCardComponentModel> events)
    {
        featuredEventsLoading.SetActive(false);
        featuredEvents.gameObject.SetActive(events.Count > 0);

        featuredEventCardsPool.ReleaseAll();

        featuredEvents.ExtractItems();
        
        cardsVisualUpdateBuffer.Enqueue(() => SetFeaturedEventsAsync(events, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }
    
    private async UniTask SetFeaturedEventsAsync(List<EventCardComponentModel> events, CancellationToken cancellationToken)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            featuredEvents.AddItem(
                ExploreEventsUtils.InstantiateConfiguredEventCard(eventInfo, featuredEventCardsPool,
                    OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked));
            await UniTask.NextFrame(cancellationToken);
        }

        featuredEvents.SetManualControlsActive();
        featuredEvents.GenerateDotsSelector();
        
        poolsPrewarmAsyncsBuffer.Enqueue(() => featuredEventCardsPool.PrewarmAsync(events.Count, cancellationToken));
    }
    
    private void UpdateCardsVisual()
    {
        if (!isUpdatingCardsVisual)
            ShowCardsProcess().Forget();
        
        async UniTask ShowCardsProcess()
        {
            isUpdatingCardsVisual = true;

            while (cardsVisualUpdateBuffer.Count > 0)
                await cardsVisualUpdateBuffer.Dequeue().Invoke();

            while (poolsPrewarmAsyncsBuffer.Count > 0)
                await poolsPrewarmAsyncsBuffer.Dequeue().Invoke();

            isUpdatingCardsVisual = false;
        }
    }
}