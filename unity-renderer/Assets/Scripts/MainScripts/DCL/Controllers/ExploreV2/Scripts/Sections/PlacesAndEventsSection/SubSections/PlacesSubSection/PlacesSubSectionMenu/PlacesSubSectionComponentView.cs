using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.Controllers.HotScenes;
using TMPro;
using UnityEngine.EventSystems;

public class PlacesSubSectionComponentView : BaseComponentView, IPlacesSubSectionComponentView
{
    internal const string PLACE_CARDS_POOL_NAME = "Places_PlaceCardsPool";
    private const int PLACE_CARDS_POOL_PREWARM = 20;
    private const string MOST_ACTIVE_FILTER_ID = "most_active";
    private const string MOST_ACTIVE_FILTER_TEXT = "Most active";
    private const string BEST_FILTER_ID = "like_rate";
    private const string BEST_FILTER_TEXT = "Best";

    private readonly CancellationTokenSource disposeCts = new ();
    private CancellationTokenSource setPlacesCts = new ();
    private readonly List<PlaceCategoryButton> placeCategoryButtons = new ();

    private List<string> poiCoords;

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;
    [SerializeField] internal PlaceCategoryButton placeCategoryButtonPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView places;
    [SerializeField] internal GameObject placesLoading;
    [SerializeField] internal GameObject placesNoDataContainer;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal GameObject showMorePlacesButtonContainer;
    [SerializeField] internal ButtonComponentView showMorePlacesButton;
    [SerializeField] internal DropdownComponentView sortByDropdown;
    [SerializeField] internal Transform placeCategoriesGrid;
    [SerializeField] internal TMP_Text totalResultsCounter;

    [SerializeField] private Canvas canvas;

    internal PlaceCardComponentView placeModal;
    internal Pool placeCardsPool;
    private Canvas placesCanvas;

    public Color[] currentFriendColors => friendColors;

    public int currentPlacesPerRow => places.currentItemsPerRow;

    public void SetAllAsLoading() => SetPlacesAsLoading(true);
    public void SetShowMoreButtonActive(bool isActive) => SetShowMorePlacesButtonActive(isActive);

    public void SetPOICoords(List<string> poiList) =>
        poiCoords = poiList;

    public int CurrentTilesPerRow => currentPlacesPerRow;
    public int CurrentGoingTilesPerRow { get; }
    public string filter { get; private set; }
    public string sort { get; private set; }

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnJumpInClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<string, bool> OnFavoriteClicked;
    public event Action<IFriendTrackerHandler> OnFriendHandlerAdded;
    public event Action OnPlacesSubSectionEnable;
    public event Action OnFilterChanged;
    public event Action OnSortingChanged;
    public event Action OnShowMorePlacesClicked;

    public override void Awake()
    {
        base.Awake();
        placesCanvas = places.GetComponent<Canvas>();
    }

    public void Start()
    {
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);

        places.RemoveItems();
        LoadSortByDropdown();

