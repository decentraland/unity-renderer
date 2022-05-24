using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDComponentView : BaseComponentView, IFriendsHUDComponentView
{
    private const int FRIENDS_LIST_TAB_INDEX = 0;
    private const int FRIENDS_REQUEST_TAB_INDEX = 1;

    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button friendsTabFocusButton;
    [SerializeField] private Button friendRequestsTabFocusButton;
    [SerializeField] private FriendsTabComponentView friendsTab;
    [SerializeField] private FriendRequestsTabComponentView friendRequestsTab;
    [SerializeField] private Model model;

    public event Action<FriendRequestEntry> OnFriendRequestApproved
    {
        add => friendRequestsTab.OnFriendRequestApproved += value;
        remove => friendRequestsTab.OnFriendRequestApproved -= value;
    }

    public event Action<FriendRequestEntry> OnCancelConfirmation
    {
        add => friendRequestsTab.OnCancelConfirmation += value;
        remove => friendRequestsTab.OnCancelConfirmation -= value;
    }

    public event Action<FriendRequestEntry> OnRejectConfirmation
    {
        add => friendRequestsTab.OnRejectConfirmation += value;
        remove => friendRequestsTab.OnRejectConfirmation -= value;
    }

    public event Action<string> OnFriendRequestSent
    {
        add => friendRequestsTab.OnFriendRequestSent += value;
        remove => friendRequestsTab.OnFriendRequestSent -= value;
    }

    public event Action<FriendEntry> OnWhisper
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

    public event Action<string> OnSearchFriendsRequested
    {
        add => friendsTab.OnSearchRequested += value;
        remove => friendsTab.OnSearchRequested -= value;
    }

    public event Action OnClose;

    public RectTransform Transform => transform as RectTransform;

    public bool ListByOnlineStatus
    {
        set => friendsTab.ListByOnlineStatus = value;
    }

    public int FriendCount => friendsTab.Count;
    public int FriendRequestCount => friendRequestsTab.Count;

    public static FriendsHUDComponentView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>("SocialBarV1/FriendsHUD"))
            .GetComponent<FriendsHUDComponentView>();
        return view;
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

    public void HideSpinner()
    {
        loadingSpinner.SetActive(false);
        model.isLoadingSpinnerActive = false;
    }

    public void ShowSpinner()
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

    public int GetReceivedFriendRequestCount() => friendRequestsTab.ReceivedRequestsList.Count();

    public void Destroy() => Destroy(gameObject);

    public void Show()
    {
        model.visible = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        model.visible = false;
        gameObject.SetActive(false);
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
                friendRequestsTab.Enqueue(userId, new FriendRequestEntryModel(model, true));
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.REQUESTED_TO:
                friendRequestsTab.Enqueue(userId, new FriendRequestEntryModel(model, false));
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
        Debug.Log($"Set {userId}, {friendshipStatus}, {model.userName}");
        
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
                friendRequestsTab.Enqueue(userId, new FriendRequestEntryModel(model, false));
                break;
            case FriendshipStatus.REQUESTED_FROM:
                friendsTab.Remove(userId);
                friendRequestsTab.Enqueue(userId, new FriendRequestEntryModel(model, true));
                break;
            default:
                Debug.LogError($"FriendshipStatus not supported: {friendshipStatus}");
                break;
        }
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

    public void ShowMoreFriendsToLoadHint(int pendingFriendsCount) => friendsTab.ShowMoreFriendsToLoadHint(pendingFriendsCount);

    public void HideMoreFriendsToLoadHint() => friendsTab.HideMoreFriendsToLoadHint();

    public void ShowMoreRequestsToLoadHint(int pendingRequestsCount)
    {
        throw new NotImplementedException();
    }

    public bool ContainsFriend(string userId) => friendsTab.Get(userId) != null;

    public void FilterFriends(Dictionary<string, FriendEntryModel> friends) => friendsTab.Filter(friends);

    public void ClearFriendFilter() => friendsTab.ClearFilter();

    public override void RefreshControl()
    {
        if (model.isLoadingSpinnerActive)
            ShowSpinner();
        else
            HideSpinner();

        if (model.visible)
            Show();
        else
            Hide();

        FocusTab(model.focusedTabIndex);
    }

    private void FocusTab(int index)
    {
        model.focusedTabIndex = index;

        if (index == FRIENDS_LIST_TAB_INDEX)
        {
            friendsTab.Show();
            friendRequestsTab.Hide();
        }
        else if (index == FRIENDS_REQUEST_TAB_INDEX)
        {
            friendsTab.Hide();
            friendRequestsTab.Show();
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