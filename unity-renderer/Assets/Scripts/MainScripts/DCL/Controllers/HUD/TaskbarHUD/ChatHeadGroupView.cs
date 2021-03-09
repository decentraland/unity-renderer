using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

public class ChatHeadGroupView : MonoBehaviour
{
    const int MAX_GROUP_SIZE = 5;
    const string CHAT_HEAD_PATH = "ChatHead";
    const string PLAYER_PREFS_LATEST_OPEN_CHATS = "LatestOpenChats";

    public event System.Action<TaskbarButton> OnHeadToggleOn;
    public event System.Action<TaskbarButton> OnHeadToggleOff;

    public Transform container;

    [System.NonSerialized]
    public List<ChatHeadButton> chatHeads = new List<ChatHeadButton>();

    private IChatController chatController;
    private IFriendsController friendsController;
    private ulong rendererStateTimeMark;
    private RectTransform contentParentRT;

    public void Initialize(IChatController chatController, IFriendsController friendsController)
    {
        this.chatController = chatController;
        this.friendsController = friendsController;

        if (chatController != null)
            chatController.OnAddMessage += ChatController_OnAddMessage;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += FriendsController_OnUpdateFriendship;
            friendsController.OnUpdateUserStatus += FriendsController_OnUpdateUserStatus;
            friendsController.OnInitialized += FriendsController_OnInitialized;
        }

        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
    }

    private void Start()
    {
        contentParentRT = container.parent as RectTransform;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // NOTE: Update is only enabled when SetParentContainerAsDirty is called 
        DCL.Helpers.Utils.ForceUpdateLayout(contentParentRT, delayed: true);
        this.enabled = false;
    }

    private void FriendsController_OnUpdateFriendship(string id, FriendshipAction action)
    {
        if (action != FriendshipAction.NONE)
            return;

        RemoveChatHead(id);
    }

    private void FriendsController_OnUpdateUserStatus(string userId, FriendsController.UserStatus userStatus)
    {
        ChatHeadButton updatedChatHead = chatHeads.FirstOrDefault(ch => ch.profile.userId == userId);
        if (updatedChatHead != null)
            updatedChatHead.SetOnlineStatus(userStatus.presence == PresenceStatus.ONLINE);
    }

    private void FriendsController_OnInitialized()
    {
        // Load the chat heads from local storage just after FriendsController has been initialized
        LoadLatestOpenChats();
        friendsController.OnInitialized -= FriendsController_OnInitialized;
    }

    private void RendererState_OnChange(bool current, bool previous)
    {
        rendererStateTimeMark = (ulong) System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void OnDestroy()
    {
        if (chatController != null)
            chatController.OnAddMessage -= ChatController_OnAddMessage;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship -= FriendsController_OnUpdateFriendship;
            friendsController.OnUpdateUserStatus -= FriendsController_OnUpdateUserStatus;
            friendsController.OnInitialized -= FriendsController_OnInitialized;
        }

        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void ChatController_OnAddMessage(DCL.Interface.ChatMessage obj)
    {
        if (!CommonScriptableObjects.rendererState.Get() ||
            obj.messageType != DCL.Interface.ChatMessage.Type.PRIVATE ||
            obj.timestamp < rendererStateTimeMark)
            return;

        var ownProfile = UserProfile.GetOwnUserProfile();

        string userId = string.Empty;

        if (obj.sender != ownProfile.userId)
            userId = obj.sender;
        else if (obj.recipient != ownProfile.userId)
            userId = obj.recipient;

        if (!string.IsNullOrEmpty(userId))
        {
            AddChatHead(userId, obj.timestamp);
        }
    }

    private void OnToggleOn(TaskbarButton head)
    {
        OnHeadToggleOn?.Invoke(head);
    }

    private void OnToggleOff(TaskbarButton head)
    {
        if (!(head is ChatHeadButton))
            return;

        OnHeadToggleOff?.Invoke(head);
    }

    private void SortChatHeads()
    {
        chatHeads = chatHeads.OrderByDescending((x) => x.lastTimestamp).ToList();

        for (int i = 0; i < chatHeads.Count; i++)
        {
            chatHeads[i].transform.SetSiblingIndex(i);
        }
    }

    internal ChatHeadButton AddChatHead(string userId, ulong timestamp, bool saveStatusInStorage = true, bool forceLayoutUpdate = true)
    {
        var existingHead = chatHeads.FirstOrDefault(x => x.profile.userId == userId);

        if (existingHead != null)
        {
            existingHead.lastTimestamp = timestamp;
            SortChatHeads();

            if (saveStatusInStorage)
            {
                LatestOpenChatsList.Model existingHeadInStorage = CommonScriptableObjects.latestOpenChats.GetList().FirstOrDefault(c => c.userId == userId);
                if (existingHeadInStorage != null)
                {
                    existingHeadInStorage.lastTimestamp = timestamp;
                    SaveLatestOpenChats();
                }
            }

            return existingHead;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        GameObject prefab = Resources.Load(CHAT_HEAD_PATH) as GameObject;
        GameObject instance = Instantiate(prefab, container);
        ChatHeadButton chatHead = instance.GetComponent<ChatHeadButton>();

        chatHead.Initialize(UserProfileController.userProfilesCatalog.Get(userId));
        chatHead.lastTimestamp = timestamp;
        chatHead.OnToggleOn += OnToggleOn;
        chatHead.OnToggleOff += OnToggleOff;

        if (friendsController != null &&
            friendsController.GetFriends().TryGetValue(userId, out FriendsController.UserStatus friendStatus))
        {
            chatHead.SetOnlineStatus(friendStatus.presence == PresenceStatus.ONLINE);
        }

        var animator = chatHead.GetComponent<ShowHideAnimator>();

        if (animator != null)
            animator.Show();

        chatHeads.Add(chatHead);
        SortChatHeads();

        if (saveStatusInStorage)
        {
            CommonScriptableObjects.latestOpenChats.Add(new LatestOpenChatsList.Model {userId = userId, lastTimestamp = timestamp});
            SaveLatestOpenChats();
        }

        if (chatHeads.Count > MAX_GROUP_SIZE)
        {
            var lastChatHead = chatHeads[chatHeads.Count - 1];
            RemoveChatHead(lastChatHead, saveStatusInStorage, forceLayoutUpdate: false);
        }

        if (forceLayoutUpdate)
        {
            SetParentContainerAsDirty();
        }

        return chatHead;
    }

    internal void ClearChatHeads()
    {
        var chatHeadsToRemove = chatHeads.ToArray();

        for (int i = 0; i < chatHeadsToRemove.Length; i++)
        {
            RemoveChatHead(chatHeadsToRemove[i], forceLayoutUpdate: false);
        }

        SetParentContainerAsDirty();
    }

    internal void RemoveChatHead(string userId, bool forceLayoutUpdate = true)
    {
        RemoveChatHead(chatHeads.FirstOrDefault(x => x.profile.userId == userId), true, forceLayoutUpdate);
    }

    internal void RemoveChatHead(ChatHeadButton chatHead, bool saveStatusInStorage = true, bool forceLayoutUpdate = true)
    {
        if (chatHead == null)
            return;

        var animator = chatHead.GetComponent<ShowHideAnimator>();

        if (animator != null)
        {
            animator.OnWillFinishHide -= Animator_OnWillFinishHide;
            animator.OnWillFinishHide += Animator_OnWillFinishHide;
            animator.Hide();
        }
        else
        {
            DestroyChatHead(chatHead.gameObject, forceLayoutUpdate);
        }

        chatHeads.Remove(chatHead);

        if (saveStatusInStorage)
        {
            LatestOpenChatsList.Model chatHeadToRemove = CommonScriptableObjects.latestOpenChats.GetList().FirstOrDefault(c => c.userId == chatHead.profile.userId);
            CommonScriptableObjects.latestOpenChats.Remove(chatHeadToRemove);
            SaveLatestOpenChats();
        }
    }

    private void Animator_OnWillFinishHide(ShowHideAnimator animator)
    {
        animator.OnWillFinishHide -= Animator_OnWillFinishHide;
        DestroyChatHead(animator.gameObject, true);
    }

    private void SaveLatestOpenChats()
    {
        PlayerPrefsUtils.SetString(PLAYER_PREFS_LATEST_OPEN_CHATS, JsonConvert.SerializeObject(CommonScriptableObjects.latestOpenChats.GetList()));
        PlayerPrefsUtils.Save();
    }

    private void LoadLatestOpenChats()
    {
        CommonScriptableObjects.latestOpenChats.Clear();
        List<LatestOpenChatsList.Model> latestOpenChatsFromStorage = JsonConvert.DeserializeObject<List<LatestOpenChatsList.Model>>(PlayerPrefs.GetString(PLAYER_PREFS_LATEST_OPEN_CHATS));
        if (latestOpenChatsFromStorage != null)
        {
            foreach (LatestOpenChatsList.Model item in latestOpenChatsFromStorage)
            {
                if (UserProfileController.userProfilesCatalog.Get(item.userId) == null)
                    continue;

                CommonScriptableObjects.latestOpenChats.Add(item);
                AddChatHead(item.userId, item.lastTimestamp, false, forceLayoutUpdate: false);
            }

            SetParentContainerAsDirty();
        }
    }

    private void DestroyChatHead(GameObject chatHeadGO, bool forceLayoutUpdate)
    {
        Destroy(chatHeadGO);

        if (container.childCount <= 1)
        {
            gameObject.SetActive(false);
        }
        else if (forceLayoutUpdate)
        {
            SetParentContainerAsDirty();
        }
    }

    private void SetParentContainerAsDirty()
    {
        this.enabled = true;
    }
}