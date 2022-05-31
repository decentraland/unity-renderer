using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFriendsHUDComponentView
{
    event Action<FriendRequestEntryModel> OnFriendRequestApproved;
    event Action<FriendRequestEntryModel> OnCancelConfirmation;
    event Action<FriendRequestEntryModel> OnRejectConfirmation;
    event Action<string> OnFriendRequestSent;
    event Action<FriendEntryModel> OnWhisper;
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
    void Destroy();
    void Show();
    void Hide();
    void Set(string userId, FriendshipAction friendshipAction, FriendEntryModel model);
    void Set(string userId, FriendshipStatus friendshipStatus, FriendEntryModel model);
    void Populate(string userId, FriendEntryModel model);
    bool IsActive();
    void ShowRequestSendError(FriendRequestError error);
    void ShowRequestSendSuccess();
    void ShowMoreFriendsToLoadHint(int pendingFriendsCount);
    void HideMoreFriendsToLoadHint();
    void ShowMoreRequestsToLoadHint(int pendingRequestsCount);
    void HideMoreRequestsToLoadHint();
    bool ContainsFriend(string userId);
    bool ContainsFriendRequest(string userId);
    void FilterFriends(Dictionary<string, FriendEntryModel> friends);
    void ClearFriendFilter();
}