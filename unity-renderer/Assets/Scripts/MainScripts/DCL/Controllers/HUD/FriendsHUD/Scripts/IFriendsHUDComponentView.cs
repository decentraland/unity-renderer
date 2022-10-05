using System;
using SocialFeaturesAnalytics;
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
    event Action OnFriendListDisplayed;
    event Action OnRequestListDisplayed;

    void Initialize(IChatController chatController,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics);

    void RefreshFriendsTab();
    RectTransform Transform { get; }
    int FriendCount { get; }
    int FriendRequestCount { get; }
    int FriendRequestSentCount { get; }
    int FriendRequestReceivedCount { get; }
    bool IsFriendListActive { get; }
    bool IsRequestListActive { get; }

    void HideLoadingSpinner();
    void ShowLoadingSpinner();
    void DisplayFriendUserNotFound();
    void Dispose();
    void Show();
    void Hide();
    void Set(string userId, FriendEntryModel model);
    void Set(string userId, FriendRequestEntryModel model);
    void Remove(string userId);
    bool IsActive();
    void ShowRequestSendError(FriendRequestError error);
    void ShowRequestSendSuccess();
    void ShowMoreFriendsToLoadHint(int hiddenCount);
    void HideMoreFriendsToLoadHint();
    void ShowMoreRequestsToLoadHint(int hiddenCount);
    void HideMoreRequestsToLoadHint();
    bool ContainsFriend(string userId);
    bool ContainsFriendRequest(string userId);
    void EnableSearchMode();
    void DisableSearchMode();
    void UpdateBlockStatus(string userId, bool blocked);
    void ClearAll();
}