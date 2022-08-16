using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IRealmRowComponentView
{
    RealmHandler friendsHandler { get; set; }

    /// <summary>
    /// Event that will be triggered when the Warp In button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onWarpInClick { get; }

    /// <summary>
    /// Set the name label.
    /// </summary>
    /// <param name="name">A string.</param>
    void SetName(string name);

    /// <summary>
    /// Set the number of players label.
    /// </summary>
    /// <param name="numberOfPlayers">Number of players.</param>
    void SetNumberOfPlayers(int numberOfPlayers);

    /// <summary>
    /// Show/hide the connected mark.
    /// </summary>
    /// <param name="isConnected">True for showing the connected mark.</param>
    void SetAsConnected(bool isConnected);

    /// <summary>
    /// Set the background color of the row.
    /// </summary>
    /// <param name="color">Color to apply.</param>
    void SetRowColor(Color color);

    /// <summary>
    /// Set the background color of the row when it is hovered.
    /// </summary>
    /// <param name="color">Color to apply.</param>
    void SetOnHoverColor(Color color);
}

public class RealmRowComponentView : BaseComponentView, IRealmRowComponentView, IComponentModelConfig<RealmRowComponentModel>
{
    [Header("Assets References")]
    [SerializeField] internal FriendHeadForPlaceCardComponentView friendHeadPrefab;

    [Header("Prefab References")]
    [SerializeField] internal TMP_Text nameText;
    [SerializeField] internal TMP_Text playersText;
    [SerializeField] internal ButtonComponentView warpInButton;
    [SerializeField] internal GameObject connectedMark;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal GridContainerComponentView friendsGrid;

    [Header("Configuration")]
    [SerializeField] internal int maxFriendsToShow = 6;
    [SerializeField] internal RealmRowComponentModel model;

    internal Color originalBackgroundColor;
    internal Color onHoverColor;

    public RealmHandler friendsHandler { get; set; }
    internal RealmInfoHandler realmInfoHandler { get; set; }

    public Button.ButtonClickedEvent onWarpInClick => warpInButton?.onClick;

    internal Dictionary<string, BaseComponentView> currentFriendHeads = new Dictionary<string, BaseComponentView>();

    public override void Awake()
    {
        base.Awake();

        CleanFriendHeadsItems();

        originalBackgroundColor = backgroundImage.color;
        onHoverColor = backgroundImage.color;
    }

    public void Configure(RealmRowComponentModel newModel)
    {
        model = newModel;

        InitializeFriendsTracker();

        if (realmInfoHandler != null)
            realmInfoHandler.SetRealmInfo(model.name);

        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetName(model.name);
        SetNumberOfPlayers(model.players);
        SetAsConnected(model.isConnected);
        SetRowColor(model.backgroundColor);
        SetOnHoverColor(model.onHoverColor);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        backgroundImage.color = onHoverColor;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        backgroundImage.color = originalBackgroundColor;
    }

    public override void Dispose()
    {
        base.Dispose();

        if (friendsHandler != null)
        {
            friendsHandler.OnFriendAddedEvent -= OnFriendAdded;
            friendsHandler.OnFriendRemovedEvent -= OnFriendRemoved;
        }

        if (friendsGrid != null)
            friendsGrid.Dispose();
    }

    public void SetName(string name)
    {
        model.name = name;

        if (nameText == null)
            return;

        nameText.text = name.ToUpper();
    }

    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        model.players = numberOfPlayers;

        if (playersText == null)
            return;

        playersText.text = ExploreV2CommonUtils.FormatNumberToString(numberOfPlayers);
    }

    public void SetAsConnected(bool isConnected)
    {
        model.isConnected = isConnected;

        if (connectedMark == null)
            return;

        connectedMark.SetActive(isConnected);
        warpInButton.gameObject.SetActive(!isConnected);
    }

    public void SetRowColor(Color color)
    {
        model.backgroundColor = color;

        if (backgroundImage == null)
            return;

        backgroundImage.color = color;
        originalBackgroundColor = color;
    }

    public void SetOnHoverColor(Color color)
    {
        model.onHoverColor = color;
        onHoverColor = color;
    }

    internal void InitializeFriendsTracker()
    {
        CleanFriendHeadsItems();

        if (realmInfoHandler == null)
            realmInfoHandler = new RealmInfoHandler();

        if (friendsHandler == null)
        {
            friendsHandler = new RealmHandler(realmInfoHandler);
            friendsHandler.OnFriendAddedEvent += OnFriendAdded;
            friendsHandler.OnFriendRemovedEvent += OnFriendRemoved;
        }
    }

    internal void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        if (currentFriendHeads.Count == maxFriendsToShow)
            return;

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
}