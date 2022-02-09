using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendsHUDUIComponent : BaseComponentView, IFriendsHUDView
{
    public event Action<FriendRequestEntry> OnFriendRequestApproved;
    public event Action<FriendRequestEntry> OnCancelConfirmation;
    public event Action<FriendRequestEntry> OnRejectConfirmation;
    public event Action<string> OnFriendRequestSent;
    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnClose;

    public RectTransform Transform { get; }
    
    public void HideSpinner()
    {
        throw new NotImplementedException();
    }

    public void ShowSpinner()
    {
        throw new NotImplementedException();
    }

    public List<FriendEntryBase> GetAllEntries()
    {
        throw new NotImplementedException();
    }

    public FriendEntryBase GetEntry(string userId)
    {
        throw new NotImplementedException();
    }

    public void UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        throw new NotImplementedException();
    }

    public void DisplayFriendUserNotFound()
    {
        throw new NotImplementedException();
    }

    public bool IsFriendListFocused()
    {
        throw new NotImplementedException();
    }

    public bool IsFriendListCreationReady()
    {
        throw new NotImplementedException();
    }

    public int GetReceivedFriendRequestCount()
    {
        throw new NotImplementedException();
    }

    public void Destroy()
    {
        throw new NotImplementedException();
    }

    public void Show()
    {
        throw new NotImplementedException();
    }

    public void Hide()
    {
        throw new NotImplementedException();
    }

    public void UpdateFriendshipStatus(string userId, FriendshipAction friendshipAction, FriendEntryBase.Model friendEntryModel)
    {
        throw new NotImplementedException();
    }

    public void Search(string userId)
    {
        throw new NotImplementedException();
    }

    public bool IsActive()
    {
        throw new NotImplementedException();
    }

    public void ShowCurrentFriendPassport()
    {
        throw new NotImplementedException();
    }

    public void ReportCurrentFriend()
    {
        throw new NotImplementedException();
    }
    
    public override void RefreshControl()
    {
        throw new NotImplementedException();
    }
}