using System;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FriendsHUDView : MonoBehaviour, IFriendsHUDComponentView
{
    public const string NOTIFICATIONS_ID = "Friends";
    static readonly int ANIM_PROPERTY_SELECTED = Animator.StringToHash("Selected");
    const string VIEW_PATH = "FriendsHUD";
    const int PREINSTANTIATED_FRIENDS_ENTRIES = 25;
    const int PREINSTANTIATED_FRIENDS_REQUEST_ENTRIES = 5;
    
    public event Action<FriendRequestEntry> OnFriendRequestApproved
    {
        add => friendRequestsList.OnFriendRequestApproved += value;
        remove => friendRequestsList.OnFriendRequestApproved -= value;
    }
    public event Action<FriendRequestEntry> OnCancelConfirmation
    {
        add => friendRequestsList.OnCancelConfirmation += value;
        remove => friendRequestsList.OnCancelConfirmation -= value;
    }
    public event Action<FriendRequestEntry> OnRejectConfirmation
    {
        add => friendRequestsList.OnRejectConfirmation += value;
        remove => friendRequestsList.OnRejectConfirmation -= value;
    }
    public event Action<string> OnFriendRequestSent
    {
        add => friendRequestsList.OnFriendRequestSent += value;
        remove => friendRequestsList.OnFriendRequestSent -= value;
    }
    public event Action<FriendEntry> OnWhisper
    {
        add => friendsList.OnWhisper += value;
        remove => friendsList.OnWhisper -= value;
    }
    public event Action<string> OnDeleteConfirmation
    {
        add => friendsList.OnDeleteConfirmation += value;
        remove => friendsList.OnDeleteConfirmation -= value;
    }
    public event Action OnClose;

    public Button closeButton;
    public Button friendsButton;
    public Button friendRequestsButton;
    public FriendsTabView friendsList;
    public FriendRequestsTabView friendRequestsList;
    public GameObject spinner;

    public float notificationsDuration = 3f;

    private FriendsHUDController controller;
    
    public RectTransform Transform => transform as RectTransform;

    public static FriendsHUDView Create(FriendsHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize(controller);
        return view;
    }

    public List<FriendEntryBase> GetAllEntries()
    {
        var result = new List<FriendEntryBase>();
        result.AddRange(friendsList.GetAllEntries());
        result.AddRange(friendRequestsList.GetAllEntries());
        return result;
    }

    public FriendEntryBase GetEntry(string userId)
    {
        return friendsList.GetEntry(userId) ?? friendRequestsList.GetEntry(userId);
    }

    public void UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        friendsList.UpdateEntry(userId, model);
        friendRequestsList.UpdateEntry(userId, model);
    }

    public void DisplayFriendUserNotFound() => friendRequestsList.DisplayFriendUserNotFound();

    public bool IsFriendListFocused() => friendsList.gameObject.activeInHierarchy;

    public int GetReceivedFriendRequestCount() => friendRequestsList.receivedRequestsList.Count();

    public void Destroy()
    {
        if (!this) return;
        if (!gameObject) return;
        Destroy(gameObject);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        
        if (friendsButton.interactable)
            friendsButton.onClick.Invoke();
    }

    public void Hide() => gameObject.SetActive(false);
    
    public void UpdateFriendshipStatus(string userId, FriendshipAction friendshipAction,
        FriendEntryBase.Model friendEntryModel)
    {
        switch (friendshipAction)
        {
            case FriendshipAction.NONE:
                friendRequestsList.RemoveEntry(userId);
                friendsList.RemoveEntry(userId);
                break;
            case FriendshipAction.APPROVED:
                friendRequestsList.RemoveEntry(userId);
                friendsList.CreateOrUpdateEntryDeferred(userId, friendEntryModel);
                break;
            case FriendshipAction.REJECTED:
                friendRequestsList.RemoveEntry(userId);
                break;
            case FriendshipAction.CANCELLED:
                friendRequestsList.RemoveEntry(userId);
                break;
            case FriendshipAction.REQUESTED_FROM:
                friendRequestsList.CreateOrUpdateEntry(userId, friendEntryModel, true);
                break;
            case FriendshipAction.REQUESTED_TO:
                friendRequestsList.CreateOrUpdateEntry(userId,  friendEntryModel, false);
                break;
            case FriendshipAction.DELETED:
                friendRequestsList.RemoveEntry(userId);
                friendsList.RemoveEntry(userId);
                break;
        }
    }

    public void Search(string userId) => friendRequestsList.friendSearchInputField.onSubmit.Invoke(userId);
    
    public bool IsActive() => gameObject.activeInHierarchy;

    public bool IsFriendListCreationReady() => friendsList.creationQueue.Count == 0;

    public void ShowSpinner()
    {
        spinner.gameObject.SetActive(true);

        friendsList.gameObject.SetActive(false);
        friendRequestsList.gameObject.SetActive(false);

        friendsButton.interactable = false;
        friendRequestsButton.interactable = false;
    }

    public void HideSpinner()
    {
        spinner.gameObject.SetActive(false);

        friendsList.gameObject.SetActive(true);
        friendRequestsList.gameObject.SetActive(false);

        friendsButton.interactable = true;
        friendsButton.onClick.Invoke();

        friendRequestsButton.interactable = true;
    }

    private void Initialize(FriendsHUDController controller)
    {
        this.controller = controller;
        friendsList.Initialize(this, PREINSTANTIATED_FRIENDS_ENTRIES);
        friendRequestsList.Initialize(this, PREINSTANTIATED_FRIENDS_REQUEST_ENTRIES);

        closeButton.onClick.AddListener(OnCloseButtonPressed);

        friendsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendsList.gameObject.SetActive(true);
            friendRequestsList.gameObject.SetActive(false);
            Utils.ForceUpdateLayout(friendsList.transform as RectTransform);
        });

        friendRequestsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendsList.gameObject.SetActive(false);
            friendRequestsList.gameObject.SetActive(true);
            Utils.ForceUpdateLayout(friendRequestsList.transform as RectTransform);
        });

        if (friendsButton.interactable)
            friendsButton.onClick.Invoke();
    }

    public void OnCloseButtonPressed()
    {
        OnClose?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("AddFakeRequestReceived")]
    public void AddFakeRequestReceived()
    {
        string id1 = Random.Range(0, 1000000).ToString();
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.REQUESTED_FROM
        });
    }

    [ContextMenu("AddFakeRequestSent")]
    public void AddFakeRequestSent()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.REQUESTED_TO
        });
    }

    [ContextMenu("AddFakeRequestSentAccepted")]
    public void AddFakeRequestSentAccepted()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.REQUESTED_TO
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.APPROVED
        });
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus()
            { userId = id1, presence = PresenceStatus.ONLINE });
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus()
            { userId = id1, presence = PresenceStatus.OFFLINE });
    }
#endif
}