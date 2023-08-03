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
    private const string EVENT_CARDS_POOL_NAME = "Events_EventCardsPool";
    private const int EVENT_CARDS_POOL_PREWARM = 9;
    private const string ALL_FILTER_ID = "all";
    private const string ALL_FILTER_TEXT = "All";
    private const string ONE_TIME_EVENT_FREQUENCY_FILTER_ID = "one_time_event";
    private const string ONE_TIME_EVENT_FREQUENCY_FILTER_TEXT = "One time event";
    private const string RECURRING_EVENT_FREQUENCY_FILTER_ID = "recurring_event";
    private const string RECURRING_EVENT_FREQUENCY_FILTER_TEXT = "Recurring event";

    private readonly Queue<Func<UniTask>> cardsVisualUpdateBuffer = new ();
    private readonly Queue<Func<UniTask>> poolsPrewarmAsyncsBuffer = new ();
    private readonly CancellationTokenSource cancellationTokenSource = new ();

    [Header("Assets References")]
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GameObject featuredEventsLoading;
    [SerializeField] internal GridContainerComponentView eventsGrid;
    [SerializeField] internal GameObject eventsLoading;
    [SerializeField] internal TMP_Text eventsNoDataText;
    [SerializeField] internal GameObject showMoreEventsButtonContainer;
    [SerializeField] internal ButtonComponentView showMoreEventsButton;
    [SerializeField] internal GameObject guestGoingToPanel;
    [SerializeField] internal ButtonComponentView connectWalletGuest;

    [SerializeField] internal Button featuredButton;
    [SerializeField] internal GameObject featuredDeselected;
    [SerializeField] internal GameObject featuredSelected;
    [SerializeField] internal Button trendingButton;
    [SerializeField] internal GameObject trendingDeselected;
    [SerializeField] internal GameObject trendingSelected;
    [SerializeField] internal Button wantToGoButton;
    [SerializeField] internal GameObject wantToGoDeselected;
    [SerializeField] internal GameObject wantToGoSelected;
    [SerializeField] internal DropdownComponentView frequencyDropdown;
    [SerializeField] internal DropdownComponentView categoriesDropdown;

    [SerializeField] private Canvas canvas;

    internal Pool featuredEventCardsPool;
    internal Pool eventCardsPool;

    internal EventCardComponentView eventModal;

    private Canvas eventsCanvas;

    private bool isUpdatingCardsVisual;
    private bool isGuest;

    public int currentEventsPerRow => eventsGrid.currentItemsPerRow;

    public void SetAllAsLoading() => SetAllEventGroupsAsLoading();

    public int CurrentTilesPerRow => currentEventsPerRow;

    public EventsType SelectedEventType { get; private set; }
    public string SelectedFrequency { get; private set; }
    public string SelectedCategory { get; private set; }

    public event Action OnReady;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;
    public event Action OnShowMoreEventsClicked;
    public event Action OnConnectWallet;
    public event Action OnEventsSubSectionEnable;
    public event Action OnFiltersChanged;

    public override void Awake()
    {
        base.Awake();
        eventsCanvas = eventsGrid.GetComponent<Canvas>();
        guestGoingToPanel.SetActive(false);
    }

    public void Start()
    {
        LoadFrequencyDropdown();

        eventModal = PlacesAndEventsCardsFactory.GetEventCardTemplateHiddenLazy(eventCardModalPrefab);

        featuredEvents.RemoveItems();
        eventsGrid.RemoveItems();

        showMoreEventsButton.onClick.RemoveAllListeners();
        showMoreEventsButton.onClick.AddListener(() => OnShowMoreEventsClicked?.Invoke());
        connectWalletGuest.onClick.RemoveAllListeners();
        connectWalletGuest.onClick.AddListener(() => OnConnectWallet?.Invoke());

        featuredButton.onClick.RemoveAllListeners();
        featuredButton.onClick.AddListener(ClickedOnFeatured);
        trendingButton.onClick.RemoveAllListeners();
        trendingButton.onClick.AddListener(ClickedOnTrending);
        wantToGoButton.onClick.RemoveAllListeners();
        wantToGoButton.onClick.AddListener(ClickedOnWantToGo);
        frequencyDropdown.OnOptionSelectionChanged += OnFrequencyFilterChanged;
        categoriesDropdown.OnOptionSelectionChanged += OnCategoryFilterChanged;

        OnReady?.Invoke();
    }

    public override void OnEnable()
    {
        SelectedEventType = EventsType.Upcoming;
        SetFeaturedStatus(false);
        SetTrendingStatus(false);
        SetWantToGoStatus(false);
        SelectedFrequency = ALL_FILTER_ID;
        frequencyDropdown.SetTitle(ALL_FILTER_TEXT);
        frequencyDropdown.SelectOption(ALL_FILTER_ID, false);
        SelectedCategory = ALL_FILTER_ID;

        OnEventsSubSectionEnable?.Invoke();
    }

    public void SetIsGuestUser(bool isGuestUser)
    {
        isGuest = isGuestUser;
    }

    public void SetCategories(List<ToggleComponentModel> categories)
    {
        if (categories.Count == 0)
            return;

        SelectedCategory = categories[0].id;
        categoriesDropdown.SetTitle(categories[0].text);
        categoriesDropdown.SetOptions(categories);
    }

    public override void Dispose()
    {
        base.Dispose();
        cancellationTokenSource.Cancel();

        showMoreEventsButton.onClick.RemoveAllListeners();

        frequencyDropdown.OnOptionSelectionChanged -= OnFrequencyFilterChanged;
        categoriesDropdown.OnOptionSelectionChanged -= OnCategoryFilterChanged;

        featuredEvents.Dispose();
        eventsGrid.Dispose();

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
        featuredEventCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(FEATURED_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, FEATURED_EVENT_CARDS_POOL_PREWARM);
        eventCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(EVENT_CARDS_POOL_NAME, eventCardPrefab, EVENT_CARDS_POOL_PREWARM);
    }

    public override void RefreshControl()
    {
        featuredEvents.RefreshControl();
        eventsGrid.RefreshControl();
    }

    public void SetAllEventGroupsAsLoading()
    {
        SetFeaturedEventsGroupAsLoading();
        SetEventsGroupAsLoading(isVisible: true, eventsCanvas, eventsLoading);
        eventsNoDataText.gameObject.SetActive(false);
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

    public void SetShowMoreEventsButtonActive(bool isActive) =>
        showMoreEventsButtonContainer.gameObject.SetActive(isActive);

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        EventsCardsConfigurator.Configure(eventModal, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked);
    }

    public void HideEventModal()
    {
        if (eventModal != null)
            eventModal.Hide();
    }

    public void RestartScrollViewPosition() =>
        scrollView.verticalNormalizedPosition = 1;

    public void SetEvents(List<EventCardComponentModel> events) =>
        SetEvents(events, eventsGrid, eventsCanvas, eventCardsPool, eventsLoading, eventsNoDataText);

    private void SetEvents(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Canvas gridCanvas, Pool eventCardsPool, GameObject loadingBar, TMP_Text eventsNoDataText)
    {
        SetEventsGroupAsLoading(false, gridCanvas, loadingBar);
        eventsNoDataText.gameObject.SetActive(events.Count == 0);

        eventCardsPool.ReleaseAll();

        eventsGrid.ExtractItems(eventCardsPool.container.transform);
        eventsGrid.RemoveItems();

        cardsVisualUpdateBuffer.Enqueue(() => SetEventsAsync(events, eventsGrid, eventCardsPool, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    public void AddEvents(List<EventCardComponentModel> events)
    {
        cardsVisualUpdateBuffer.Enqueue(() => SetEventsAsync(events, eventsGrid, eventCardsPool, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetEventsAsync(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Pool pool, CancellationToken cancellationToken)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            eventsGrid.AddItem(
                PlacesAndEventsCardsFactory.CreateConfiguredEventCard(pool, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked));

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

        featuredEvents.ExtractItems(featuredEventCardsPool.container.transform);

        cardsVisualUpdateBuffer.Enqueue(() => SetFeaturedEventsAsync(events, cancellationTokenSource.Token));
        UpdateCardsVisual();
    }

    private async UniTask SetFeaturedEventsAsync(List<EventCardComponentModel> events, CancellationToken cancellationToken)
    {
        foreach (EventCardComponentModel eventInfo in events)
        {
            featuredEvents.AddItem(
                PlacesAndEventsCardsFactory.CreateConfiguredEventCard(featuredEventCardsPool, eventInfo, OnInfoClicked, OnJumpInClicked, OnSubscribeEventClicked, OnUnsubscribeEventClicked));

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

    private void LoadFrequencyDropdown()
    {
        List<ToggleComponentModel> valuesToAdd = new List<ToggleComponentModel>
        {
            new () { id = ALL_FILTER_ID, text = ALL_FILTER_TEXT, isOn = true, isTextActive = true, changeTextColorOnSelect = true },
            new () { id = ONE_TIME_EVENT_FREQUENCY_FILTER_ID, text = ONE_TIME_EVENT_FREQUENCY_FILTER_TEXT, isOn = false, isTextActive = true, changeTextColorOnSelect = true },
            new () { id = RECURRING_EVENT_FREQUENCY_FILTER_ID, text = RECURRING_EVENT_FREQUENCY_FILTER_TEXT, isOn = false, isTextActive = true, changeTextColorOnSelect = true },
        };

        frequencyDropdown.SetTitle(valuesToAdd[0].text);
        frequencyDropdown.SetOptions(valuesToAdd);
    }

    private void SetFeaturedStatus(bool isSelected)
    {
        featuredDeselected.SetActive(!isSelected);
        featuredSelected.SetActive(isSelected);
    }

    private void SetTrendingStatus(bool isSelected)
    {
        trendingDeselected.SetActive(!isSelected);
        trendingSelected.SetActive(isSelected);
    }

    private void SetWantToGoStatus(bool isSelected)
    {
        wantToGoDeselected.SetActive(!isSelected);
        wantToGoSelected.SetActive(isSelected);
    }

    private void ClickedOnFeatured()
    {
        if (SelectedEventType == EventsType.Featured)
        {
            SelectedEventType = EventsType.Upcoming;
            SetFeaturedStatus(false);
        }
        else
        {
            SelectedEventType = EventsType.Featured;
            SetFeaturedStatus(true);
            SetTrendingStatus(false);
            SetWantToGoStatus(false);
        }

        OnFiltersChanged?.Invoke();
    }

    private void ClickedOnTrending()
    {
        if (SelectedEventType == EventsType.Trending)
        {
            SelectedEventType = EventsType.Upcoming;
            SetTrendingStatus(false);
        }
        else
        {
            SelectedEventType = EventsType.Trending;
            SetTrendingStatus(true);
            SetFeaturedStatus(false);
            SetWantToGoStatus(false);
        }

        OnFiltersChanged?.Invoke();
    }

    private void ClickedOnWantToGo()
    {
        if (SelectedEventType == EventsType.WantToGo)
        {
            SelectedEventType = EventsType.Upcoming;
            SetWantToGoStatus(false);
        }
        else
        {
            SelectedEventType = EventsType.WantToGo;
            SetWantToGoStatus(true);
            SetFeaturedStatus(false);
            SetTrendingStatus(false);
        }

        OnFiltersChanged?.Invoke();
    }

    private void OnFrequencyFilterChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        SelectedFrequency = optionId;
        frequencyDropdown.SetTitle(optionName);
        OnFiltersChanged?.Invoke();
    }

    private void OnCategoryFilterChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        SelectedCategory = optionId;
        categoriesDropdown.SetTitle(optionName);
        OnFiltersChanged?.Invoke();
    }
}
