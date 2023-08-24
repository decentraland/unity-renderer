using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public interface IWorldsSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    string filter { get; }
    string sort { get; }

    /// <summary>
    /// Number of worlds per row that fit with the current worlds grid configuration.
    /// </summary>
    int currentWorldsPerRow { get; }

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

    event Action<string, bool?> OnVoteChanged;

    /// <summary>
    /// It will be triggered when the favorite button is clicked.
    /// </summary>
    event Action<string, bool> OnFavoriteClicked;

    /// <summary>
    /// It will be triggered when a new friend handler is added by a world card.
    /// </summary>
    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnWorldsSubSectionEnable;
    event Action OnSortingChanged;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMoreWorldsClicked;

    /// <summary>
    /// Set the worlds component with a list of worlds.
    /// </summary>
    /// <param name="worlds">List of worlds (place component model) to be loaded.</param>
    void SetWorlds(List<PlaceCardComponentModel> worlds);

    /// <summary>
    /// Add a list of worlds in the worlds component.
    /// </summary>
    /// <param name="worlds">List of worlds (place component model) to be added.</param>
    void AddWorlds(List<PlaceCardComponentModel> worlds);

    /// <summary>
    /// Set the worlds component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetWorldsAsLoading(bool isVisible);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreWorldsButtonActive(bool isActive);

    /// <summary>
    /// Shows the Wolrd Card modal with the provided information.
    /// </summary>
    /// <param name="worldInfo">World (place component model) to be loaded in the card.</param>
    void ShowWorldModal(PlaceCardComponentModel worldInfo);

    /// <summary>
    /// Hides the Wolrd Card modal.
    /// </summary>
    void HideWorldModal();

    /// <summary>
    /// Configure the needed pools for the worlds instantiation.
    /// </summary>
    void ConfigurePools();

    void SetShowMoreButtonActive(bool isActive);

    void SetPOICoords(List<string> poiList);
}
