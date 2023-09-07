using DCL;
using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils = DCL.Helpers.Utils;

public class FavoriteSubSectionComponentView : BaseComponentView, IFavoriteSubSectionComponentView
{
    private const int MAX_POOL_COUNT = 6;
    internal const string WORLDS_SUBSECTION_FF = "worlds_subsection";

    public int CurrentTilesPerRow { get; }
    public int CurrentGoingTilesPerRow { get; }

    [SerializeField] private GameObject minimalFavoriteList;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private GameObject fullFavoriteList;
    [SerializeField] private GameObject normalHeader;
    [SerializeField] private GameObject fullFavoriteListHeader;
    [SerializeField] private Button backButton;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject worldsSection;
    [SerializeField] private Transform placesParent;
    [SerializeField] private Transform worldsParent;
    [SerializeField] private RectTransform fullPlacesParent;
    [SerializeField] private RectTransform fullWorldsParent;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private PlaceCardComponentView placePrefab;
    [SerializeField] private GameObject loadingPlaces;
    [SerializeField] private GameObject loadingWorlds;
    [SerializeField] private GameObject loadingAll;
    [SerializeField] private Button showAllPlaces;
    [SerializeField] private Button showAllWorlds;
    [SerializeField] private Button showMore;

    [SerializeField] internal GameObject noPlaces;
    [SerializeField] internal GameObject noWorlds;
    [SerializeField] internal GameObject noResults;
    [SerializeField] private TMP_Text noPlacesText;
    [SerializeField] private TMP_Text noWorldsText;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;
    [SerializeField] internal PlaceCardComponentView worldCardModalPrefab;

    internal PlaceCardComponentView placeModal;
    internal PlaceCardComponentView worldModal;
    public event Action OnRequestFavorites;
    public event Action<int> OnRequestAllPlaces;
    public event Action<int> OnRequestAllWorlds;
    public event Action<PlaceCardComponentModel, int> OnPlaceInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;
    public event Action<PlaceCardComponentModel, int> OnWorldInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnWorldJumpInClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<string, bool> OnPlaceFavoriteChanged;

    private UnityObjectPool<PlaceCardComponentView> placesPool;
    internal List<PlaceCardComponentView> pooledPlaces = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> fullPlacesPool;
    internal List<PlaceCardComponentView> pooledFullPlaces = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> worldsPool;
    internal List<PlaceCardComponentView> pooledWorlds = new List<PlaceCardComponentView>();
    private UnityObjectPool<PlaceCardComponentView> fullWorldsPool;
    internal List<PlaceCardComponentView> pooledFullWorlds = new List<PlaceCardComponentView>();
    private int currentPage = 0;

