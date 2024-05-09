using DCL.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IPlaceCardComponentView
{
    event Action<string, bool?> OnVoteChanged;
    event Action<string, bool> OnFavoriteChanged;
    event Action<Vector2Int> OnPressedLinkCopy;
    event Action<Vector2Int, string> OnPressedTwitterButton;

    IFriendTrackerHandler friendsHandler { get; set; }

    /// <summary>
    /// Event that will be triggered when the jumpIn button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onJumpInClick { get; }

    /// <summary>
    /// Event that will be triggered when the info button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onInfoClick { get; }

    /// <summary>
    /// Event that will be triggered when the background button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onBackgroundClick { get; }

    /// <summary>
    /// Set the place picture directly from a sprite.
    /// </summary>
    /// <param name="sprite">Place picture (sprite).</param>
    void SetPlacePicture(Sprite sprite);

    /// <summary>
    /// Set the place picture from a 2D texture.
    /// </summary>
    /// <param name="texture">Place picture (texture).</param>
    void SetPlacePicture(Texture2D texture);

    /// <summary>
    /// Set the place picture from an uri.
    /// </summary>
    /// <param name="uri">Place picture (url).</param>
    void SetPlacePicture(string uri);

    /// <summary>
    /// Set the place name in the card.
    /// </summary>
    /// <param name="newText">New place name.</param>
    void SetPlaceName(string newText);

    /// <summary>
    /// Set the place description in the card.
    /// </summary>
    /// <param name="newText">New place description.</param>
    void SetPlaceDescription(string newText);

    /// <summary>
    /// Set the place organizer in the card.
    /// </summary>
    /// <param name="newText">New place organizer.</param>
    void SetPlaceAuthor(string newText);

    /// <summary>
    /// Set the the number of users in the place.
    /// </summary>
    /// <param name="newNumberOfUsers">Number of users.</param>
    void SetNumberOfUsers(int newNumberOfUsers);

    /// <summary>
    /// Set the the number of visits in the last 30 days.
    /// </summary>
    /// <param name="userVisits">Number of visits.</param>
    void SetUserVisits(int userVisits);

    /// <summary>
    /// Set the the user rating that considers only up and down votes with at least 100 VotingPower.
    /// </summary>
    /// <param name="userRating">User rating value.</param>
    void SetUserRating(float? userRating);

    /// <summary>
    /// Set the place coords.
    /// </summary>
    /// <param name="newCoords">Place coords.</param>
    void SetCoords(Vector2Int newCoords);

    /// <summary>
    /// Set the parcels contained in the place.
    /// </summary>
    /// <param name="parcels">List of parcels.</param>
    void SetParcels(Vector2Int[] parcels);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the card info.</param>
    void SetLoadingIndicatorVisible(bool isVisible);

    /// <summary>
    /// Set the place as favorite or not.
    /// </summary>
    /// <param name="isFavorite"></param>
    /// <param name="placeId"></param>
    void SetFavoriteButton(bool isFavorite, string placeId);

    /// <summary>
    /// Vote/Unvote the place.
    /// </summary>
    /// <param name="isUpvoted"></param>
    /// <param name="isDownvoted"></param>
    void SetVoteButtons(bool isUpvoted, bool isDownvoted);

    /// <summary>
    /// Set the total amount of votes of the place.
    /// </summary>
    /// <param name="totalVotes"></param>
    void SetTotalVotes(int totalVotes);

    /// <summary>
    /// Set the amount of favorites set in the place.
    /// </summary>
    /// <param name="numberOfFavorites"></param>
    void SetNumberOfFavorites(int numberOfFavorites);

    /// <summary>
    /// Set the date of the last update of this place.
    /// </summary>
    /// <param name="updatedAt"></param>
    void SetDeployedAt(string updatedAt);

    /// <summary>
    /// Set the place card as point of interest.
    /// </summary>
    /// <param name="isPOI">Tru for set it as POI.</param>
    void SetIsPOI(bool isPOI);

    void SetActive(bool isActive);

    void SetAgeRating(SceneContentCategory contentCategory);

    void SetAllPlaceCategories(List<(string id, string nameToShow)> placeCategories);

    void SetAppearsOn(string[] categories);
}
