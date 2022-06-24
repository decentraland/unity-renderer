using System;
using System.Collections.Generic;
using SocialFeaturesAnalytics;
using UnityEngine;

public class FriendsHUDWindowMock : MonoBehaviour, IFriendsHUDComponentView
{
    public event Action<FriendRequestEntryModel> OnFriendRequestApproved;
    public event Action<FriendRequestEntryModel> OnCancelConfirmation;
    public event Action<FriendRequestEntryModel> OnRejectConfirmation;
    public event Action<string> OnFriendRequestSent;
    public event Action<FriendEntryModel> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnClose;
    public event Action OnRequireMoreFriends;
    public event Action OnRequireMoreFriendRequests;
    public event Action<string> OnSearchFriendsRequested;
    public event Action OnFriendListDisplayed;
    public event Action OnRequestListDisplayed;

    public void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService,
        IFriendsController friendsController, ISocialAnalytics socialAnalytics)
    {
    }

    public RectTransform Transform => (RectTransform) transform;
    public bool ListByOnlineStatus { get; set; }
    public int FriendCount { get; }
    public int FriendRequestCount { get; }
    public bool IsFriendListActive { get; }
    public bool IsRequestListActive { get; }

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void HideLoadingSpinner()
    {
    }

    public void ShowLoadingSpinner()
    {
    }

    public List<FriendEntryBase> GetAllEntries() => new List<FriendEntryBase>();

    public FriendEntryBase GetEntry(string userId) => null;

    public void DisplayFriendUserNotFound()
    {
    }

    public bool IsFriendListCreationReady() => false;

    public void Dispose()
    {
        if (isDestroyed) return;
        Destroy(gameObject);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void Set(string userId, FriendshipAction friendshipAction, FriendEntryModel friendEntryModel)
    {
    }

    public void Set(string userId, FriendshipStatus friendshipStatus, FriendEntryModel model)
    {
    }

    public void Populate(string userId, FriendEntryModel model)
    {
    }

    public bool IsActive() => gameObject.activeSelf;

    public void ShowRequestSendError(FriendRequestError error)
    {
    }

    public void ShowRequestSendSuccess()
    {
    }

    public void ShowMoreFriendsToLoadHint(int hiddenCount)
    {
    }

    public void HideMoreFriendsToLoadHint()
    {
    }

    public void ShowMoreRequestsToLoadHint(int hiddenCount)
    {
    }

    public void HideMoreRequestsToLoadHint()
    {
    }

    public bool ContainsFriend(string userId) => false;

    public bool ContainsFriendRequest(string userId) => false;

    public void FilterFriends(Dictionary<string, FriendEntryModel> friends)
    {
    }

    public void ClearFriendFilter()
    {
    }
}