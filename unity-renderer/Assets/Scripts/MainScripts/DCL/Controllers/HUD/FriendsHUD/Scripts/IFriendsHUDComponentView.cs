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
    
    RectTransform Transform { get; }
    bool ListByOnlineStatus { set; }

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
}