    public override void Awake()
    {
        InitializePools();
        InitialiseButtonEvents();

        noPlaces.SetActive(false);
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);
        worldModal = PlacesAndEventsCardsFactory.GetWorldCardTemplateHiddenLazy(worldCardModalPrefab);
    }

    private void InitialiseButtonEvents()
    {
        if (showAllPlaces != null)
        {
            showAllPlaces.onClick.RemoveAllListeners();
            showAllPlaces.onClick.AddListener(RequestAllPlaces);
        }

        if (showAllWorlds != null)
        {
            showAllWorlds.onClick.RemoveAllListeners();
            showAllWorlds.onClick.AddListener(RequestAllWorlds);
        }

        if (showMore != null)
        {
            showMore.onClick.RemoveAllListeners();
            showMore.onClick.AddListener(RequestAdditionalPage);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonPressed);
        }
    }

    private void RequestAdditionalPage()
    {
        currentPage++;

        if(fullPlacesParent.gameObject.activeSelf)
            OnRequestAllPlaces?.Invoke(currentPage);
        else if(fullWorldsParent.gameObject.activeSelf)
            OnRequestAllWorlds?.Invoke(currentPage);
    }

    private void OnBackButtonPressed()
    {
        SetHeaderEnabled(false);
        minimalFavoriteList.SetActive(true);
        fullFavoriteList.SetActive(false);
    }

    private void RequestAllPlaces()
    {
        currentPage = 0;
        minimalFavoriteList.SetActive(false);
        fullFavoriteList.SetActive(true);
        fullPlacesParent.gameObject.SetActive(true);
        fullWorldsParent.gameObject.SetActive(false);
        loadingAll.SetActive(true);
        ClearFullPlacesPool();
        OnRequestAllPlaces?.Invoke(currentPage);
    }

    private void RequestAllWorlds()
    {
        currentPage = 0;
        minimalFavoriteList.SetActive(false);
        fullFavoriteList.SetActive(true);
        fullPlacesParent.gameObject.SetActive(false);
        fullWorldsParent.gameObject.SetActive(true);
        loadingAll.SetActive(true);
        ClearFullWorldsPool();
        OnRequestAllWorlds?.Invoke(currentPage);
    }

    public void ShowPlaces(List<PlaceCardComponentModel> places)
    {
        ClearPlacesPool();
        foreach (PlaceCardComponentModel placeCardComponentModel in places)
        {
            PlaceCardComponentView placeCardComponentView = placesPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            pooledPlaces.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        placesParent.gameObject.SetActive(true);
        loadingPlaces.gameObject.SetActive(false);

        showAllPlaces.gameObject.SetActive(places.Count == 6);
        if (places.Count == 0)
        {
            noPlaces.SetActive(true);
            noPlacesText.text = $"No favorite places found";
        }
        else
        {
            noPlaces.SetActive(false);
        }
        CheckAndSetNoResults();
    }

    public void ShowWorlds(List<PlaceCardComponentModel> worlds)
    {
        ClearWorldsPool();
        foreach (PlaceCardComponentModel placeCardComponentModel in worlds)
        {
            PlaceCardComponentView placeCardComponentView = worldsPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            pooledWorlds.Add(placeCardComponentView);
            ConfigureWorldCardActions(placeCardComponentView, placeCardComponentModel);
        }
        worldsParent.gameObject.SetActive(true);
        loadingWorlds.gameObject.SetActive(false);

        showAllWorlds.gameObject.SetActive(worlds.Count == 6);
        if (worlds.Count == 0)
        {
            noWorlds.SetActive(true);
            noWorldsText.text = $"No favorite worlds found";
        }
        else
        {
            noWorlds.SetActive(false);
        }
        CheckAndSetNoResults();
    }

    private void CheckAndSetNoResults()
    {
        if (noPlaces.activeSelf && noWorlds.activeSelf)
        {
            noResults.SetActive(true);
            minimalFavoriteList.SetActive(false);
        }
        else
        {
            noResults.SetActive(false);
            if(minimalFavoriteList.activeSelf == false)
                minimalFavoriteList.SetActive(true);
        }
        Utils.ForceRebuildLayoutImmediate(gridContainer);
    }

    private void ConfigurePlaceCardActions(PlaceCardComponentView view, PlaceCardComponentModel model)
    {
        view.onInfoClick.RemoveAllListeners();
        view.onBackgroundClick.RemoveAllListeners();
        view.onJumpInClick.RemoveAllListeners();
        view.OnFavoriteChanged -= ViewOnOnFavoriteChanged;
        view.OnVoteChanged -= ViewOnVoteChanged;
        view.onInfoClick.AddListener(()=>OnPlaceInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onBackgroundClick.AddListener(()=>OnPlaceInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onJumpInClick.AddListener(()=>OnPlaceJumpInClicked?.Invoke(model.placeInfo));
        view.OnFavoriteChanged += ViewOnOnFavoriteChanged;
        view.OnVoteChanged += ViewOnVoteChanged;
    }

    private void ConfigureWorldCardActions(PlaceCardComponentView view, PlaceCardComponentModel model)
    {
        view.onInfoClick.RemoveAllListeners();
        view.onBackgroundClick.RemoveAllListeners();
        view.onJumpInClick.RemoveAllListeners();
        view.OnFavoriteChanged -= ViewOnOnFavoriteChanged;
        view.OnVoteChanged -= ViewOnVoteChanged;
        view.onInfoClick.AddListener(()=>OnWorldInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onBackgroundClick.AddListener(()=>OnWorldInfoClicked?.Invoke(model, view.transform.GetSiblingIndex()));
        view.onJumpInClick.AddListener(()=>OnWorldJumpInClicked?.Invoke(model.placeInfo));
        view.OnFavoriteChanged += ViewOnOnFavoriteChanged;
        view.OnVoteChanged += ViewOnVoteChanged;
    }

    private void ViewOnVoteChanged(string placeName, bool? isUpvoted)
    {
        OnVoteChanged?.Invoke(placeName, isUpvoted);
    }

    private void ViewOnOnFavoriteChanged(string placeId, bool isFavorite)
    {
        OnPlaceFavoriteChanged?.Invoke(placeId, isFavorite);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeModel)
    {
        placeModal.Show();
        PlacesCardsConfigurator.Configure(placeModal, placeModel, null, OnPlaceJumpInClicked, OnVoteChanged, OnPlaceFavoriteChanged);
    }

    public void ShowWorldModal(PlaceCardComponentModel placeModel)
    {
        worldModal.Show();
        PlacesCardsConfigurator.Configure(worldModal, placeModel, null, OnWorldJumpInClicked, OnVoteChanged, OnPlaceFavoriteChanged);
    }

    public void ShowAllPlaces(List<PlaceCardComponentModel> places, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (PlaceCardComponentModel placeCardComponentModel in places)
        {
            PlaceCardComponentView placeCardComponentView = fullPlacesPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            placeCardComponentView.OnLoseFocus();
            placeCardComponentView.transform.SetAsLastSibling();
            pooledFullPlaces.Add(placeCardComponentView);
            ConfigurePlaceCardActions(placeCardComponentView, placeCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullPlacesParent);
    }

    public void ShowAllWorlds(List<PlaceCardComponentModel> worlds, bool showMoreButton)
    {
        showMore.gameObject.SetActive(showMoreButton);
        foreach (PlaceCardComponentModel placeCardComponentModel in worlds)
        {
            PlaceCardComponentView placeCardComponentView = fullWorldsPool.Get();
            placeCardComponentView.model = placeCardComponentModel;
            placeCardComponentView.RefreshControl();
            placeCardComponentView.OnLoseFocus();
            placeCardComponentView.transform.SetAsLastSibling();
            pooledFullWorlds.Add(placeCardComponentView);
            ConfigureWorldCardActions(placeCardComponentView, placeCardComponentModel);
        }
        loadingAll.SetActive(false);
        Utils.ForceRebuildLayoutImmediate(fullWorldsParent);
    }

    private void InitializePools()
    {
        placesPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, placesParent);
        placesPool.Prewarm(MAX_POOL_COUNT);
        worldsPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, worldsParent);
        worldsPool.Prewarm(MAX_POOL_COUNT);
        fullPlacesPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, fullPlacesParent);
        fullPlacesPool.Prewarm(MAX_POOL_COUNT);
        fullWorldsPool = new UnityObjectPool<PlaceCardComponentView>(placePrefab, fullWorldsParent);
        fullWorldsPool.Prewarm(MAX_POOL_COUNT);
    }

    public void RestartScrollViewPosition()
    {
    }

    private void CloseFullList()
    {
        minimalFavoriteList.SetActive(true);
        fullFavoriteList.SetActive(false);
    }

    public void SetAllAsLoading()
    {
        CloseFullList();
        placesParent.gameObject.SetActive(false);
        worldsParent.gameObject.SetActive(false);
        loadingPlaces.SetActive(true);
        loadingWorlds.SetActive(true);
    }

    public void SetHeaderEnabled(bool isFullHeaderActive)
    {
        normalHeader.SetActive(!isFullHeaderActive);
        fullFavoriteListHeader.SetActive(isFullHeaderActive);

        if (isFullHeaderActive)
            headerText.text = fullPlacesParent.gameObject.activeSelf ? "Favorite Places" : "Favorite Worlds";
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

    public override void OnEnable()
    {
        base.OnEnable();

        //Temporary until the full feature is released
        worldsSection.SetActive(DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(WORLDS_SUBSECTION_FF));
        OnRequestFavorites?.Invoke();
    }

    public override void RefreshControl()
    {
    }

    public override void Dispose()
    {
        ClearPlacesPool();
        ClearFullPlacesPool();
        ClearWorldsPool();
        ClearFullWorldsPool();
    }

    public void HideWorldModal()
    {
        if(worldModal != null)
            worldModal.Hide();
    }

    private void ClearPlacesPool()
    {
        foreach (var pooledEvent in pooledPlaces)
            placesPool.Release(pooledEvent);
        pooledPlaces.Clear();
    }

    private void ClearFullPlacesPool(){
        foreach (var pooledEvent in pooledFullPlaces)
            fullPlacesPool.Release(pooledEvent);
        pooledFullPlaces.Clear();
    }

    private void ClearWorldsPool()
    {
        foreach (var pooledWorld in pooledWorlds)
            worldsPool.Release(pooledWorld);
        pooledWorlds.Clear();
    }

    private void ClearFullWorldsPool(){
        foreach (var pooledWorld in pooledFullWorlds)
            fullWorldsPool.Release(pooledWorld);
        pooledFullWorlds.Clear();
    }
}
