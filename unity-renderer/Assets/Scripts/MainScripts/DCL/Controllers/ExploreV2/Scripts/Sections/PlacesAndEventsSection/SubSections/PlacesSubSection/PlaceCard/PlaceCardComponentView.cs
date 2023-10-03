using DCL;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPlaceCardComponentView
{
    event Action<string, bool?> OnVoteChanged;

    FriendsHandler friendsHandler { get; set; }

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

    /// <summary>
    /// Set the the age rating set for the place.
    /// </summary>
    /// <param name="contentCategory">Age category.</param>
    void SetAgeRating(SceneContentCategory contentCategory);
}

public class PlaceCardComponentView : BaseComponentView, IPlaceCardComponentView, IComponentModelConfig<PlaceCardComponentModel>
{
    internal const int THMBL_MARKETPLACE_WIDTH = 196;
    internal const int THMBL_MARKETPLACE_HEIGHT = 143;
    internal const int THMBL_MARKETPLACE_SIZEFACTOR = 50;
    private const string NO_DESCRIPTION_TEXT = "No description.";

    [Header("Assets References")]
    [SerializeField] internal FriendHeadForPlaceCardComponentView friendHeadPrefab;
    [SerializeField] internal UserProfile ownUserProfile;

    [Header("Prefab References")]
    [SerializeField] internal GameObject poiMark;
    [SerializeField] internal ImageComponentView placeImage;
    [SerializeField] internal TMP_Text placeNameOnIdleText;
    [SerializeField] internal TMP_Text placeNameOnFocusText;
    [SerializeField] internal TMP_Text placeDescText;
    [SerializeField] internal TMP_Text placeAuthorOnIdleText;
    [SerializeField] internal TMP_Text placeAuthorOnFocusText;
    [SerializeField] internal RectTransform userVisitsAndRatingContainer;
    [SerializeField] internal TMP_Text userVisitsText;
    [SerializeField] internal TMP_Text userRatingText;
    [SerializeField] internal RectTransform numberOfUsersContainer;
    [SerializeField] internal TMP_Text numberOfUsersText;
    [SerializeField] internal TMP_Text coordsText;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ButtonComponentView backgroundButton;
    [SerializeField] internal ButtonComponentView infoButton;
    [SerializeField] internal ButtonComponentView upvoteButton;
    [SerializeField] internal ButtonComponentView downvoteButton;
    [SerializeField] internal GameObject upvoteOff;
    [SerializeField] internal GameObject upvoteOn;
    [SerializeField] internal GameObject downvoteOff;
    [SerializeField] internal GameObject downvoteOn;
    [SerializeField] internal TMP_Text totalVotesText;
    [SerializeField] internal ButtonComponentView jumpinButton;
    [SerializeField] internal GridContainerComponentView friendsGrid;
    [SerializeField] internal GameObject imageContainer;
    [SerializeField] internal GameObject placeInfoContainer;
    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal GameObject cardSelectionFrame;
    [SerializeField] internal VerticalLayoutGroup contentVerticalLayout;
    [SerializeField] internal VerticalLayoutGroup infoVerticalLayout;
    [SerializeField] internal PlaceCardAnimatorBase cardAnimator;
    [SerializeField] internal FavoriteButtonComponentView favoriteButton;
    [SerializeField] internal GameObject favoriteButtonContainer;
    [SerializeField] internal TMP_Text numberOfFavoritesText;
    [SerializeField] internal TMP_Text updatedAtText;
    [SerializeField] internal ScrollRect scroll;
    [SerializeField] internal TMP_Text ageRatingText;
    [SerializeField] internal GameObject ageRatingOutline;

    [Header("Configuration")]
    [SerializeField] internal Sprite defaultPicture;
    [SerializeField] internal bool isPlaceCardModal;
    [SerializeField] internal PlaceCardComponentModel model;

    public FriendsHandler friendsHandler { get; set; }
    internal MapInfoHandler mapInfoHandler { get; set; }

    internal readonly Dictionary<string, BaseComponentView> currentFriendHeads = new ();

    public Button.ButtonClickedEvent onJumpInClick => jumpinButton != null ? jumpinButton.onClick : new Button.ButtonClickedEvent();
    public Button.ButtonClickedEvent onInfoClick => infoButton != null ? infoButton.onClick : new Button.ButtonClickedEvent();
    public Button.ButtonClickedEvent onBackgroundClick => backgroundButton != null ? backgroundButton.onClick : new Button.ButtonClickedEvent();

