using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class FriendsHUDComponentView : BaseComponentView, IFriendsHUDComponentView
{
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private FriendsTabComponentView friendsTab;
    [SerializeField] private Model model;

    public event Action<FriendRequestEntry> OnFriendRequestApproved;
    public event Action<FriendRequestEntry> OnCancelConfirmation;
    public event Action<FriendRequestEntry> OnRejectConfirmation;
    public event Action<string> OnFriendRequestSent;
    public event Action<FriendEntry> OnWhisper;
    public event Action<string> OnDeleteConfirmation;
    public event Action OnClose;

    public RectTransform Transform => transform as RectTransform;

    public override void Start()
    {
        base.Start();

        searchBar.OnSearchText += FilterFriends;
    }

    public override void Dispose()
    {
        base.Dispose();

        searchBar.OnSearchText -= FilterFriends;
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
        throw new NotImplementedException();
    }

    public FriendEntryBase GetEntry(string userId)
    {
        throw new NotImplementedException();
    }

    public void UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        friendsTab.Set(userId, model);
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

    public void Destroy() => Object.Destroy(gameObject);

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
        throw new NotImplementedException();
    }

    public void Search(string userId)
    {
        FilterFriends(userId);
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
        if (model.isLoadingSpinnerActive)
            ShowSpinner();
        else
            HideSpinner();

        if (model.visible)
            Show();
        else
            Hide();

        Search(model.searchContent);
    }

    private void FilterFriends(string search)
    {
        friendsTab.Filter(search);
    }

    [Serializable]
    private struct Model
    {
        public bool isLoadingSpinnerActive;
        public bool visible;
        public string searchContent;
    }
}