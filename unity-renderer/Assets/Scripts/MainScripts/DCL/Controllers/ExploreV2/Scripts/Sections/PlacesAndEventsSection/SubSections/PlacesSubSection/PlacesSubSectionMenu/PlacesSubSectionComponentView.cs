using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IPlacesSubSectionComponentView
{
    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

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
    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal GridContainerComponentView places;
    [SerializeField] internal GameObject placesLoading;
    [SerializeField] internal TMP_Text placesNoDataText;

    public event Action OnReady;
    public event Action OnPlacesSubSectionEnable;

    internal PlaceCardComponentView placeModal;

    private void OnEnable() { OnPlacesSubSectionEnable?.Invoke(); }

    public override void PostInitialization()
    {
        StartCoroutine(WaitForComponentsInitialization());

        placeModal = GameObject.Instantiate(placeCardModalPrefab);
        placeModal.gameObject.SetActive(false);
    }

    public override void RefreshControl() { places.RefreshControl(); }

    public IEnumerator WaitForComponentsInitialization()
    {
        yield return new WaitUntil(() => places.isFullyInitialized);

        OnReady?.Invoke();
    }

    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        List<BaseComponentView> placeComponentsToAdd = IntantiateAndConfigurePlaceCards(places, placeCardPrefab);
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

    internal List<BaseComponentView> IntantiateAndConfigurePlaceCards(List<PlaceCardComponentModel> places, PlaceCardComponentView prefabToUse)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (PlaceCardComponentModel placeInfo in places)
        {
            PlaceCardComponentView placeGO = GameObject.Instantiate(prefabToUse);
            placeGO.Configure(placeInfo);
            instantiatedPlaces.Add(placeGO);
        }

        return instantiatedPlaces;
    }
}