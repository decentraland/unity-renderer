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

    void HideSpinner();
    void ShowSpinner();
    List<FriendEntryBase> GetAllEntries();
    FriendEntryBase GetEntry(string userId);
    void UpdateEntry(string userId, FriendEntryBase.Model model);
    void DisplayFriendUserNotFound();
    bool IsFriendListCreationReady();
    int GetReceivedFriendRequestCount();
    void Destroy();
    void Show();
    void Hide();
    void UpdateFriendshipStatus(string userId, FriendshipAction friendshipAction, FriendEntryBase.Model friendEntryModel);
    void Search(string userId);
    bool IsActive();
    void ShowRequestSendError(FriendRequestError error);
    void ShowRequestSendSuccess();
}