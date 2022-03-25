using System;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView
{
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject directChatsLoadingContainer;
    [SerializeField] private GameObject directChatsContainer;
    [SerializeField] private TMP_Text directChatsHeaderLabel;
    [SerializeField] private ScrollRect scroll;
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
        UpdateDirectChatsHeader();
    }

    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateRecipient(UserProfile user, ChatMessage recentMessage, bool isBlocked, PresenceStatus presence)
    {
        directChatList.Set(user.userId, new DirectChatEntry.DirectChatEntryModel(
            user.userId,
            user.userName,
            recentMessage.body,
            user.face256SnapshotURL,
            isBlocked,
            presence));
        UpdateDirectChatsHeader();
    }

    public void ShowPrivateChatsLoading() => SetPrivateChatLoadingVisibility(true);

    public void HidePrivateChatsLoading() => SetPrivateChatLoadingVisibility(false);

    public override void RefreshControl()
    {
        directChatList.Clear();
        foreach (var entry in model.entries)
            directChatList.Set(entry.userId, entry);
        SetPrivateChatLoadingVisibility(model.isLoadingDirectChats);
    }

    private void SetPrivateChatLoadingVisibility(bool visible)
    {
        model.isLoadingDirectChats = visible;
        directChatsLoadingContainer.SetActive(visible);
        directChatsContainer.SetActive(!visible);
        scroll.enabled = !visible;
    }

    private void UpdateDirectChatsHeader()
    {
        directChatsHeaderLabel.text = $"Direct Messages ({directChatList.Count()})";
    }

    [Serializable]
    private class Model
    {
        public DirectChatEntry.DirectChatEntryModel[] entries;
        public bool isLoadingDirectChats;
    }
}