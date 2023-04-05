using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public interface IPlacesSubSectionComponentView:IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    /// <summary>
    /// Number of places per row that fit with the current places grid configuration.
    /// </summary>
    int currentPlacesPerRow { get; }

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
    event Action<IHotScenesController.HotSceneInfo> OnJumpInClicked;

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
    /// Configure the needed pools for the places instantiation.
    /// </summary>
    void ConfigurePools();

    void SetShowMoreButtonActive(bool isActive);
}
