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

    public event Action OnClose;

    public RectTransform Transform => transform as RectTransform;

    public int FriendCount => friendsTab.Count;
    public int FriendRequestCount => friendRequestsTab.Count;

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

    public void Refresh()
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

        //TODO ANTON clarify before removing completely
        //friendsTab.Expand();
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
        }
    }

    public void ShowRequestSendSuccess()
    {
        friendRequestsTab.ShowRequestSuccessfullySentNotification();
    }

    public void ShowMoreFriendsToLoadHint(int pendingFriendsCount) => friendsTab.ShowMoreFriendsToLoadHint(pendingFriendsCount);

    public void HideMoreFriendsToLoadHint() => friendsTab.HideMoreFriendsToLoadHint();

    public void ShowMoreRequestsToLoadHint(int pendingRequestsCount) =>
        friendRequestsTab.ShowMoreFriendsToLoadHint(pendingRequestsCount);

    public void HideMoreRequestsToLoadHint() => friendRequestsTab.HideMoreFriendsToLoadHint();

    public bool ContainsFriend(string userId) => friendsTab.Get(userId) != null;

    public bool ContainsFriendRequest(string userId) => friendRequestsTab.Get(userId) != null;

    public void FilterFriends(Dictionary<string, FriendEntryModel> friends) => friendsTab.Filter(friends);

    public void ClearFriendFilter() => friendsTab.ClearFilter();
    
    public void UpdateBlockStatus(string userId, bool blocked)
    {
        UpdateBlockStatus(blocked, friendsTab.Get(userId));
        UpdateBlockStatus(blocked, friendRequestsTab.Get(userId));
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
        }
        else if (index == FRIENDS_REQUEST_TAB_INDEX)
        {
            friendsTab.Hide();
            friendRequestsTab.Show();
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