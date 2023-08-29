using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.Controllers.HotScenes;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class WorldsSubSectionComponentView : BaseComponentView, IWorldsSubSectionComponentView
{
    internal const string WORLD_CARDS_POOL_NAME = "Worlds_WorldCardsPool";
    private const int WORLD_CARDS_POOL_PREWARM = 20;
    private const string MOST_ACTIVE_FILTER_ID = "most_active";
    private const string MOST_ACTIVE_FILTER_TEXT = "Most active";
    private const string BEST_FILTER_ID = "like_rate";
    private const string BEST_FILTER_TEXT = "Best";

    private readonly CancellationTokenSource disposeCts = new ();
    private CancellationTokenSource setWorldsCts = new ();

    private List<string> poiCoords;

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView worldCardPrefab;
    [SerializeField] internal PlaceCardComponentView worldCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView worlds;
    [SerializeField] internal GameObject worldsLoading;
    [SerializeField] internal GameObject worldsNoDataContainer;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal GameObject showMoreWorldsButtonContainer;
    [SerializeField] internal ButtonComponentView showMoreWorldsButton;
    [SerializeField] internal DropdownComponentView sortByDropdown;

    [SerializeField] private Canvas canvas;

    internal PlaceCardComponentView worldModal;
    internal Pool worldCardsPool;
    private Canvas worldsCanvas;

    public Color[] currentFriendColors => friendColors;

    public int currentWorldsPerRow => worlds.currentItemsPerRow;

    public void SetAllAsLoading() => SetWorldsAsLoading(true);
    public void SetShowMoreButtonActive(bool isActive) => SetShowMoreWorldsButtonActive(isActive);

    public void SetPOICoords(List<string> poiList) =>
        poiCoords = poiList;

    public int CurrentTilesPerRow => currentWorldsPerRow;
    public int CurrentGoingTilesPerRow { get; }
    public string filter { get; private set; }
    public string sort { get; private set; }

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnJumpInClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<string, bool> OnFavoriteClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnWorldsSubSectionEnable;
    public event Action OnSortingChanged;
    public event Action OnShowMoreWorldsClicked;

    public override void Awake()
    {
        base.Awake();
        worldsCanvas = worlds.GetComponent<Canvas>();
    }

    public void Start()
    {
        worldModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(worldCardModalPrefab);

        worlds.RemoveItems();
        LoadSortByDropdown();

        showMoreWorldsButton.onClick.RemoveAllListeners();
        showMoreWorldsButton.onClick.AddListener(() => OnShowMoreWorldsClicked?.Invoke());
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
        filter = "";
        SetSortDropdownValue(MOST_ACTIVE_FILTER_ID, MOST_ACTIVE_FILTER_TEXT, false);
        OnWorldsSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        disposeCts?.SafeCancelAndDispose();
        setWorldsCts?.SafeCancelAndDispose();

        showMoreWorldsButton.onClick.RemoveAllListeners();

        sortByDropdown.OnOptionSelectionChanged -= SortByDropdownValueChanged;

        worlds.Dispose();

        if (worldModal != null)
        {
            worldModal.Dispose();
            Destroy(worldModal.gameObject);
        }
    }

    public void ConfigurePools() =>
        worldCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(WORLD_CARDS_POOL_NAME, worldCardPrefab, WORLD_CARDS_POOL_PREWARM);

    public override void RefreshControl() =>
        worlds.RefreshControl();

    public void SetWorlds(List<PlaceCardComponentModel> worlds)
    {
        SetWorldsAsLoading(false);
        worldsNoDataContainer.SetActive(worlds.Count == 0);

        worldCardsPool.ReleaseAll();

        this.worlds.ExtractItems();
        this.worlds.RemoveItems();

        setWorldsCts?.SafeCancelAndDispose();
        setWorldsCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        AddWorldsAsync(worlds, setWorldsCts.Token).Forget();
    }

    public void SetWorldsAsLoading(bool isVisible)
    {
        worldsCanvas.enabled = !isVisible;

        worldsLoading.SetActive(isVisible);

        if (isVisible)
            worldsNoDataContainer.SetActive(false);
    }

    public void AddWorlds(List<PlaceCardComponentModel> worlds) =>
        AddWorldsAsync(worlds, disposeCts.Token).Forget();

    private async UniTask AddWorldsAsync(List<PlaceCardComponentModel> worlds, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel world in worlds)
        {
            PlaceCardComponentView worldCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(worldCardsPool, world, OnInfoClicked, OnJumpInClicked, OnVoteChanged, OnFavoriteClicked);

            var isPoi = false;

            if (poiCoords != null)
            {
                foreach (Vector2Int worldParcel in world.parcels)
                {
                    if (!poiCoords.Contains($"{worldParcel.x},{worldParcel.y}"))
                        continue;

                    isPoi = true;
                    break;
                }
            }

            worldCard.SetIsPOI(isPoi);

            OnFriendHandlerAdded?.Invoke(worldCard.friendsHandler);

            this.worlds.AddItem(worldCard);

            await UniTask.NextFrame(cancellationToken);
        }

        this.worlds.SetItemSizeForModel();
        await worldCardsPool.PrewarmAsync(worlds.Count, cancellationToken);
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

    public void SetShowMoreWorldsButtonActive(bool isActive) =>
        showMoreWorldsButtonContainer.gameObject.SetActive(isActive);

    public void ShowWorldModal(PlaceCardComponentModel worldInfo)
    {
        worldModal.Show();
        PlacesCardsConfigurator.Configure(worldModal, worldInfo, OnInfoClicked, OnJumpInClicked, OnVoteChanged, OnFavoriteClicked);
    }

    public void HideWorldModal()
    {
        if (worldModal != null)
            worldModal.Hide();
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
}
