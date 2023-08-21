using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Tasks;
using UIComponents.Scripts.Components.RangeSlider;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private const float TIME_MIN_VALUE = 0;
    private const float TIME_MAX_VALUE = 48;

    private readonly Queue<Func<UniTask>> cardsVisualUpdateBuffer = new ();
    private readonly Queue<Func<UniTask>> poolsPrewarmAsyncsBuffer = new ();
    private CancellationTokenSource cancellationTokenSource;
    private CancellationTokenSource featuredEventsCancellationTokenSource;

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
    [SerializeField] internal GameObject eventsNoDataContainer;
    [SerializeField] internal GameObject showMoreEventsButtonContainer;
    [SerializeField] internal ButtonComponentView showMoreEventsButton;
    [SerializeField] internal GameObject guestGoingToPanel;
    [SerializeField] internal ButtonComponentView connectWalletGuest;

    [SerializeField] internal Button featuredButton;
    [SerializeField] internal GameObject featuredDeselected;
    [SerializeField] internal Image featuredDeselectedImage;
    [SerializeField] internal GameObject featuredSelected;
    [SerializeField] internal Image featuredSelectedImage;
    [SerializeField] internal Button trendingButton;
    [SerializeField] internal GameObject trendingDeselected;
    [SerializeField] internal Image trendingDeselectedImage;
    [SerializeField] internal GameObject trendingSelected;
    [SerializeField] internal Image trendingSelectedImage;
    [SerializeField] internal Button wantToGoButton;
    [SerializeField] internal GameObject wantToGoDeselected;
    [SerializeField] internal Image wantToGoDeselectedImage;
    [SerializeField] internal GameObject wantToGoSelected;
    [SerializeField] internal Image wantToGoSelectedImage;
    [SerializeField] internal DropdownComponentView frequencyDropdown;
    [SerializeField] internal DropdownComponentView categoriesDropdown;
    [SerializeField] internal DropdownWithRangeSliderComponentView timeDropdown;

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
    public float SelectedLowTime { get; private set; }
    public float SelectedHighTime { get; private set; }

    public event Action OnReady;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;
    public event Action OnShowMoreEventsClicked;
    public event Action OnConnectWallet;
    public event Action OnEventsSubSectionEnable;
    public event Action OnEventTypeFiltersChanged;
    public event Action OnEventFrequencyFilterChanged;
    public event Action OnEventCategoryFilterChanged;
    public event Action OnEventTimeFilterChanged;

    public override void Awake()
    {
        base.Awake();
        cancellationTokenSource = cancellationTokenSource.SafeRestart();
        featuredEventsCancellationTokenSource = featuredEventsCancellationTokenSource.SafeRestart();
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
        timeDropdown.OnValueChanged.AddListener(OnTimeFilterChanged);

        OnReady?.Invoke();
    }

    public override void OnEnable()
    {
        SelectedEventType = EventsType.Upcoming;
        SetFeaturedStatus(false);
        SetTrendingStatus(false);
        SetWantToGoStatus(false);
        SetFrequencyDropdownValue(ALL_FILTER_ID, ALL_FILTER_TEXT, false);
        SetCategoryDropdownValue(ALL_FILTER_ID, ALL_FILTER_TEXT, false);
        SelectedLowTime = TIME_MIN_VALUE;
        SelectedHighTime = TIME_MAX_VALUE;
        timeDropdown.SetWholeNumbers(true);
        timeDropdown.SetValues(TIME_MIN_VALUE, TIME_MAX_VALUE);
        timeDropdown.SetTitle($"{ConvertToTimeString(TIME_MIN_VALUE)} - {ConvertToTimeString(TIME_MAX_VALUE)} (UTC)");

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

        categoriesDropdown.SetOptions(categories);
        SetCategoryDropdownValue(categories[0].id, categories[0].text, false);
    }

    public override void Dispose()
    {
        base.Dispose();
        cancellationTokenSource.SafeCancelAndDispose();
        featuredEventsCancellationTokenSource.SafeCancelAndDispose();

        showMoreEventsButton.onClick.RemoveAllListeners();

        frequencyDropdown.OnOptionSelectionChanged -= OnFrequencyFilterChanged;
        categoriesDropdown.OnOptionSelectionChanged -= OnCategoryFilterChanged;
        timeDropdown.OnValueChanged.RemoveAllListeners();

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
        eventsNoDataContainer.SetActive(false);
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
        SetEvents(events, eventsGrid, eventsCanvas, eventCardsPool, eventsLoading);

    private void SetEvents(List<EventCardComponentModel> events, GridContainerComponentView eventsGrid, Canvas gridCanvas, Pool eventCardsPool, GameObject loadingBar)
    {
        SetEventsGroupAsLoading(false, gridCanvas, loadingBar);
        eventsNoDataContainer.gameObject.SetActive(events.Count == 0);
        eventsNoDataContainer.gameObject.SetActive(events.Count == 0 && !(SelectedEventType == EventsType.WantToGo && isGuest));
        guestGoingToPanel.SetActive(SelectedEventType == EventsType.WantToGo && isGuest);

        eventCardsPool.ReleaseAll();

        eventsGrid.ExtractItems();
        eventsGrid.RemoveItems();

        cancellationTokenSource = cancellationTokenSource.SafeRestart();
        cardsVisualUpdateBuffer.Clear();
        isUpdatingCardsVisual = false;
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

        featuredEvents.ExtractItems();

        featuredEventsCancellationTokenSource = featuredEventsCancellationTokenSource.SafeRestart();
        SetFeaturedEventsAsync(events, featuredEventsCancellationTokenSource.Token).Forget();
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
        featuredButton.targetGraphic = isSelected ? featuredSelectedImage : featuredDeselectedImage;
        featuredDeselected.SetActive(!isSelected);
        featuredSelected.SetActive(isSelected);
    }

    private void SetTrendingStatus(bool isSelected)
    {
        trendingButton.targetGraphic = isSelected ? trendingSelectedImage : trendingDeselectedImage;
        trendingDeselected.SetActive(!isSelected);
        trendingSelected.SetActive(isSelected);
    }

    private void SetWantToGoStatus(bool isSelected)
    {
        wantToGoButton.targetGraphic = isSelected ? wantToGoSelectedImage : wantToGoDeselectedImage;
        wantToGoDeselected.SetActive(!isSelected);
        wantToGoSelected.SetActive(isSelected);
    }

    private void ClickedOnFeatured()
    {
        DeselectButtons();

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

        OnEventTypeFiltersChanged?.Invoke();
    }

    private void ClickedOnTrending()
    {
        DeselectButtons();

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

        OnEventTypeFiltersChanged?.Invoke();
    }

    private void ClickedOnWantToGo()
    {
        DeselectButtons();

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

        OnEventTypeFiltersChanged?.Invoke();
    }

    private void OnFrequencyFilterChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        SetFrequencyDropdownValue(optionId, optionName, false);
        OnEventFrequencyFilterChanged?.Invoke();
    }

    private void OnCategoryFilterChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        SetCategoryDropdownValue(optionId, optionName, false);
        OnEventCategoryFilterChanged?.Invoke();
    }

    private void OnTimeFilterChanged(float lowValue, float highValue)
    {
        SelectedLowTime = lowValue;
        SelectedHighTime = highValue;
        timeDropdown.SetTitle($"{ConvertToTimeString(lowValue)} - {ConvertToTimeString(highValue)} (UTC)");
        OnEventTimeFilterChanged?.Invoke();
    }

    private static string ConvertToTimeString(float hours)
    {
        var wholeHours = (int)(hours / 2);
        int minutes = (int)(hours % 2) * 30;
        return $"{wholeHours:D2}:{minutes:D2}";
    }

    private void SetFrequencyDropdownValue(string id, string title, bool notify)
    {
        SelectedFrequency = id;
        frequencyDropdown.SetTitle(title);
        frequencyDropdown.SelectOption(id, notify);
    }

    private void SetCategoryDropdownValue(string id, string title, bool notify)
    {
        SelectedCategory = id;
        categoriesDropdown.SetTitle(title);
        categoriesDropdown.SelectOption(id, notify);
    }

    private static void DeselectButtons()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
    }
}
