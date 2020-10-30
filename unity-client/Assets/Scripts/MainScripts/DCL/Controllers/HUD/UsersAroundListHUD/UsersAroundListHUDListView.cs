using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListView : MonoBehaviour, IUsersAroundListHUDListView
{
    public event Action<string, bool> OnRequestMuteUser;
    public event Action<bool> OnRequestMuteGlobal;

    [SerializeField] private UsersAroundListHUDListElementView listElementView;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] internal TMPro.TextMeshProUGUI textFriendsTitle;
    [SerializeField] internal TMPro.TextMeshProUGUI textPlayersTitle;
    [SerializeField] internal Transform contentFriends;
    [SerializeField] internal Transform contentPlayers;
    [SerializeField] internal Toggle muteAllToggle;
    [SerializeField] internal UserContextMenu contextMenu;
    [SerializeField] internal UserContextConfirmationDialog confirmationDialog;

    internal Queue<UsersAroundListHUDListElementView> availableElements;
    internal Dictionary<string, UsersAroundListHUDListElementView> userElementDictionary;

    private string friendsTextPattern;
    private string playersTextPattern;
    private int friendsCount = 0;
    private int playersCount = 0;

    private void Awake()
    {
        availableElements = new Queue<UsersAroundListHUDListElementView>();
        userElementDictionary = new Dictionary<string, UsersAroundListHUDListElementView>();

        friendsTextPattern = textFriendsTitle.text;
        playersTextPattern = textPlayersTitle.text;
        textFriendsTitle.text = string.Format(friendsTextPattern, friendsCount);
        textPlayersTitle.text = string.Format(playersTextPattern, playersCount);

        muteAllToggle.onValueChanged.AddListener(OnMuteGlobal);

        listElementView.OnMuteUser += OnMuteUser;
        listElementView.OnShowUserContexMenu += OnUserContextMenu;
        listElementView.OnPoolRelease();
        availableElements.Enqueue(listElementView);

        if (FriendsController.i)
            FriendsController.i.OnUpdateFriendship += OnUpdateFriendship;
    }

    void IUsersAroundListHUDListView.AddOrUpdateUser(MinimapMetadata.MinimapUserInfo userInfo)
    {
        if (userElementDictionary.ContainsKey(userInfo.userId))
        {
            return;
        }

        var profile = UserProfileController.userProfilesCatalog.Get(userInfo.userId);

        if (profile == null)
            return;

        bool isFriend = false;

        if (FriendsController.i && FriendsController.i.friends.TryGetValue(userInfo.userId, out FriendsController.UserStatus status))
        {
            isFriend = status.friendshipStatus == FriendshipStatus.FRIEND;
        }

        UsersAroundListHUDListElementView view = null;
        if (availableElements.Count > 0)
        {
            view = availableElements.Dequeue();
            view.transform.SetParent(isFriend ? contentFriends : contentPlayers);
        }
        else
        {
            view = Instantiate(listElementView, isFriend ? contentFriends : contentPlayers);
            view.OnMuteUser += OnMuteUser;
            view.OnShowUserContexMenu += OnUserContextMenu;
        }

        view.OnPoolGet();
        view.SetUserProfile(profile);
        userElementDictionary.Add(userInfo.userId, view);
        ModifyListCount(isFriend, 1);
    }

    void IUsersAroundListHUDListView.RemoveUser(string userId)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        if (!elementView)
        {
            return;
        }

        ModifyListCount(elementView.transform.parent == contentFriends, -1);
        PoolElementView(elementView);
        userElementDictionary.Remove(userId);
    }

    void IUsersAroundListHUDListView.SetUserRecording(string userId, bool isRecording)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetRecording(isRecording);
    }

    void IUsersAroundListHUDListView.SetUserMuted(string userId, bool isMuted)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetMuted(isMuted);
    }

    void IUsersAroundListHUDListView.SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            showHideAnimator.Show();
        }
        else
        {
            showHideAnimator.Hide();
            contextMenu.Hide();
            confirmationDialog.Hide();
        }
    }

    void IUsersAroundListHUDListView.Dispose()
    {
        if (FriendsController.i)
            FriendsController.i.OnUpdateFriendship -= OnUpdateFriendship;

        userElementDictionary.Clear();
        availableElements.Clear();
        Destroy(gameObject);
    }

    void OnMuteUser(string userId, bool mute)
    {
        OnRequestMuteUser?.Invoke(userId, mute);
    }

    void OnMuteGlobal(bool mute)
    {
        OnRequestMuteGlobal?.Invoke(mute);
    }

    void OnUserContextMenu(Vector3 position, string userId)
    {
        contextMenu.transform.position = position;
        contextMenu.Show(userId);
    }

    void PoolElementView(UsersAroundListHUDListElementView element)
    {
        element.OnPoolRelease();
        availableElements.Enqueue(element);
    }

    void ModifyListCount(bool friendList, int delta)
    {
        if (friendList)
        {
            friendsCount += delta;
            textFriendsTitle.text = string.Format(friendsTextPattern, friendsCount);
        }
        else
        {
            playersCount += delta;
            textPlayersTitle.text = string.Format(playersTextPattern, playersCount);
        }
    }

    bool IsInFriendsList(UsersAroundListHUDListElementView element)
    {
        return element.transform.parent == contentFriends;
    }

    void OnUpdateFriendship(string userId, FriendshipAction status)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }

        bool isFriend = IsFriend(status);
        bool isInFriendsList = IsInFriendsList(elementView);

        if (isFriend && !isInFriendsList)
        {
            ModifyListCount(friendList: false, -1);
            ModifyListCount(friendList: true, 1);
            elementView.transform.SetParent(contentFriends);
        }
        else if (!isFriend && isInFriendsList)
        {
            ModifyListCount(friendList: true, -1);
            ModifyListCount(friendList: false, 1);
            elementView.transform.SetParent(contentPlayers);
        }
    }

    bool IsFriend(FriendshipAction status)
    {
        return status == FriendshipAction.APPROVED;
    }
}
