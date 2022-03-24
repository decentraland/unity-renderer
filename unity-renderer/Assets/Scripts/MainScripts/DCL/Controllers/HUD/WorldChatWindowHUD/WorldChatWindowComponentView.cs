using System;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView
{
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private Button closeButton;
    [SerializeField] private Model model;
    
    public event Action OnClose;
    public event Action<string> OnOpenChat; 
    
    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeInHierarchy;
    
    public static WorldChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<WorldChatWindowComponentView>("SocialBarV1/ConversationListHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        directChatList.OnOpenChat += entry => OnOpenChat?.Invoke(entry.Model.userId);
    }

    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetDirectRecipient(UserProfile user, ChatMessage recentMessage, bool isBlocked, PresenceStatus presence)
    {
        directChatList.Set(user.userId, new DirectChatEntry.DirectChatEntryModel(
            user.userId,
            user.userName,
            recentMessage.body,
            user.face256SnapshotURL,
            isBlocked,
            presence));
    }

    public override void RefreshControl()
    {
        directChatList.Clear();
        foreach (var entry in model.entries)
            directChatList.Set(entry.userId, entry);
    }

    [Serializable]
    private class Model
    {
        public DirectChatEntry.DirectChatEntryModel[] entries;
    }
}