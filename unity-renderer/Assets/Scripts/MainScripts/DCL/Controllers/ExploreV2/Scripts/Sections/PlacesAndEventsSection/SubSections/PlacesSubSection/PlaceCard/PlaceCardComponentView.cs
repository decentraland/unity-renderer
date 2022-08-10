using DCL;
using DCL.Helpers;
using System.Collections.Generic;
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

public class PlaceCardComponentView : BaseComponentView, IPlaceCardComponentView, IComponentModelConfig<PlaceCardComponentModel>
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
    [SerializeField] internal InputAction_Trigger closeAction;
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
    [SerializeField] internal Sprite defaultPicture;
    [SerializeField] internal PlaceCardComponentModel model;

    public FriendsHandler friendsHandler { get; set; }
    internal MapInfoHandler mapInfoHandler { get; set; }

    internal Dictionary<string, BaseComponentView> currentFriendHeads = new Dictionary<string, BaseComponentView>();

    public Button.ButtonClickedEvent onJumpInClick => jumpinButton?.onClick;
    public Button.ButtonClickedEvent onInfoClick => infoButton?.onClick;

    internal bool thumbnailFromMarketPlaceRequested = false;

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

        CleanFriendHeadsItems();
    }

    public void Configure(PlaceCardComponentModel newModel)
    {
        model = newModel;

        InitializeFriendsTracker();

        if (mapInfoHandler != null)
            mapInfoHandler.SetMinimapSceneInfo(model.hotSceneInfo);

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
            friendsGrid.AddItem(newFriend);

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

    internal void RebuildCardLayouts()
    {
        if (contentVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(contentVerticalLayout.transform as RectTransform);

        if (infoVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(infoVerticalLayout.transform as RectTransform);
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }
}