    public event Action<string, bool> OnFavoriteChanged;

    private bool thumbnailFromMarketPlaceRequested;
    public event Action<string, bool?> OnVoteChanged;

    public override void Awake()
    {
        base.Awake();

        if (placeImage != null)
            placeImage.OnLoaded += OnPlaceImageLoaded;

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (closeAction != null)
            closeAction.OnTriggered += OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);

        if(upvoteButton != null)
            upvoteButton.onClick.AddListener(() => ChangeVote(true));

        if(downvoteButton != null)
            downvoteButton.onClick.AddListener(() => ChangeVote(false));

        CleanFriendHeadsItems();
    }

    private void ChangeVote(bool upvote)
    {
        if (upvote)
        {
            OnVoteChanged?.Invoke(model.placeInfo.id, model.isUpvote ? (bool?)null : true);

            if (ownUserProfile != null && ownUserProfile.isGuest)
                return;

            model.isUpvote = !model.isUpvote;
            model.isDownvote = false;
        }
        else
        {
            OnVoteChanged?.Invoke(model.placeInfo.id, model.isDownvote ? (bool?)null : false);

            if (ownUserProfile != null && ownUserProfile.isGuest)
                return;

            model.isDownvote = !model.isDownvote;
            model.isUpvote = false;
        }
        SetVoteButtons(model.isUpvote, model.isDownvote);
    }

    public void Configure(PlaceCardComponentModel newModel)
    {
        model = newModel;

        InitializeFriendsTracker();

        if (mapInfoHandler != null)
            mapInfoHandler.SetMinimapSceneInfo(model.placeInfo);

        RefreshControl();
    }

    public override void RefreshControl()
    {
        thumbnailFromMarketPlaceRequested = false;

        if (model == null)
            return;

        SetParcels(model.parcels);

        if (model.placePictureSprite != null)
            SetPlacePicture(model.placePictureSprite);
        else if (model.placePictureTexture != null)
            SetPlacePicture(model.placePictureTexture);
        else if (!string.IsNullOrEmpty(model.placePictureUri))
            SetPlacePicture(model.placePictureUri);
        else
            OnPlaceImageLoaded(null);

        SetPlaceName(model.placeName);
        SetPlaceDescription(model.placeDescription);
        SetPlaceAuthor(model.placeAuthor);
        SetUserVisits(model.userVisits);
        SetUserRating(model.userRating);
        SetNumberOfUsers(model.numberOfUsers);
        SetCoords(model.coords);
        SetFavoriteButton(model.isFavorite, model.placeInfo.id);

        //Temporary untill the release of the functionality
        if (!DataStore.i.HUDs.enableFavoritePlaces.Get())
        {
            if(favoriteButtonContainer != null)
                favoriteButtonContainer.SetActive(false);
        }

        SetVoteButtons(model.isUpvote, model.isDownvote);
        SetTotalVotes(model.totalVotes);
        SetNumberOfFavorites(model.numberOfFavorites);
        SetDeployedAt(model.deployedAt);
        SetIsPOI(model.isPOI);
        SetAgeRating(model.ageRating);
        ResetScrollPosition();
        RebuildCardLayouts();
    }

    public void SetFavoriteButton(bool isFavorite, string placeId)
    {
        model.isFavorite = isFavorite;
        model.placeInfo.id = placeId;

        if (favoriteButton == null)
            return;
        favoriteButton.gameObject.SetActive(true);
        favoriteButton.Configure(new FavoriteButtonComponentModel()
        {
            isFavorite = isFavorite,
            placeUUID = placeId
        });
        ShowFavoriteButton(isFavorite);

        favoriteButton.OnFavoriteChange -= FavoriteValueChanged;
        favoriteButton.OnFavoriteChange += FavoriteValueChanged;
    }

    public void SetVoteButtons(bool isUpvoted, bool isDownvoted)
    {
        model.isUpvote = isUpvoted;
        model.isDownvote = isDownvoted;

        if(upvoteOn == null || upvoteOff == null || downvoteOn == null || downvoteOff == null)
            return;

        upvoteOn.SetActive(isUpvoted);
        upvoteOff.SetActive(!isUpvoted);
        downvoteOn.SetActive(isDownvoted);
        downvoteOff.SetActive(!isDownvoted);
    }

    public void SetTotalVotes(int totalVotes)
    {
        model.totalVotes = totalVotes;

        if (totalVotesText == null)
            return;

        totalVotesText.text = $"({totalVotes})";
    }

    public void SetNumberOfFavorites(int numberOfFavorites)
    {
        model.numberOfFavorites = numberOfFavorites;

        if (numberOfFavoritesText != null)
            numberOfFavoritesText.text = FormatNumber(numberOfFavorites);
    }

    public void SetDeployedAt(string updatedAt)
    {
        model.deployedAt = updatedAt;

        if (updatedAtText == null)
            return;

        updatedAtText.text = DateTime.TryParse(updatedAt, out DateTime updateAtDT) ?
            updateAtDT.ToString("dd/MM/yyyy") :
            "-";
    }

    public void SetIsPOI(bool isPOI)
    {
        model.isPOI = isPOI;

        if (poiMark == null)
            return;

        poiMark.SetActive(isPOI);

    }

    public void SetAgeRating(SceneContentCategory contentCategory)
    {
        model.ageRating = contentCategory;

        if (ageRatingText != null)
            ageRatingText.text = contentCategory switch
                                 {
                                     SceneContentCategory.ADULT => "PG 18+",
                                     SceneContentCategory.RESTRICTED => "RESTRICTED",
                                     _ => "PG 13+",
                                 };

        if (ageRatingOutline != null)
            ageRatingOutline.SetActive(contentCategory != SceneContentCategory.RESTRICTED);
    }

    private void FavoriteValueChanged(string placeUUID, bool isFavorite)
    {
        OnFavoriteChanged?.Invoke(placeUUID, isFavorite);

        if (ownUserProfile != null && ownUserProfile.isGuest)
        {
            favoriteButton.Configure(new FavoriteButtonComponentModel
            {
                placeUUID = placeUUID,
                isFavorite = false,
            });
            return;
        }

        model.isFavorite = true;
        model.placeInfo.id = placeUUID;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(true);

        ShowFavoriteButton(true);

        if(cardAnimator != null)
            cardAnimator.Focus();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        ShowFavoriteButton(false);

        if(cardAnimator != null)
            cardAnimator.Idle();
    }

    public override void Show(bool instant = false)
    {
        base.Show(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(true);
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(false);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (placeImage != null)
        {
            placeImage.OnLoaded -= OnPlaceImageLoaded;
            placeImage.Dispose();
        }

        if (closeCardButton != null)
            closeCardButton.onClick.RemoveAllListeners();

        if (closeAction != null)
            closeAction.OnTriggered -= OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.RemoveAllListeners();

        if (friendsHandler != null)
        {
            friendsHandler.OnFriendAddedEvent -= OnFriendAdded;
            friendsHandler.OnFriendRemovedEvent -= OnFriendRemoved;
        }

        if (friendsGrid != null)
            friendsGrid.Dispose();

        if(favoriteButton != null)
            favoriteButton.OnFavoriteChange -= FavoriteValueChanged;
    }

    private void ShowFavoriteButton(bool show)
    {
        if(favoriteButton != null && !favoriteButton.IsFavorite())
            favoriteButton.gameObject.SetActive(show);
    }

    public void SetPlacePicture(Sprite sprite)
    {
        if (sprite == null && defaultPicture != null)
            sprite = defaultPicture;

        model.placePictureSprite = sprite;

        if (placeImage == null)
            return;

        placeImage.SetImage(sprite);
    }

    public void SetPlacePicture(Texture2D texture)
    {
        if (texture == null && defaultPicture != null)
        {
            SetPlacePicture(defaultPicture);
            return;
        }

        model.placePictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (placeImage == null)
            return;

        placeImage.SetImage(texture);
    }

    public void SetPlacePicture(string uri)
    {
        if (string.IsNullOrEmpty(uri) && defaultPicture != null)
        {
            SetPlacePicture(defaultPicture);
            return;
        }

        model.placePictureUri = uri;

        if (!Application.isPlaying)
            return;

        if (placeImage == null)
            return;

        placeImage.SetImage(uri);
    }

    public void SetPlaceName(string newText)
    {
        model.placeName = newText;

        if (placeNameOnIdleText != null)
            placeNameOnIdleText.text = newText;

        if (placeNameOnFocusText != null)
            placeNameOnFocusText.text = newText;
    }

    public void SetPlaceDescription(string newText)
    {
        model.placeDescription = newText;

        if (placeDescText == null)
            return;

        placeDescText.text = string.IsNullOrEmpty(newText) ? NO_DESCRIPTION_TEXT : newText;
    }

    public void SetPlaceAuthor(string newText)
    {
        model.placeAuthor = newText;

        if (placeAuthorOnIdleText != null)
            placeAuthorOnIdleText.text = newText;

        if(placeAuthorOnFocusText != null)
            placeAuthorOnFocusText.text = newText;
    }

    public void SetUserVisits(int userVisits)
    {
        model.userVisits = userVisits;

        if (userVisitsText != null)
            userVisitsText.text = FormatNumber(userVisits);
    }

    public void SetUserRating(float? userRating)
    {
        model.userRating = userRating;

        if (userRatingText != null)
            userRatingText.text = userRating != null ? $"{userRating.Value * 100:0}%" : "-%";
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfUsersText != null)
            numberOfUsersText.text = FormatNumber(newNumberOfUsers);

        if (numberOfUsersContainer != null)
            numberOfUsersContainer.gameObject.SetActive(newNumberOfUsers > 0);
    }

    public void SetCoords(Vector2Int newCoords)
    {
        model.coords = newCoords;

        if (coordsText == null)
            return;

        coordsText.text = $"{newCoords.x},{newCoords.y}";
    }

    public void SetParcels(Vector2Int[] parcels) { model.parcels = parcels; }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        imageContainer.SetActive(!isVisible);
        placeInfoContainer.SetActive(!isVisible);
        loadingSpinner.SetActive(isVisible);
    }

    internal void OnPlaceImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            return;

        if (!thumbnailFromMarketPlaceRequested)
        {
            thumbnailFromMarketPlaceRequested = true;
            SetPlacePicture(MapUtils.GetMarketPlaceThumbnailUrl(model.parcels, THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR));
        }
        else
        {
            SetPlacePicture(sprite: null);
        }
    }

    internal void InitializeFriendsTracker()
    {
        CleanFriendHeadsItems();

        if (mapInfoHandler == null)
            mapInfoHandler = new MapInfoHandler();

        if (friendsHandler == null)
        {
            friendsHandler = new FriendsHandler(mapInfoHandler);
            friendsHandler.OnFriendAddedEvent += OnFriendAdded;
            friendsHandler.OnFriendRemovedEvent += OnFriendRemoved;
        }
    }

    internal void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        if (currentFriendHeads.ContainsKey(profile.userId))
            return;

        BaseComponentView newFriend = InstantiateAndConfigureFriendHead(
            new FriendHeadForPlaceCardComponentModel
            {
                userProfile = profile,
                backgroundColor = backgroundColor
            },
            friendHeadPrefab);

        if (friendsGrid != null)
            friendsGrid.AddItemWithResize(newFriend);

        currentFriendHeads.Add(profile.userId, newFriend);
    }

    internal void OnFriendRemoved(UserProfile profile)
    {
        if (!currentFriendHeads.ContainsKey(profile.userId))
            return;

        if (friendsGrid != null)
            friendsGrid.RemoveItem(currentFriendHeads[profile.userId]);

        currentFriendHeads.Remove(profile.userId);
    }

    internal void CleanFriendHeadsItems()
    {
        if (friendsGrid != null)
        {
            friendsGrid.RemoveItems();
            currentFriendHeads.Clear();
        }
    }

    internal BaseComponentView InstantiateAndConfigureFriendHead(FriendHeadForPlaceCardComponentModel friendInfo, FriendHeadForPlaceCardComponentView prefabToUse)
    {
        FriendHeadForPlaceCardComponentView friendHeadGO = GameObject.Instantiate(prefabToUse);
        friendHeadGO.Configure(friendInfo);

        return friendHeadGO;
    }

    private void RebuildCardLayouts()
    {
        if (contentVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(contentVerticalLayout.transform as RectTransform);

        if (infoVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(infoVerticalLayout.transform as RectTransform);

        if (numberOfUsersContainer != null)
            Utils.ForceRebuildLayoutImmediate(numberOfUsersContainer);

        if (userVisitsAndRatingContainer != null)
            Utils.ForceRebuildLayoutImmediate(userVisitsAndRatingContainer);
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }

    private static string FormatNumber(int num)
    {
        if (num < 1000)
            return num.ToString();

        float divided = num / 1000.0f;
        divided = (int)(divided * 100) / 100f;
        return $"{divided:F2}k";
    }

    private void ResetScrollPosition()
    {
        if (scroll == null)
            return;

        scroll.verticalNormalizedPosition = 1;
    }
}
