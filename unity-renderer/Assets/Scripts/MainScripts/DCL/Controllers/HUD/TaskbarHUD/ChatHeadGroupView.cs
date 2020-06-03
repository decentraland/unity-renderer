using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChatHeadGroupView : MonoBehaviour
{
    const int MAX_GROUP_SIZE = 5;
    const string CHAT_HEAD_PATH = "ChatHead";

    public event System.Action<TaskbarButton> OnHeadToggleOn;
    public event System.Action<TaskbarButton> OnHeadToggleOff;

    public Transform container;
    [System.NonSerialized] public List<ChatHeadButton> chatHeads = new List<ChatHeadButton>();

    private IChatController chatController;
    private IFriendsController friendsController;

    public void Initialize(IChatController chatController, IFriendsController friendsController)
    {
        this.chatController = chatController;
        this.friendsController = friendsController;

        if (chatController != null)
            chatController.OnAddMessage += ChatController_OnAddMessage;

        if (friendsController != null)
            friendsController.OnUpdateFriendship += FriendsController_OnUpdateFriendship;
    }

    private void FriendsController_OnUpdateFriendship(string id, FriendshipAction action)
    {
        if (action != FriendshipAction.NONE)
            return;

        RemoveChatHead(id);
    }

    private void OnDestroy()
    {
        if (chatController != null)
            chatController.OnAddMessage -= ChatController_OnAddMessage;

        if (friendsController != null)
            friendsController.OnUpdateFriendship -= FriendsController_OnUpdateFriendship;
    }

    private void ChatController_OnAddMessage(DCL.Interface.ChatMessage obj)
    {
        if (obj.messageType != DCL.Interface.ChatMessage.Type.PRIVATE)
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

    internal ChatHeadButton AddChatHead(string userId, ulong timestamp)
    {
        var existingHead = chatHeads.FirstOrDefault(x => x.profile.userId == userId);

        if (existingHead != null)
        {
            existingHead.lastTimestamp = timestamp;
            SortChatHeads();
            return existingHead;
        }

        GameObject prefab = Resources.Load(CHAT_HEAD_PATH) as GameObject;
        GameObject instance = Instantiate(prefab, container);
        ChatHeadButton chatHead = instance.GetComponent<ChatHeadButton>();

        chatHead.Initialize(UserProfileController.userProfilesCatalog.Get(userId));
        chatHead.lastTimestamp = timestamp;
        chatHead.OnToggleOn += OnToggleOn;
        chatHead.OnToggleOff += OnToggleOff;

        var animator = chatHead.GetComponent<ShowHideAnimator>();

        if (animator != null)
            animator.Show();

        chatHeads.Add(chatHead);
        SortChatHeads();

        if (chatHeads.Count > MAX_GROUP_SIZE)
        {
            var lastChatHead = chatHeads[chatHeads.Count - 1];
            RemoveChatHead(lastChatHead);
        }

        return chatHead;
    }

    internal void ClearChatHeads()
    {
        var chatHeadsToRemove = chatHeads.ToArray();

        for (int i = 0; i < chatHeadsToRemove.Length; i++)
        {
            RemoveChatHead(chatHeadsToRemove[i]);
        }
    }

    internal void RemoveChatHead(string userId)
    {
        RemoveChatHead(chatHeads.FirstOrDefault(x => x.profile.userId == userId));
    }

    internal void RemoveChatHead(ChatHeadButton chatHead)
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
            Destroy(chatHead.gameObject);
        }

        chatHeads.Remove(chatHead);
    }

    private void Animator_OnWillFinishHide(ShowHideAnimator animator)
    {
        animator.OnWillFinishHide -= Animator_OnWillFinishHide;
        Destroy(animator.gameObject);
    }
}