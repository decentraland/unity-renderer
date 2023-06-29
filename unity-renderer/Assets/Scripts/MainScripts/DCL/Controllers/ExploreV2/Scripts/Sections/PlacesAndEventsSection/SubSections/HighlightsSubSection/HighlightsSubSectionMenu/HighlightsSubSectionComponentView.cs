using Cysharp.Threading.Tasks;
using System.Linq;
using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using DCL.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.Controllers.HotScenes;

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

    private Func<CancellationToken, UniTask> setPlacesTask;
    private Func<CancellationToken, UniTask> setLiveEventsTask;
    private Func<CancellationToken, UniTask> setTrendingTask;
    private readonly Queue<Func<UniTask>> poolsPrewarmAsyncsBuffer = new ();
    private readonly CancellationTokenSource disposeCts = new ();
    private CancellationTokenSource setPlacesCts;
    private CancellationTokenSource updateVisualsCts;

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

    public void SetAllAsLoading()
    {
        SetTrendingPlacesAndEventsAsLoading(true);
        SetFeaturedPlacesAsLoading(true);
        SetLiveAsLoading(true);
    }

    public int CurrentTilesPerRow { get; }
    public int CurrentGoingTilesPerRow { get; }

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnPlaceInfoClicked;
    public event Action<EventCardComponentModel> OnEventInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;
    public event Action<string, bool> OnFavoriteClicked;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<string> OnEventSubscribeEventClicked;
    public event Action<string> OnEventUnsubscribeEventClicked;
    public event Action OnViewAllEventsClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnHighlightsSubSectionEnable;

    public void Start()
    {
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);
        eventModal = PlacesAndEventsCardsFactory.GetEventCardTemplateHiddenLazy(eventCardModalPrefab);

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

        disposeCts?.SafeCancelAndDispose();
        setPlacesCts?.SafeCancelAndDispose();
        updateVisualsCts?.SafeCancelAndDispose();

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
        if (canvas.enabled == isActive)
            return;
        updateVisualsCts?.SafeCancelAndDispose();
        canvas.enabled = isActive;

        if (isActive)
        {
            updateVisualsCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
            UpdateCardsVisualProcess(updateVisualsCts.Token).Forget();
            OnEnable();
        }
        else
        {
            poolsPrewarmAsyncsBuffer.Clear();
            OnDisable();
        }
    }

    public void ConfigurePools()
    {
        trendingPlaceCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(TRENDING_PLACE_CARDS_POOL_NAME, placeCardLongPrefab, TRENDING_PLACE_CARDS_POOL_PREWARM);
        featuredPlaceCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(FEATURED_PLACE_CARDS_POOL_NAME, placeCardPrefab, FEATURED_PLACE_CARDS_POOL_PREWARM);

        trendingEventCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(TRENDING_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, TRENDING_EVENT_CARDS_POOL_PREWARM);
        liveEventCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(LIVE_EVENT_CARDS_POOL_NAME, eventCardPrefab, LIVE_EVENT_CARDS_POOL_PREWARM);
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
        PlacesCardsConfigurator.Configure(placeModal, placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked, OnFavoriteClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        EventsCardsConfigurator.Configure(eventModal, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
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

        setPlacesCts?.SafeCancelAndDispose();
        setPlacesCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        setPlacesTask = (ct) => SetPlacesAsync(places, CancellationTokenSource.CreateLinkedTokenSource(setPlacesCts.Token, ct).Token);
    }

    private async UniTask SetPlacesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        featuredPlaceCardsPool.ReleaseAll();

        featuredPlaces.ExtractItems();
        featuredPlaces.RemoveItems();

        foreach (PlaceCardComponentModel place in places)
        {
            PlaceCardComponentView placeCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(featuredPlaceCardsPool, place, OnPlaceInfoClicked, OnPlaceJumpInClicked, OnFavoriteClicked);
            OnFriendHandlerAdded?.Invoke(placeCard.friendsHandler);

            this.featuredPlaces.AddItem(placeCard);

            await UniTask.NextFrame(cancellationToken);
        }

        featuredPlaces.SetItemSizeForModel();
        poolsPrewarmAsyncsBuffer.Enqueue(() => featuredPlaceCardsPool.PrewarmAsync(places.Count, cancellationToken));
    }

    public void SetLiveEvents(List<EventCardComponentModel> events)
    {
        SetLiveAsLoading(false);
        liveEventsNoDataText.gameObject.SetActive(events.Count == 0);

        setLiveEventsTask = (ct) => SetLiveEventsAsync(events, liveEvents, liveEventCardsPool, disposeCts.Token);
    }

    private async UniTask SetLiveEventsAsync(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool pool, CancellationToken cancellationToken)
    {
        liveEventCardsPool.ReleaseAll();

        liveEvents.ExtractItems();
        liveEvents.RemoveItems();

        foreach (EventCardComponentModel eventInfo in events)
        {
            eventsGrid.AddItem(
                PlacesAndEventsCardsFactory.CreateConfiguredEventCard(pool, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked));

            await UniTask.NextFrame(cancellationToken);
        }

        eventsGrid.SetItemSizeForModel();

        poolsPrewarmAsyncsBuffer.Enqueue(() => pool.PrewarmAsync(events.Count, cancellationToken));
    }

    public void SetTrendingPlacesAndEvents(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events)
    {
        SetTrendingPlacesAndEventsAsLoading(false);

        setTrendingTask = (ct) => SetTrendingsAsync(places, events, CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token, ct).Token);
    }

    private async UniTask SetTrendingsAsync(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events, CancellationToken cancellationToken)
    {
        trendingPlaceCardsPool.ReleaseAll();
        trendingEventCardsPool.ReleaseAll();

        trendingPlacesAndEvents.ExtractItems();
        trendingPlacesAndEvents.RemoveItems();

        int eventId = 0;
        int placeId = 0;

        for (int i = 0; i < HighlightsSubSectionComponentController.DEFAULT_NUMBER_OF_TRENDING_PLACES; i++)
        {
            if (i % 2 == 0 && eventId < events.Count)
            {
                trendingPlacesAndEvents.AddItem(
                    PlacesAndEventsCardsFactory.CreateConfiguredEventCard(trendingEventCardsPool, events[eventId], OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked));

                eventId++;
            }
            else if (placeId < places.Count)
            {
                PlaceCardComponentView placeCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(trendingPlaceCardsPool, places[placeId], OnPlaceInfoClicked, OnPlaceJumpInClicked, OnFavoriteClicked);
                OnFriendHandlerAdded?.Invoke(placeCard.friendsHandler);

                trendingPlacesAndEvents.AddItem(placeCard);

                placeId++;
            }

            await UniTask.NextFrame(cancellationToken);
        }

        trendingPlacesAndEvents.SetManualControlsActive();
        trendingPlacesAndEvents.GenerateDotsSelector();

        poolsPrewarmAsyncsBuffer.Enqueue(() => trendingPlaceCardsPool.PrewarmAsync(HighlightsSubSectionComponentController.DEFAULT_NUMBER_OF_TRENDING_PLACES, cancellationToken));
    }

    private async UniTask UpdateCardsVisualProcess(CancellationToken ct)
    {
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                if (setPlacesTask != null)
                {
                    await setPlacesTask.Invoke(ct);
                    setPlacesTask = null;
                }

                ct.ThrowIfCancellationRequested();
                if (setLiveEventsTask != null)
                {
                    await setLiveEventsTask.Invoke(ct);
                    setLiveEventsTask = null;
                }

                ct.ThrowIfCancellationRequested();
                if(setTrendingTask != null)
                {
                    await setTrendingTask.Invoke(ct);
                    setTrendingTask = null;
                }

                ct.ThrowIfCancellationRequested();

                while (poolsPrewarmAsyncsBuffer.Count > 0)
                {

                    ct.ThrowIfCancellationRequested();
                    await poolsPrewarmAsyncsBuffer.Dequeue().Invoke();
                }

                await UniTask.Yield();
            }
        }
        catch (OperationCanceledException) { }
    }
}
