using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighlightsSubSectionComponentView : BaseComponentView, IHighlightsSubSectionComponentView
{
    private const string TRENDING_PLACE_CARDS_POOL_NAME = "Highlights_TrendingPlaceCardsPool";
    private const int TRENDING_PLACE_CARDS_POOL_PREWARM = 10;
    private const string TRENDING_EVENT_CARDS_POOL_NAME = "Highlights_TrendingEventCardsPool";
    private const int TRENDING_EVENT_CARDS_POOL_PREWARM = 10;
    private const string FEATURED_PLACE_CARDS_POOL_NAME = "Highlights_FeaturedPlaceCardsPool";
    private const int FEATURED_PLACE_CARDS_POOL_PREWARM = 9;
    private const string LIVE_EVENT_CARDS_POOL_NAME = "Highlights_LiveEventCardsPool";
    private const int LIVE_EVENT_CARDS_POOL_PREWARM = 3;

    private readonly Queue<Func<UniTask>> cardsVisualUpdateBuffer = new Queue<Func<UniTask>>();
    private readonly Queue<Func<UniTask>> poolsPrewarmAsyncsBuffer = new Queue<Func<UniTask>>();
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal CarouselComponentView trendingPlacesAndEvents;
    [SerializeField] internal GameObject trendingPlacesAndEventsLoading;
    [SerializeField] internal GridContainerComponentView featuredPlaces;
    [SerializeField] internal GameObject featuredPlacesLoading;
    [SerializeField] internal TMP_Text featuredPlacesNoDataText;
    [SerializeField] internal GridContainerComponentView liveEvents;
    [SerializeField] internal GameObject liveEventsLoading;
    [SerializeField] internal TMP_Text liveEventsNoDataText;
    [SerializeField] internal ButtonComponentView viewAllEventsButton;
    [SerializeField] internal Color[] friendColors = null;

    [SerializeField] private Canvas canvas;
    
    internal PlaceCardComponentView placeModal;
    internal EventCardComponentView eventModal;
    internal Pool trendingPlaceCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool featuredPlaceCardsPool;
    internal Pool liveEventCardsPool;
    
    private bool isUpdatingCardsVisual;

    public Color[] currentFriendColors => friendColors;

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnPlaceInfoClicked;
    public event Action<EventCardComponentModel> OnEventInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<string> OnEventSubscribeEventClicked;
    public event Action<string> OnEventUnsubscribeEventClicked;
    public event Action OnViewAllEventsClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnHighlightsSubSectionEnable;

    public override void Start()
    {
        placeModal = ExplorePlacesUtils.ConfigurePlaceCardModal(placeCardModalPrefab);
        eventModal = ExploreEventsUtils.ConfigureEventCardModal(eventCardModalPrefab);

        trendingPlacesAndEvents.RemoveItems();
        featuredPlaces.RemoveItems();
        liveEvents.RemoveItems();

        viewAllEventsButton.onClick.AddListener(() => OnViewAllEventsClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void OnEnable()
    {
        OnHighlightsSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();

        cancellationTokenSource.Cancel();

        trendingPlacesAndEvents.Dispose();
        featuredPlaces.Dispose();
        liveEvents.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }

        if (eventModal != null)
        {
            eventModal.Dispose();
            Destroy(eventModal.gameObject);
        }

        viewAllEventsButton.onClick.RemoveAllListeners();
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
        ExplorePlacesUtils.ConfigurePlaceCardsPool(out trendingPlaceCardsPool, TRENDING_PLACE_CARDS_POOL_NAME, placeCardLongPrefab, TRENDING_PLACE_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out trendingEventCardsPool, TRENDING_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, TRENDING_EVENT_CARDS_POOL_PREWARM);
        ExplorePlacesUtils.ConfigurePlaceCardsPool(out featuredPlaceCardsPool, FEATURED_PLACE_CARDS_POOL_NAME, placeCardPrefab, FEATURED_PLACE_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out liveEventCardsPool, LIVE_EVENT_CARDS_POOL_NAME, eventCardPrefab, LIVE_EVENT_CARDS_POOL_PREWARM);
    }

    public override void RefreshControl()
    {
        trendingPlacesAndEvents.RefreshControl();
        featuredPlaces.RefreshControl();
        liveEvents.RefreshControl();
    }

    public void SetTrendingPlacesAndEventsAsLoading(bool isVisible)
    {
        SetTrendingPlacesAndEventsActive(!isVisible);
        trendingPlacesAndEventsLoading.SetActive(isVisible);
    }

    internal void SetTrendingPlacesAndEventsActive(bool isActive) =>
        trendingPlacesAndEvents.gameObject.SetActive(isActive);

    public void SetFeaturedPlacesAsLoading(bool isVisible)
    {
        featuredPlaces.gameObject.SetActive(!isVisible);
        featuredPlacesLoading.SetActive(isVisible);

        if (isVisible)
            featuredPlacesNoDataText.gameObject.SetActive(false);
    }

    public void SetLiveAsLoading(bool isVisible)
    {
        liveEvents.gameObject.SetActive(!isVisible);
        liveEventsLoading.SetActive(isVisible);
        viewAllEventsButton.gameObject.SetActive(!isVisible);

        if (isVisible)
            liveEventsNoDataText.gameObject.SetActive(false);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        ExplorePlacesUtils.ConfigurePlaceCard(placeModal, placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        ExploreEventsUtils.ConfigureEventCard(eventModal, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
    }

    public void HideEventModal()
    {
        if (eventModal != null)
            eventModal.Hide();
    }

    public void RestartScrollViewPosition() => 
        scrollView.verticalNormalizedPosition = 1;

    public void SetFeaturedPlaces(List<PlaceCardComponentModel> places)
    {
        SetFeaturedPlacesAsLoading(false);
        featuredPlacesNoDataText.gameObject.SetActive(places.Count == 0);

        featuredPlaceCardsPool.ReleaseAll();

        featuredPlaces.ExtractItems();
        featuredPlaces.RemoveItems();

        cardsVisualUpdateBuffer.Enqueue(() => SetPlacesAsync(places, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetPlacesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel place in places)
        {
            featuredPlaces.AddItem(
                ExplorePlacesUtils.InstantiateConfiguredPlaceCard(place, featuredPlaceCardsPool,
                    OnFriendHandlerAdded, OnPlaceInfoClicked, OnPlaceJumpInClicked));

            await UniTask.NextFrame(cancellationToken);
        }

        featuredPlaces.SetItemSizeForModel();
        poolsPrewarmAsyncsBuffer.Enqueue(() => featuredPlaceCardsPool.PrewarmAsync(places.Count, cancellationToken));
    }

    public void SetLiveEvents(List<EventCardComponentModel> events)
    {
        SetLiveAsLoading(false);
        liveEventsNoDataText.gameObject.SetActive(events.Count == 0);

        liveEventCardsPool.ReleaseAll();

        liveEvents.ExtractItems();
        liveEvents.RemoveItems();

        cardsVisualUpdateBuffer.Enqueue(() => SetEventsAsync(events, liveEvents, liveEventCardsPool, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetEventsAsync(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool pool, CancellationToken cancellationToken)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            eventsGrid.AddItem(
                ExploreEventsUtils.InstantiateConfiguredEventCard(eventInfo, pool,
                    OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked));

            await UniTask.NextFrame(cancellationToken);
        }

        eventsGrid.SetItemSizeForModel();

        poolsPrewarmAsyncsBuffer.Enqueue(() => pool.PrewarmAsync(events.Count, cancellationToken));
    }

    public void SetTrendingPlacesAndEvents(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events)
    {
        SetTrendingPlacesAndEventsAsLoading(false);

        trendingPlaceCardsPool.ReleaseAll();
        trendingEventCardsPool.ReleaseAll();

        trendingPlacesAndEvents.ExtractItems();
        trendingPlacesAndEvents.RemoveItems();

        cardsVisualUpdateBuffer.Enqueue(() => SetTrendingsAsync(places, events, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetTrendingsAsync(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events, CancellationToken cancellationToken)
    {
        int eventId = 0;
        int placeId = 0;

        for (int i = 0 ; i < HighlightsSubSectionComponentController.DEFAULT_NUMBER_OF_TRENDING_PLACES; i++)
        {
            if (i % 2 == 0 && eventId < events.Count)
            {
                trendingPlacesAndEvents.AddItem(ExploreEventsUtils.InstantiateConfiguredEventCard(events[eventId], trendingEventCardsPool,
                    OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked));
                eventId++;
            }
            else if (placeId < places.Count)
            {
                trendingPlacesAndEvents.AddItem(ExplorePlacesUtils.InstantiateConfiguredPlaceCard(places[placeId], trendingPlaceCardsPool,
                    OnFriendHandlerAdded, OnPlaceInfoClicked, OnPlaceJumpInClicked));
                placeId++;
            }

            await UniTask.NextFrame(cancellationToken);
        }

        trendingPlacesAndEvents.SetManualControlsActive();
        trendingPlacesAndEvents.GenerateDotsSelector();

        poolsPrewarmAsyncsBuffer.Enqueue(() => trendingPlaceCardsPool.PrewarmAsync(HighlightsSubSectionComponentController.DEFAULT_NUMBER_OF_TRENDING_PLACES, cancellationToken));
    }
    
    private void UpdateCardsVisual()
    {
        if (!isUpdatingCardsVisual)
            UpdateCardsVisualProcess().Forget();
        
        async UniTask UpdateCardsVisualProcess()
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