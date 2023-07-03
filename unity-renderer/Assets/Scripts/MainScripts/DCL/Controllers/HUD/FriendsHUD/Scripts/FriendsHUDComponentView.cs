using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDComponentView : BaseComponentView, IFriendsHUDComponentView
{
    private const int FRIENDS_LIST_TAB_INDEX = 0;
    private const int FRIENDS_REQUEST_TAB_INDEX = 1;

    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button friendsTabFocusButton;
    [SerializeField] internal Button friendRequestsTabFocusButton;
    [SerializeField] internal FriendsTabComponentView friendsTab;
    [SerializeField] internal FriendRequestsTabComponentView friendRequestsTab;
    [SerializeField] private Model model;

    public event Action<FriendRequestEntryModel> OnFriendRequestApproved
    {
        add => friendRequestsTab.OnFriendRequestApproved += value;
        remove => friendRequestsTab.OnFriendRequestApproved -= value;
    }

    public event Action<FriendRequestEntryModel> OnCancelConfirmation
    {
        add => friendRequestsTab.OnCancelConfirmation += value;
        remove => friendRequestsTab.OnCancelConfirmation -= value;
    }

    public event Action<FriendRequestEntryModel> OnRejectConfirmation
    {
        add => friendRequestsTab.OnRejectConfirmation += value;
        remove => friendRequestsTab.OnRejectConfirmation -= value;
    }

    public event Action<string> OnFriendRequestSent
    {
        add => friendRequestsTab.OnFriendRequestSent += value;
        remove => friendRequestsTab.OnFriendRequestSent -= value;
    }

    public event Action<string> OnFriendRequestOpened
    {
        add => friendRequestsTab.OnFriendRequestOpened += value;
        remove => friendRequestsTab.OnFriendRequestOpened -= value;
    }

    public event Action<FriendEntryModel> OnWhisper
    {
        add => friendsTab.OnWhisper += value;
        remove => friendsTab.OnWhisper -= value;
    }

    public event Action<string> OnDeleteConfirmation
    {
        add => friendsTab.OnDeleteConfirmation += value;
        remove => friendsTab.OnDeleteConfirmation -= value;
    }

    public event Action OnRequireMoreFriends
    {
        add => friendsTab.OnRequireMoreEntries += value;
        remove => friendsTab.OnRequireMoreEntries -= value;
    }

    public event Action OnRequireMoreFriendRequests
    {
        add => friendRequestsTab.OnRequireMoreEntries += value;
        remove => friendRequestsTab.OnRequireMoreEntries -= value;
    }

    public event Action<string> OnSearchFriendsRequested
    {
        add => friendsTab.OnSearchRequested += value;
        remove => friendsTab.OnSearchRequested -= value;
    }

    public event Action OnFriendListDisplayed;
    public event Action OnRequestListDisplayed;

    public event Action OnClose;

    public RectTransform Transform => transform as RectTransform;

    public int FriendCount => friendsTab.Count;
    public int FriendRequestCount => friendRequestsTab.Count;
    public int FriendRequestSentCount => friendRequestsTab.SentCount;
    public int FriendRequestReceivedCount => friendRequestsTab.ReceivedCount;
    public bool IsFriendListActive => friendsTab.gameObject.activeInHierarchy;
    public bool IsRequestListActive => friendRequestsTab.gameObject.activeInHierarchy;

    public void Initialize(IChatController chatController,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics)
    {
        friendsTab.Initialize(chatController, friendsController, socialAnalytics);
    }

    public void RefreshFriendsTab()
    {
        friendsTab.RefreshControl();
    }

    public override void Awake()
    {
        base.Awake();

        friendsTabFocusButton.onClick.AddListener(() => FocusTab(FRIENDS_LIST_TAB_INDEX));
        friendRequestsTabFocusButton.onClick.AddListener(() => FocusTab(FRIENDS_REQUEST_TAB_INDEX));
        closeButton.onClick.AddListener(() =>
        {
            OnClose?.Invoke();
            Hide();
        });

        friendRequestsTab.Expand();
    }

    public void HideLoadingSpinner()
    {
        loadingSpinner.SetActive(false);
        model.isLoadingSpinnerActive = false;
    }

    public void ShowLoadingSpinner()
    {
        loadingSpinner.SetActive(true);
        model.isLoadingSpinnerActive = true;
    }

    public FriendEntryBase GetEntry(string userId)
    {
        return (FriendEntryBase) friendsTab.Get(userId) ?? friendRequestsTab.Get(userId);
    }

    public void DisplayFriendUserNotFound() => friendRequestsTab.ShowUserNotFoundNotification();

    public void Show()
    {
        model.visible = true;
        gameObject.SetActive(true);
        AudioScriptableObjects.dialogOpen.Play(true);
    }

    public void Hide()
    {
        model.visible = false;
        gameObject.SetActive(false);
        AudioScriptableObjects.dialogClose.Play(true);
    }

    public void Set(string userId, FriendEntryModel model)
    {
        friendRequestsTab.Remove(userId);
        friendsTab.Enqueue(userId, model);
    }

    public void Remove(string userId)
    {
        friendRequestsTab.Remove(userId);
        friendsTab.Remove(userId);
    }

    public void Set(string userId, FriendRequestEntryModel model)
    {
        friendRequestsTab.Enqueue(userId, model);
        friendsTab.Remove(userId);
    }

    public bool IsActive() => gameObject.activeInHierarchy;

    public void ShowRequestSendError(FriendRequestError error)
    {
        switch (error)
        {
            case FriendRequestError.AlreadyFriends:
                friendRequestsTab.ShowAlreadyFriendsNotification();
                break;
            case FriendRequestError.UserNotFound:
                friendRequestsTab.ShowUserNotFoundNotification();
                break;
        }
    }

    public void ShowRequestSendSuccess()
    {
        friendRequestsTab.ShowRequestSuccessfullySentNotification();
    }

    public void ShowMoreFriendsToLoadHint(int hiddenCount) => friendsTab.ShowMoreFriendsToLoadHint(hiddenCount);

    public void HideMoreFriendsToLoadHint() => friendsTab.HideMoreFriendsToLoadHint();

    public void ShowMoreRequestsToLoadHint(int hiddenCount) =>
        friendRequestsTab.ShowMoreEntriesToLoadHint(hiddenCount);

    public void HideMoreRequestsToLoadHint() => friendRequestsTab.HideMoreFriendsToLoadHint();

    public bool ContainsFriend(string userId) => friendsTab.Get(userId) != null;

    public bool ContainsFriendRequest(string userId) => friendRequestsTab.Get(userId) != null;

    public void EnableSearchMode() => friendsTab.EnableSearchMode();

    public void DisableSearchMode() => friendsTab.DisableSearchMode();

    public void UpdateBlockStatus(string userId, bool blocked)
    {
        UpdateBlockStatus(blocked, friendsTab.Get(userId));
        UpdateBlockStatus(blocked, friendRequestsTab.Get(userId));
    }

    public void ClearAll()
    {
        friendsTab.Clear();
        friendRequestsTab.Clear();
    }

    public override void RefreshControl()
    {
        if (model.isLoadingSpinnerActive)
            ShowLoadingSpinner();
        else
            HideLoadingSpinner();

        if (model.visible)
            Show();
        else
            Hide();

        FocusTab(model.focusedTabIndex);
    }

    internal void FocusTab(int index)
    {
        model.focusedTabIndex = index;

        if (index == FRIENDS_LIST_TAB_INDEX)
        {
            friendsTab.Show();
            friendRequestsTab.Hide();
            OnFriendListDisplayed?.Invoke();
        }
        else if (index == FRIENDS_REQUEST_TAB_INDEX)
        {
            friendsTab.Hide();
            friendRequestsTab.Show();
            OnRequestListDisplayed?.Invoke();
        }
        else
            throw new IndexOutOfRangeException();
    }

    private void UpdateBlockStatus(bool blocked, FriendEntryBase friendEntry)
    {
        if (friendEntry == null) return;
        var friendModel = friendEntry.Model;
        friendModel.blocked = blocked;
        friendEntry.RefreshControl();
    }

    [Serializable]
    private struct Model
    {
        public int focusedTabIndex;
        public bool isLoadingSpinnerActive;
        public bool visible;
    }
}
