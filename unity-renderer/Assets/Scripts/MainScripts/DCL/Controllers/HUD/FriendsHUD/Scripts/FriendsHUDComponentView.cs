using System;
using System.Collections.Generic;
using SocialFeaturesAnalytics;
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
        add => friendsTab.OnRequireMoreFriends += value;
        remove => friendsTab.OnRequireMoreFriends -= value;
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

    public bool ListByOnlineStatus
    {
        set => friendsTab.ListByOnlineStatus = value;
    }

    public int FriendCount => friendsTab.Count;
    public int FriendRequestCount => friendRequestsTab.Count;
    public bool IsFriendListActive => friendsTab.gameObject.activeInHierarchy;
    public bool IsRequestListActive => friendRequestsTab.gameObject.activeInHierarchy;

    public static FriendsHUDComponentView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>("SocialBarV1/FriendsHUD"))
            .GetComponent<FriendsHUDComponentView>();
        return view;
    }
    
    public void Initialize(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics)
    {
        friendsTab.Initialize(chatController, lastReadMessagesService, friendsController, socialAnalytics);
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

        friendsTab.Expand();
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

    public List<FriendEntryBase> GetAllEntries()
    {
        var result = new List<FriendEntryBase>();
        result.AddRange(friendsTab.Entries.Values);
        result.AddRange(friendRequestsTab.Entries.Values);
        return result;
    }

    public FriendEntryBase GetEntry(string userId)
    {
        return (FriendEntryBase) friendsTab.Get(userId) ?? friendRequestsTab.Get(userId);
    }

    public void DisplayFriendUserNotFound() => friendRequestsTab.ShowUserNotFoundNotification();

    public bool IsFriendListCreationReady() => friendsTab.DidDeferredCreationCompleted;

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

    public void Set(string userId,
        FriendshipAction friendshipAction,
        FriendEntryModel model)
    {
        switch (friendshipAction)
        {
            case FriendshipAction.NONE:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.APPROVED:
                friendRequestsTab.Remove(userId);
                friendsTab.Enqueue(userId, model);
                break;
            case FriendshipAction.REJECTED:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.CANCELLED:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.REQUESTED_FROM:
                friendRequestsTab.Enqueue(userId, (FriendRequestEntryModel) model);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.REQUESTED_TO:
                friendRequestsTab.Enqueue(userId, (FriendRequestEntryModel) model);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.DELETED:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
            default:
                Debug.LogError($"FriendshipAction not supported: {friendshipAction}");
                break;
        }
    }

    public void Set(string userId, FriendshipStatus friendshipStatus, FriendEntryModel model)
    {
        switch (friendshipStatus)
        {
            case FriendshipStatus.FRIEND:
                friendsTab.Enqueue(userId, model);
                friendRequestsTab.Remove(userId);
                break;
            case FriendshipStatus.NOT_FRIEND:
                friendsTab.Remove(userId);
                friendRequestsTab.Remove(userId);
                break;
            case FriendshipStatus.REQUESTED_TO:
                friendsTab.Remove(userId);
                friendRequestsTab.Enqueue(userId, (FriendRequestEntryModel) model);
                break;
            case FriendshipStatus.REQUESTED_FROM:
                friendsTab.Remove(userId);
                friendRequestsTab.Enqueue(userId, (FriendRequestEntryModel) model);
                break;
            default:
                Debug.LogError($"FriendshipStatus not supported: {friendshipStatus}");
                break;
        }
    }

    public void Populate(string userId, FriendEntryModel model)
    {
        friendsTab.Populate(userId, model);
        friendRequestsTab.Populate(userId, (FriendRequestEntryModel) model);
    }

    public bool IsActive() => gameObject.activeInHierarchy;

    public void ShowRequestSendError(FriendRequestError error)
    {
        switch (error)
        {
            case FriendRequestError.AlreadyFriends:
                friendRequestsTab.ShowAlreadyFriendsNotification();
                break;
        }
    }

    public void ShowRequestSendSuccess()
    {
        friendRequestsTab.ShowRequestSuccessfullySentNotification();
    }

    public void ShowMoreFriendsToLoadHint() => friendsTab.ShowMoreFriendsToLoadHint();

    public void HideMoreFriendsToLoadHint() => friendsTab.HideMoreFriendsToLoadHint();

    public void ShowMoreRequestsToLoadHint() =>
        friendRequestsTab.ShowMoreFriendsToLoadHint();

    public void HideMoreRequestsToLoadHint() => friendRequestsTab.HideMoreFriendsToLoadHint();

    public bool ContainsFriend(string userId) => friendsTab.Get(userId) != null;

    public bool ContainsFriendRequest(string userId) => friendRequestsTab.Get(userId) != null;

    public void FilterFriends(Dictionary<string, FriendEntryModel> friends) => friendsTab.Filter(friends);

    public void ClearFriendFilter() => friendsTab.ClearFilter();

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

    [Serializable]
    private struct Model
    {
        public int focusedTabIndex;
        public bool isLoadingSpinnerActive;
        public bool visible;
    }
}