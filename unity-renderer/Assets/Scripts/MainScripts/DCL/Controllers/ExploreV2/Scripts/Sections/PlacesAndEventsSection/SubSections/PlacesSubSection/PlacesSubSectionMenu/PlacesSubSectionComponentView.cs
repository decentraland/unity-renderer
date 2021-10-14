using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IPlacesSubSectionComponentView
{
    Color[] currentFriendColors { get; }

    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnPlacesSubSectionEnable;

    /// <summary>
    /// Set the places component with a list of places.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    void SetPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Set the places component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetPlacesAsLoading(bool isVisible);

    /// <summary>
    /// Shows the Place Card modal with the provided information.
    /// </summary>
    /// <param name="placeInfo">Place (model) to be loaded in the card.</param>
    void ShowPlaceModal(PlaceCardComponentModel placeInfo);

    /// <summary>
    /// Hides the Place Card modal.
    /// </summary>
    void HidePlaceModal();
}

public class PlacesSubSectionComponentView : BaseComponentView, IPlacesSubSectionComponentView
{
    internal const string PLACE_CARDS_POOL_NAME = "PlaceCardsPool";

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal GridContainerComponentView places;
    [SerializeField] internal GameObject placesLoading;
    [SerializeField] internal TMP_Text placesNoDataText;
    [SerializeField] internal Color[] friendColors = null;

    public event Action OnReady;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnPlacesSubSectionEnable;

    internal PlaceCardComponentView placeModal;
    internal Pool placeCardsPool;

    public Color[] currentFriendColors => friendColors;

    private void OnEnable() { OnPlacesSubSectionEnable?.Invoke(); }

    public override void PostInitialization()
    {
        ConfigurePlaceCardModal();
        ConfigurePlaceCardsPool();
        StartCoroutine(WaitForComponentsInitialization());
    }

    internal IEnumerator WaitForComponentsInitialization()
    {
        yield return new WaitUntil(() => places.isFullyInitialized);

        places.RemoveItems();
        OnReady?.Invoke();
    }

    public override void RefreshControl() { places.RefreshControl(); }

    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        this.places.ExtractItems();
        placeCardsPool.ReleaseAll();
        List<BaseComponentView> placeComponentsToAdd = InstantiateAndConfigurePlaceCards(places);
        this.places.SetItems(placeComponentsToAdd, false);
        placesNoDataText.gameObject.SetActive(places.Count == 0);
    }

    public void SetPlacesAsLoading(bool isVisible)
    {
        places.gameObject.SetActive(!isVisible);
        placesLoading.SetActive(isVisible);
        placesNoDataText.gameObject.SetActive(false);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.gameObject.SetActive(true);
        placeModal.Configure(placeInfo);
    }

    public void HidePlaceModal() { placeModal.gameObject.SetActive(false); }

    internal void ConfigurePlaceCardModal()
    {
        placeModal = Instantiate(placeCardModalPrefab);
        placeModal.gameObject.SetActive(false);
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
            placeGO.Configure(placeInfo);

            if (placeGO.isFullyInitialized)
                OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);
            else
                placeGO.OnFullyInitialized += () => OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);

            instantiatedPlaces.Add(placeGO);
        }

        return instantiatedPlaces;
    }
}