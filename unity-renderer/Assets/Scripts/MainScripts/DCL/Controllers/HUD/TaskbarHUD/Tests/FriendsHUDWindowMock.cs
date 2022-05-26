using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendsHUDWindowMock : MonoBehaviour, IFriendsHUDComponentView
{
    public event Action<FriendRequestEntry> OnFriendRequestApproved;
    public event Action<FriendRequestEntry> OnCancelConfirmation;
    public event Action<FriendRequestEntry> OnRejectConfirmation;
    public event Action<string> OnFriendRequestSent;
    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnClose;
    public event Action OnRequireMoreFriends;
    public event Action OnRequireMoreFriendRequests;
    public event Action<string> OnSearchFriendsRequested;

    public RectTransform Transform => (RectTransform) transform;
    public bool ListByOnlineStatus { get; set; }
    public int FriendCount { get; }
    public int FriendRequestCount { get; }

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void HideSpinner()
    {
    }

    public void ShowSpinner()
    {
    }

    public List<FriendEntryBase> GetAllEntries() => new List<FriendEntryBase>();

    public FriendEntryBase GetEntry(string userId) => null;

    public void DisplayFriendUserNotFound()
    {
    }

    public bool IsFriendListCreationReady() => false;

    public int GetReceivedFriendRequestCount() => 0;

    public void Destroy()
    {
        if (isDestroyed) return;
        Destroy(gameObject);
    }

    public void Show()
    {
    }

    public void Hide()
    {
    }

    public void Set(string userId, FriendshipAction friendshipAction, FriendEntryModel friendEntryModel)
    {
    }

    public void Set(string userId, FriendshipStatus friendshipStatus, FriendEntryModel model)
    {
    }

    public bool IsActive() => gameObject.activeSelf;

    public void ShowRequestSendError(FriendRequestError error)
    {
    }

    public void ShowRequestSendSuccess()
    {
    }

    public void ShowMoreFriendsToLoadHint(int pendingFriendsCount)
    {
    }

    public void HideMoreFriendsToLoadHint()
    {
    }

    public void ShowMoreRequestsToLoadHint(int pendingRequestsCount)
    {
    }

    public void HideMoreRequestsToLoadHint()
    {
    }

    public bool ContainsFriend(string userId) => false;

    public void FilterFriends(Dictionary<string, FriendEntryModel> friends)
    {
    }

    public void ClearFriendFilter()
    {
    }
}