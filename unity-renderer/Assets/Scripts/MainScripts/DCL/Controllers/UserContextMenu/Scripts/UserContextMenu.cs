using System.Collections.Generic;
using System.Linq;
using DCL.Interface;
using SocialFeaturesAnalytics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Contextual menu with different options about an user.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UserContextMenu : MonoBehaviour
{
    internal const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";
    const string DELETE_MSG_PATTERN = "Are you sure you want to delete {0} as a friend?";

    [System.Flags]
    public enum MenuConfigFlags
    {
        Name = 1,
        Friendship = 2,
        Message = 4,
        Passport = 8,
        Block = 16,
        Report = 32
    }

    const MenuConfigFlags headerFlags = MenuConfigFlags.Name | MenuConfigFlags.Friendship;
    const MenuConfigFlags usesFriendsApiFlags = MenuConfigFlags.Friendship | MenuConfigFlags.Message;

    [Header("Optional: Set Confirmation Dialog")]
    [SerializeField] internal UserContextConfirmationDialog confirmationDialog;

    [Header("Enable Actions")]
    [SerializeField] internal MenuConfigFlags menuConfigFlags = MenuConfigFlags.Passport | MenuConfigFlags.Block | MenuConfigFlags.Report;

    [Header("Containers")]
    [SerializeField] internal GameObject headerContainer;
    [SerializeField] internal GameObject bodyContainer;
    [SerializeField] internal GameObject friendshipContainer;
    [SerializeField] internal GameObject friendAddContainer;
    [SerializeField] internal GameObject friendRemoveContainer;
    [SerializeField] internal GameObject friendRequestedContainer;

    [Header("Texts")]
    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal TextMeshProUGUI blockText;

    [Header("Buttons")]
    [SerializeField] internal Button passportButton;
    [SerializeField] internal Button blockButton;
    [SerializeField] internal Button reportButton;
    [SerializeField] internal Button addFriendButton;
    [SerializeField] internal Button cancelFriendButton;
    [SerializeField] internal Button deleteFriendButton;
    [SerializeField] internal Button messageButton;

    public static event System.Action<string> OnOpenPrivateChatRequest;

    public bool isVisible => gameObject.activeSelf;
    public string UserId => userId;

    public event System.Action OnShowMenu;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnReport;
    public event System.Action<string, bool> OnBlock;
    public event System.Action<string> OnUnfriend;
    public event System.Action<string> OnAddFriend;
    public event System.Action<string> OnCancelFriend;
    public event System.Action<string> OnMessage;
    public event System.Action OnHide;

    private static StringVariable currentPlayerId = null;
    private RectTransform rectTransform;
    private string userId;
    private bool isBlocked;
    private MenuConfigFlags currentConfigFlags;
    private IConfirmationDialog currentConfirmationDialog;
    internal ISocialAnalytics socialAnalytics;

    /// <summary>
    /// Show context menu
    /// </summary>
    /// <param name="userId"> user id</param>
    public void Show(string userId) { Show(userId, menuConfigFlags); }

    /// <summary>
    /// Show context menu
    /// </summary>
    /// <param name="userId"> user id</param>
    /// <param name="configFlags">set buttons to enable in menu</param>
    public void Show(string userId, MenuConfigFlags configFlags)
    {
        this.userId = userId;
        ProcessActiveElements(configFlags);
        Setup(userId, configFlags);
        if (currentConfirmationDialog == null && confirmationDialog != null)
        {
            SetConfirmationDialog(confirmationDialog);
        }
        gameObject.SetActive(true);
        OnShowMenu?.Invoke();
    }

    /// <summary>
    /// Set confirmation popup to reference use
    /// </summary>
    /// <param name="confirmationPopup">confirmation popup reference</param>
    public void SetConfirmationDialog(IConfirmationDialog confirmationPopup) { this.currentConfirmationDialog = confirmationPopup; }

    /// <summary>
    /// Hides the context menu.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        OnHide?.Invoke();
    }

    /// <summary>
    /// Shows/Hides the friendship container
    /// </summary>
    public void SetFriendshipContentActive(bool isActive) => friendshipContainer.SetActive(isActive);

    private void Awake()
    {
        if (!currentPlayerId)
        {
            currentPlayerId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
        }

        rectTransform = GetComponent<RectTransform>();

        passportButton.onClick.AddListener(OnPassportButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
        deleteFriendButton.onClick.AddListener(OnDeleteUserButtonPressed);
        addFriendButton.onClick.AddListener(OnAddFriendButtonPressed);
        cancelFriendButton.onClick.AddListener(OnCancelFriendRequestButtonPressed);
        messageButton.onClick.AddListener(OnMessageButtonPressed);
    }

    private void Update() { HideIfClickedOutside(); }

    private void OnDisable()
    {
        if (FriendsController.i)
        {
            FriendsController.i.OnUpdateFriendship -= OnFriendActionUpdate;
        }
    }
    
    public void ClickReportButton() => reportButton.onClick.Invoke();

    private void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(userId);
        currentPlayerId.Set(userId);
        Hide();

        AudioScriptableObjects.dialogOpen.Play(true);
    }

    private void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(userId);
        WebInterface.SendReportPlayer(userId, UserProfileController.userProfilesCatalog.Get(userId)?.userName);
        GetSocialAnalytics().SendPlayerReport(PlayerReportIssueType.None, 0, PlayerActionSource.ProfileContextMenu);
        Hide();
    }

    private void OnDeleteUserButtonPressed()
    {
        OnUnfriend?.Invoke(userId);

        if (currentConfirmationDialog != null)
        {
            currentConfirmationDialog.SetText(string.Format(DELETE_MSG_PATTERN, UserProfileController.userProfilesCatalog.Get(userId)?.userName));
            currentConfirmationDialog.Show(() =>
            {
                UnfriendUser();
            });
        }
        else
        {
            UnfriendUser();
        }

        GetSocialAnalytics().SendFriendDeleted(UserProfile.GetOwnUserProfile().userId, userId, PlayerActionSource.ProfileContextMenu);
        Hide();
    }

    private void UnfriendUser()
    {
        FriendsController.i.RemoveFriend(userId);
    }

    private void OnAddFriendButtonPressed()
    {
        OnAddFriend?.Invoke(userId);

        if (!FriendsController.i)
        {
            return;
        }

        // NOTE: if we don't add this, the friend request has strange behaviors
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = userId,
            name = UserProfileController.userProfilesCatalog.Get(userId)?.userName
        });

        FriendsController.i.RequestFriendship(userId);

        GetSocialAnalytics().SendFriendRequestSent(UserProfile.GetOwnUserProfile().userId, userId, 0, PlayerActionSource.ProfileContextMenu);
    }

    private void OnCancelFriendRequestButtonPressed()
    {
        OnCancelFriend?.Invoke(userId);

        if (!FriendsController.i)
        {
            return;
        }

        FriendsController.i.CancelRequest(userId);

        GetSocialAnalytics().SendFriendRequestCancelled(UserProfile.GetOwnUserProfile().userId, userId, PlayerActionSource.ProfileContextMenu);
    }

    private void OnMessageButtonPressed()
    {
        OnMessage?.Invoke(userId);
        OnOpenPrivateChatRequest?.Invoke(userId);
        Hide();
    }

    private void OnBlockUserButtonPressed()
    {
        bool blockUser = !isBlocked;
        OnBlock?.Invoke(userId, blockUser);
        if (blockUser)
        {
            WebInterface.SendBlockPlayer(userId);
            GetSocialAnalytics().SendPlayerBlocked(FriendsController.i.IsFriend(userId), PlayerActionSource.ProfileContextMenu);
        }
        else
        {
            WebInterface.SendUnblockPlayer(userId);
            GetSocialAnalytics().SendPlayerUnblocked(FriendsController.i.IsFriend(userId), PlayerActionSource.ProfileContextMenu);
        }
        Hide();
    }

    private void UpdateBlockButton() { blockText.text = isBlocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT; }

    private void HideIfClickedOutside()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
                
        if (raycastResults.All(result => result.gameObject != gameObject))
            Hide();
    }

    private void ProcessActiveElements(MenuConfigFlags flags)
    {
        headerContainer.SetActive((flags & headerFlags) != 0);
        userName.gameObject.SetActive((flags & MenuConfigFlags.Name) != 0);
        friendshipContainer.SetActive((flags & MenuConfigFlags.Friendship) != 0);
        deleteFriendButton.gameObject.SetActive((flags & MenuConfigFlags.Friendship) != 0);
        passportButton.gameObject.SetActive((flags & MenuConfigFlags.Passport) != 0);
        blockButton.gameObject.SetActive((flags & MenuConfigFlags.Block) != 0);
        reportButton.gameObject.SetActive((flags & MenuConfigFlags.Report) != 0);
        messageButton.gameObject.SetActive((flags & MenuConfigFlags.Message) != 0);
    }

    private void Setup(string userId, MenuConfigFlags configFlags)
    {
        this.userId = userId;

        UserProfile profile = UserProfileController.userProfilesCatalog.Get(userId);
        bool userHasWallet = profile?.hasConnectedWeb3 ?? false;

        if (!userHasWallet || !UserProfile.GetOwnUserProfile().hasConnectedWeb3)
        {
            configFlags &= ~usesFriendsApiFlags;
        }

        currentConfigFlags = configFlags;
        ProcessActiveElements(configFlags);

        if ((configFlags & MenuConfigFlags.Block) != 0)
        {
            isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(userId);
            UpdateBlockButton();
        }
        if ((configFlags & MenuConfigFlags.Name) != 0)
        {
            string name = profile?.userName;
            userName.text = name;
        }
        if ((configFlags & usesFriendsApiFlags) != 0 && FriendsController.i)
        {
            if (FriendsController.i.friends.TryGetValue(userId, out UserStatus status))
            {
                SetupFriendship(status.friendshipStatus);
            }
            else
            {
                SetupFriendship(FriendshipStatus.NOT_FRIEND);
            }
            FriendsController.i.OnUpdateFriendship -= OnFriendActionUpdate;
            FriendsController.i.OnUpdateFriendship += OnFriendActionUpdate;
        }
    }

    private void SetupFriendship(FriendshipStatus friendshipStatus)
    {
        bool friendshipEnabled = (currentConfigFlags & MenuConfigFlags.Friendship) != 0;
        bool messageEnabled = (currentConfigFlags & MenuConfigFlags.Message) != 0;

        if (friendshipStatus == FriendshipStatus.FRIEND)
        {
            if (friendshipEnabled)
            {
                friendAddContainer.SetActive(false);
                friendRemoveContainer.SetActive(true);
                friendRequestedContainer.SetActive(false);
                deleteFriendButton.gameObject.SetActive(true);
            }
            if (messageEnabled)
            {
                messageButton.gameObject.SetActive(true);
            }
        }
        else if (friendshipStatus == FriendshipStatus.REQUESTED_TO)
        {
            if (friendshipEnabled)
            {
                friendAddContainer.SetActive(false);
                friendRemoveContainer.SetActive(false);
                friendRequestedContainer.SetActive(true);
                deleteFriendButton.gameObject.SetActive(false);
            }
            if (messageEnabled)
            {
                messageButton.gameObject.SetActive(false);
            }
        }
        else if (friendshipStatus == FriendshipStatus.NOT_FRIEND)
        {
            if (friendshipEnabled)
            {
                friendAddContainer.SetActive(true);
                friendRemoveContainer.SetActive(false);
                friendRequestedContainer.SetActive(false);
                deleteFriendButton.gameObject.SetActive(false);
            }
            if (messageEnabled)
            {
                messageButton.gameObject.SetActive(false);
            }
        }
        else if (friendshipStatus == FriendshipStatus.REQUESTED_FROM)
        {
            if (friendshipEnabled)
            {
                friendAddContainer.SetActive(true);
                friendRemoveContainer.SetActive(false);
                friendRequestedContainer.SetActive(false);
                deleteFriendButton.gameObject.SetActive(false);
            }
            if (messageEnabled)
            {
                messageButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnFriendActionUpdate(string userId, FriendshipAction action)
    {
        if (this.userId != userId)
        {
            return;
        }

        if (action == FriendshipAction.APPROVED)
        {
            SetupFriendship(FriendshipStatus.FRIEND);
        }
        else if (action == FriendshipAction.REQUESTED_TO)
        {
            SetupFriendship(FriendshipStatus.REQUESTED_TO);
        }
        else if (action == FriendshipAction.DELETED || action == FriendshipAction.CANCELLED || action == FriendshipAction.REJECTED)
        {
            SetupFriendship(FriendshipStatus.NOT_FRIEND);
        }
    }

    private ISocialAnalytics GetSocialAnalytics()
    {
        if (socialAnalytics == null)
        {
            socialAnalytics = new SocialAnalytics(
                DCL.Environment.i.platform.serviceProviders.analytics,
                new UserProfileWebInterfaceBridge());
        }

        return socialAnalytics;
    }

#if UNITY_EDITOR
    //This is just to process buttons and container visibility on editor
    private void OnValidate()
    {
        if (headerContainer == null)
            return;
        ProcessActiveElements(menuConfigFlags);
    }
#endif
}