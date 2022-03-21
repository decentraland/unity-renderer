using System;
using DCL.Interface;
using UnityEngine;

public class WorldChatWindowComponentView : MonoBehaviour, IWorldChatWindowView
{
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    
    public event Action OnClose;
    
    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeInHierarchy;
    
    public static WorldChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<WorldChatWindowComponentView>("SocialBarV1/ConversationListHUD"));
    }

    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetDirectRecipient(UserProfile user, ChatMessage recentMessage)
    {
        directChatList.Set(user.userId, new DirectChatEntry.DirectChatEntryModel(
            user.userId, user.userName, "TODO", user.face256SnapshotURL));
    }
}