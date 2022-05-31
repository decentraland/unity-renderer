using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFriendsHUDComponentView
{
    event Action<FriendRequestEntry> OnFriendRequestApproved;
    event Action<FriendRequestEntry> OnCancelConfirmation;
    event Action<FriendRequestEntry> OnRejectConfirmation;
    event Action<string> OnFriendRequestSent;
    event Action<FriendEntry> OnWhisper;
    event Action<string> OnDeleteConfirmation;
    event Action OnClose;
    event Action OnRequireMoreFriends;
    event Action OnRequireMoreFriendRequests;
    event Action<string> OnSearchFriendsRequested;
    
    RectTransform Transform { get; }
    bool ListByOnlineStatus { set; }
    int FriendCount { get; }
    int FriendRequestCount { get; }

    void HideSpinner();
    void ShowSpinner();
    List<FriendEntryBase> GetAllEntries();
    FriendEntryBase GetEntry(string userId);
    void DisplayFriendUserNotFound();
    bool IsFriendListCreationReady();
    int GetReceivedFriendRequestCount();
    void Destroy();
    void Show();
    void Hide();
    void Set(string userId, FriendshipAction friendshipAction, FriendEntryModel model);
    void Set(string userId, FriendshipStatus friendshipStatus, FriendEntryModel model);
    bool IsActive();
    void ShowRequestSendError(FriendRequestError error);
    void ShowRequestSendSuccess();
    void ShowMoreFriendsToLoadHint(int pendingFriendsCount);
    void HideMoreFriendsToLoadHint();
    void ShowMoreRequestsToLoadHint(int pendingRequestsCount);
    void HideMoreRequestsToLoadHint();
    bool ContainsFriend(string userId);
    void FilterFriends(Dictionary<string, FriendEntryModel> friends);
    void ClearFriendFilter();
}