using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public interface IFavoritesSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    /// <summary>
    /// Number of places per row that fit with the current places grid configuration.
    /// </summary>
    int currentFavoritePlacesPerRow { get; }

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
    event Action<IHotScenesController.PlaceInfo> OnJumpInClicked;

    /// <summary>
    /// It will be triggered when the favorites button is clicked.
    /// </summary>
    event Action<string, bool> OnFavoriteClicked;

    /// <summary>
    /// It will be triggered when a new friend handler is added by a place card.
    /// </summary>
    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnFavoriteSubSectionEnable;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMoreFavoritesClicked;

    /// <summary>
    /// Set the favorite places component with a list of favorites.
    /// </summary>
    /// <param name="places">List of favorite places (model) to be loaded.</param>
    void SetFavorites(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Add a list of favorite places in the places component.
    /// </summary>
    /// <param name="places">List of places (model) to be added.</param>
    void AddFavorites(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreFavoritesButtonActive(bool isActive);

    /// <summary>
    /// Shows the Place Card modal with the provided information.
    /// </summary>
    /// <param name="placeInfo">Favorite place (model) to be loaded in the card.</param>
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