        showMorePlacesButton.onClick.RemoveAllListeners();
        showMorePlacesButton.onClick.AddListener(() => OnShowMorePlacesClicked?.Invoke());
        sortByDropdown.OnOptionSelectionChanged += SortByDropdownValueChanged;
        filter = "";
        SetSortDropdownValue(MOST_ACTIVE_FILTER_ID, MOST_ACTIVE_FILTER_TEXT, false);
        OnReady?.Invoke();
    }

    private void LoadSortByDropdown()
    {
        List<ToggleComponentModel> sortingMethodsToAdd = new List<ToggleComponentModel>
        {
            new () { id = MOST_ACTIVE_FILTER_ID, text = MOST_ACTIVE_FILTER_TEXT, isOn = true, isTextActive = true, changeTextColorOnSelect = true },
            new () { id = BEST_FILTER_ID, text = BEST_FILTER_TEXT, isOn = false, isTextActive = true, changeTextColorOnSelect = true },
        };

        sortByDropdown.SetTitle(sortingMethodsToAdd[0].text);
        sortByDropdown.SetOptions(sortingMethodsToAdd);
    }

    private void SortByDropdownValueChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        SetSortDropdownValue(optionId, optionName, false);
        OnSortingChanged?.Invoke();
    }

    public override void OnEnable()
    {
        foreach (PlaceCategoryButton categoryBtn in placeCategoryButtons)
            categoryBtn.SetStatus(false);

        filter = "";
        SetSortDropdownValue(MOST_ACTIVE_FILTER_ID, MOST_ACTIVE_FILTER_TEXT, false);
        OnPlacesSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        disposeCts?.SafeCancelAndDispose();
        setPlacesCts?.SafeCancelAndDispose();

        showMorePlacesButton.onClick.RemoveAllListeners();

        sortByDropdown.OnOptionSelectionChanged -= SortByDropdownValueChanged;

        places.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }
    }

    public void ConfigurePools() =>
        placeCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(PLACE_CARDS_POOL_NAME, placeCardPrefab, PLACE_CARDS_POOL_PREWARM);

    public override void RefreshControl() =>
        places.RefreshControl();

    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        SetPlacesAsLoading(false);
        placesNoDataContainer.SetActive(places.Count == 0);

        placeCardsPool.ReleaseAll();

        this.places.ExtractItems();
        this.places.RemoveItems();

        setPlacesCts?.SafeCancelAndDispose();
        setPlacesCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        AddPlacesAsync(places, setPlacesCts.Token).Forget();
    }

    public void SetPlacesAsLoading(bool isVisible)
    {
        placesCanvas.enabled = !isVisible;

        placesLoading.SetActive(isVisible);

        if (isVisible)
            placesNoDataContainer.SetActive(false);
    }

    public void AddPlaces(List<PlaceCardComponentModel> places) =>
        AddPlacesAsync(places, disposeCts.Token).Forget();

    private async UniTask AddPlacesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel place in places)
        {
            PlaceCardComponentView placeCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(placeCardsPool, place, OnInfoClicked, OnJumpInClicked, OnVoteChanged, OnFavoriteClicked);

            var isPoi = false;

            if (poiCoords != null)
            {
                foreach (Vector2Int placeParcel in place.parcels)
                {
                    if (!poiCoords.Contains($"{placeParcel.x},{placeParcel.y}"))
                        continue;

                    isPoi = true;
                    break;
                }
            }

            placeCard.SetIsPOI(isPoi);

            OnFriendHandlerAdded?.Invoke(placeCard.friendsHandler);

            this.places.AddItem(placeCard);

            await UniTask.NextFrame(cancellationToken);
        }

        this.places.SetItemSizeForModel();
        await placeCardsPool.PrewarmAsync(places.Count, cancellationToken);
    }

    public void SetActive(bool isActive)
    {
        if (canvas.enabled == isActive)
            return;
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
            OnDisable();
    }

    public void SetShowMorePlacesButtonActive(bool isActive) =>
        showMorePlacesButtonContainer.gameObject.SetActive(isActive);

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        PlacesCardsConfigurator.Configure(placeModal, placeInfo, OnInfoClicked, OnJumpInClicked, OnVoteChanged, OnFavoriteClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void RestartScrollViewPosition() =>
        scrollView.verticalNormalizedPosition = 1;

    private void SetSortDropdownValue(string id, string title, bool notify)
    {
        sort = id;
        sortByDropdown.SetTitle(title);
        sortByDropdown.SelectOption(id, notify);
    }

    private static void DeselectButtons()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetPlaceCategories(List<(string id, string nameToShow)> placeCategories)
    {
        foreach (var category in placeCategories)
        {
            var categoryButton = Instantiate(placeCategoryButtonPrefab, placeCategoriesGrid);
            categoryButton.SetCategory(category.id, category.nameToShow);
            categoryButton.SetStatus(false);
            categoryButton.OnClick += OnCategoryButtonClicked;
            placeCategoryButtons.Add(categoryButton);
        }

        placeModal.SetAllPlaceCategories(placeCategories);
    }

    public void SetResultCounter(int totalResults) =>
        totalResultsCounter.text = $"Result ({totalResults})";

    private void OnCategoryButtonClicked(string category, bool isSelected)
    {
        foreach (PlaceCategoryButton categoryBtn in placeCategoryButtons)
            categoryBtn.SetStatus(category == categoryBtn.currenCategory && isSelected);

        if (isSelected)
        {
            filter = $"categories={category}";
            SetSortDropdownValue(BEST_FILTER_ID, BEST_FILTER_TEXT, false);
        }
        else
        {
            filter = "";
            SetSortDropdownValue(MOST_ACTIVE_FILTER_ID, MOST_ACTIVE_FILTER_TEXT, false);
        }

        OnFilterChanged?.Invoke();
    }
}
