using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPlaceCardComponentView
{
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
}

public class PlaceCardComponentView : BaseComponentView, IPlaceCardComponentView, IComponentModelConfig
{
    internal const int THMBL_MARKETPLACE_WIDTH = 196;
    internal const int THMBL_MARKETPLACE_HEIGHT = 143;
    internal const int THMBL_MARKETPLACE_SIZEFACTOR = 50;

    internal static readonly int ON_FOCUS_CARD_COMPONENT_BOOL = Animator.StringToHash("OnFocus");

    [Header("Assets References")]
    [SerializeField] internal FriendHeadForPlaceCardComponentView friendHeadPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView placeImage;
    [SerializeField] internal TMP_Text placeNameOnIdleText;
    [SerializeField] internal TMP_Text placeNameOnFocusText;
    [SerializeField] internal TMP_Text placeDescText;
    [SerializeField] internal TMP_Text placeAuthorText;
    [SerializeField] internal TMP_Text numberOfUsersText;
    [SerializeField] internal TMP_Text coordsText;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal ButtonComponentView infoButton;
    [SerializeField] internal ButtonComponentView jumpinButton;
    [SerializeField] internal GridContainerComponentView friendsGrid;
    [SerializeField] internal GameObject imageContainer;
    [SerializeField] internal GameObject placeInfoContainer;
    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal Animator cardAnimator;
    [SerializeField] internal GameObject cardSelectionFrame;
    [SerializeField] internal VerticalLayoutGroup contentVerticalLayout;
    [SerializeField] internal VerticalLayoutGroup infoVerticalLayout;

    [Header("Configuration")]
    [SerializeField] internal PlaceCardComponentModel model;

    public FriendsHandler friendsHandler { get; set; }
    internal MapInfoHandler mapInfoHandler { get; set; }

    internal Dictionary<string, BaseComponentView> currentFriendHeads = new Dictionary<string, BaseComponentView>();

    public Button.ButtonClickedEvent onJumpInClick => jumpinButton?.onClick;
    public Button.ButtonClickedEvent onInfoClick => infoButton?.onClick;

    public override void PostInitialization()
    {
        if (placeImage != null)
            placeImage.OnLoaded += OnPlaceImageLoaded;

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);

        CleanFriendHeadsItems();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (PlaceCardComponentModel)newModel;

        InitializeFriendsTracker();

        if (mapInfoHandler != null)
            mapInfoHandler.SetMinimapSceneInfo(model.hotSceneInfo);

        RefreshControl();
    }

    public override void RefreshControl()
    {
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
        SetNumberOfUsers(model.numberOfUsers);
        SetCoords(model.coords);

        RebuildCardLayouts();
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(true);

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, false);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (placeImage != null)
            placeImage.OnLoaded -= OnPlaceImageLoaded;

        if (closeCardButton != null)
            closeCardButton.onClick.RemoveAllListeners();

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.RemoveAllListeners();

        if (friendsHandler != null)
        {
            friendsHandler.OnFriendAddedEvent -= OnFriendAdded;
            friendsHandler.OnFriendRemovedEvent -= OnFriendRemoved;
        }
    }

    public void SetPlacePicture(Sprite sprite)
    {
        model.placePictureSprite = sprite;

        if (placeImage == null)
            return;

        placeImage.SetImage(sprite);
    }

    public void SetPlacePicture(Texture2D texture)
    {
        model.placePictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (placeImage == null)
            return;

        placeImage.SetImage(texture);
    }

    public void SetPlacePicture(string uri)
    {
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

        placeDescText.text = newText;
    }

    public void SetPlaceAuthor(string newText)
    {
        model.placeAuthor = newText;

        if (placeAuthorText == null)
            return;

        placeAuthorText.text = newText;
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfUsersText == null)
            return;

        numberOfUsersText.text = newNumberOfUsers.ToString();
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

    private void OnPlaceImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            SetPlacePicture(sprite);
        else
            SetPlacePicture(MapUtils.GetMarketPlaceThumbnailUrl(model.parcels, THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR));
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

        currentFriendHeads.Add(profile.userId, newFriend);

        if (friendsGrid != null)
            friendsGrid.SetItems(currentFriendHeads.Select(x => x.Value).ToList(), false);
    }

    internal void OnFriendRemoved(UserProfile profile)
    {
        if (!currentFriendHeads.ContainsKey(profile.userId))
            return;

        currentFriendHeads.Remove(profile.userId);

        if (friendsGrid != null)
            friendsGrid.SetItems(currentFriendHeads.Select(x => x.Value).ToList(), false);
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

    internal void RebuildCardLayouts()
    {
        if (contentVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(contentVerticalLayout.transform as RectTransform);

        if (infoVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(infoVerticalLayout.transform as RectTransform);
    }

    internal void CloseModal() { gameObject.SetActive(false); }
}