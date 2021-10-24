using DCL;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPlacesSubSectionComponentView
{
    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the info button is clicked.
    /// </summary>
    event Action<PlaceCardComponentModel> OnInfoClicked;

    /// <summary>
    /// It will be triggered when the JumpIn button is clicked.
    /// </summary>
    event Action<HotScenesController.HotSceneInfo> OnJumpInClicked;

    /// <summary>
    /// It will be triggered when a new friend handler is added by a place card.
    /// </summary>
    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnPlacesSubSectionEnable;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMorePlacesClicked;

    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    /// <summary>
    /// Number of places per row that fit with the current places grid configuration.
    /// </summary>
    int currentPlacesPerRow { get; }

    /// <summary>
    /// Set the places component with a list of places.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    void SetPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Add a list of places in the places component.
    /// </summary>
    /// <param name="places">List of places (model) to be added.</param>
    void AddPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Set the places component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetPlacesAsLoading(bool isVisible);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMorePlacesButtonActive(bool isActive);

    /// <summary>
    /// Shows the Place Card modal with the provided information.
    /// </summary>
    /// <param name="placeInfo">Place (model) to be loaded in the card.</param>
    void ShowPlaceModal(PlaceCardComponentModel placeInfo);

    /// <summary>
    /// Hides the Place Card modal.
    /// </summary>
    void HidePlaceModal();

    /// <summary>
    /// Set the current scroll view position to 1.
    /// </summary>
    void RestartScrollViewPosition();
}

public class PlacesSubSectionComponentView : BaseComponentView, IPlacesSubSectionComponentView
{
    internal const string PLACE_CARDS_POOL_NAME = "PlaceCardsPool";

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView places;
    [SerializeField] internal GameObject placesLoading;
    [SerializeField] internal TMP_Text placesNoDataText;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal GameObject showMorePlacesButtonContainer;
    [SerializeField] internal ButtonComponentView showMorePlacesButton;

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnJumpInClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnPlacesSubSectionEnable;
    public event Action OnShowMorePlacesClicked;

    internal PlaceCardComponentView placeModal;
    internal Pool placeCardsPool;

    public Color[] currentFriendColors => friendColors;

    public int currentPlacesPerRow => places.currentItemsPerRow;

    public override void OnEnable() { OnPlacesSubSectionEnable?.Invoke(); }

    public override void Start()
    {
        ConfigurePlaceCardModal();
        ConfigurePlaceCardsPool();

        places.RemoveItems();

        showMorePlacesButton.onClick.RemoveAllListeners();
        showMorePlacesButton.onClick.AddListener(() => OnShowMorePlacesClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void RefreshControl() { places.RefreshControl(); }

    public override void Dispose()
    {
        base.Dispose();

        showMorePlacesButton.onClick.RemoveAllListeners();
    }

    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        this.places.ExtractItems();
        placeCardsPool.ReleaseAll();
        List<BaseComponentView> placeComponentsToAdd = InstantiateAndConfigurePlaceCards(places);
        this.places.SetItems(placeComponentsToAdd);
        placesNoDataText.gameObject.SetActive(places.Count == 0);
    }

    public void AddPlaces(List<PlaceCardComponentModel> places)
    {
        List<BaseComponentView> placeComponentsToAdd = InstantiateAndConfigurePlaceCards(places);
        foreach (var place in placeComponentsToAdd)
        {
            this.places.AddItem(place);
        }
    }

    public void SetPlacesAsLoading(bool isVisible)
    {
        places.gameObject.SetActive(!isVisible);
        placesLoading.SetActive(isVisible);

        if (isVisible)
            placesNoDataText.gameObject.SetActive(false);
    }

    public void SetShowMorePlacesButtonActive(bool isActive) { showMorePlacesButtonContainer.gameObject.SetActive(isActive); }

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        ConfigurePlaceCard(placeModal, placeInfo);
    }

    public void HidePlaceModal() { placeModal.Hide(); }

    public void RestartScrollViewPosition() { scrollView.verticalNormalizedPosition = 1; }

    internal void ConfigurePlaceCardModal()
    {
        placeModal = Instantiate(placeCardModalPrefab);
        placeModal.Hide(true);
    }

    internal void ConfigurePlaceCardsPool()
    {
        placeCardsPool = PoolManager.i.GetPool(PLACE_CARDS_POOL_NAME);
        if (placeCardsPool == null)
        {
            placeCardsPool = PoolManager.i.AddPool(
                PLACE_CARDS_POOL_NAME,
                Instantiate(placeCardPrefab).gameObject,
                maxPrewarmCount: 200,
                isPersistent: true);
        }
    }

    internal List<BaseComponentView> InstantiateAndConfigurePlaceCards(List<PlaceCardComponentModel> places)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (PlaceCardComponentModel placeInfo in places)
        {
            PlaceCardComponentView placeGO = placeCardsPool.Get().gameObject.GetComponent<PlaceCardComponentView>();
            ConfigurePlaceCard(placeGO, placeInfo);
            OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);
            instantiatedPlaces.Add(placeGO);
        }

        return instantiatedPlaces;
    }

    internal void ConfigurePlaceCard(PlaceCardComponentView placeCard, PlaceCardComponentModel placeInfo)
    {
        placeCard.Configure(placeInfo);
        placeCard.onInfoClick?.RemoveAllListeners();
        placeCard.onInfoClick?.AddListener(() => OnInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick?.RemoveAllListeners();
        placeCard.onJumpInClick?.AddListener(() => OnJumpInClicked?.Invoke(placeInfo.hotSceneInfo));
    }
}