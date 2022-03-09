using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDComponentView : BaseComponentView, IFriendsHUDComponentView
{
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
    public event Action OnClose;

    public RectTransform Transform => transform as RectTransform;
    
    public static FriendsHUDComponentView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>("SocialBarV1/FriendsHUD")).GetComponent<FriendsHUDComponentView>();
        return view;
    }

    public override void Awake()
    {
        base.Awake();
        
        friendsTabFocusButton.onClick.AddListener(() => FocusTab(0));
        friendRequestsTabFocusButton.onClick.AddListener(() => FocusTab(1));
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

    public void UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        friendsTab.Populate(userId, model);
        friendRequestsTab.Populate(userId, model);
    }

    public void DisplayFriendUserNotFound() => friendRequestsTab.ShowUserNotFoundNotification();

    public bool IsFriendListFocused() => friendsTab.gameObject.activeInHierarchy;

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

    public void UpdateFriendshipStatus(string userId, FriendshipAction friendshipAction,
        FriendEntryBase.Model friendEntryModel)
    {
        switch (friendshipAction)
        {
            case FriendshipAction.NONE:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
            case FriendshipAction.APPROVED:
                friendRequestsTab.Remove(userId);
                friendsTab.Enqueue(userId, friendEntryModel);
                break;
            case FriendshipAction.REJECTED:
                friendRequestsTab.Remove(userId);
                break;
            case FriendshipAction.CANCELLED:
                friendRequestsTab.Remove(userId);
                break;
            case FriendshipAction.REQUESTED_FROM:
                friendRequestsTab.Set(userId, friendEntryModel, true);
                break;
            case FriendshipAction.REQUESTED_TO:
                friendRequestsTab.Set(userId,  friendEntryModel, false);
                break;
            case FriendshipAction.DELETED:
                friendRequestsTab.Remove(userId);
                friendsTab.Remove(userId);
                break;
        }
    }

    public void Search(string userId)
    {
        FilterFriends(userId);
    }

    public bool IsActive() => gameObject.activeInHierarchy;
    
    public void SortEntriesByTimestamp(FriendEntryBase.Model user, ulong timestamp)
    {
        friendsTab.SortEntriesByTimestamp(user, timestamp);
    }

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

        Search(model.searchContent);
        FocusTab(model.focusedTabIndex);
    }

    private void FocusTab(int index)
    {
        model.focusedTabIndex = index;

        if (index == 0)
        {
            friendsTab.Show();
            friendRequestsTab.Hide();
        }
        else if (index == 1)
        {
            friendsTab.Hide();
            friendRequestsTab.Show();
        }
        else
            throw new IndexOutOfRangeException();
    }

    private void FilterFriends(string search)
    {
        friendsTab.Filter(search);
    }

    [Serializable]
    private struct Model
    {
        public int focusedTabIndex;
        public bool isLoadingSpinnerActive;
        public bool visible;
        public string searchContent;
    }
}