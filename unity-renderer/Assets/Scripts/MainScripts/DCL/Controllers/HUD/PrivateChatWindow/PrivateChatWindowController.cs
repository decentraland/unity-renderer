using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;

public class PrivateChatWindowController : IHUD
{
    public IPrivateChatComponentView view;

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly InputAction_Trigger closeWindowTrigger;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;

    private string ConversationUserId { get; set; } = string.Empty;

    public event Action OnPressBack;

    public PrivateChatWindowController(DataStore dataStore,
        IUserProfileBridge userProfileBridge,
        IChatController chatController,
        IFriendsController friendsController,
        InputAction_Trigger closeWindowTrigger,
        ILastReadMessagesService lastReadMessagesService)
    {
        this.dataStore = dataStore;
        this.userProfileBridge = userProfileBridge;
        this.chatController = chatController;
        this.friendsController = friendsController;
        this.closeWindowTrigger = closeWindowTrigger;
        this.lastReadMessagesService = lastReadMessagesService;
    }

    public void Initialize(IPrivateChatComponentView view = null)
    {
        view ??= PrivateChatWindowComponentView.Create();
        this.view = view;
        view.OnPressBack -= View_OnPressBack;
        view.OnPressBack += View_OnPressBack;
        view.OnClose -= OnCloseView;
        view.OnClose += OnCloseView;
        view.OnMinimize += OnMinimizeView;
        closeWindowTrigger.OnTriggered -= HandleCloseInputTriggered;
        closeWindowTrigger.OnTriggered += HandleCloseInputTriggered;

        chatHudController = new ChatHUDController(dataStore, userProfileBridge, false);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelection;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelection;
        chatHudController.OnSendMessage += HandleSendChatMessage;
        chatHudController.FocusInputField();

        if (chatController != null)
        {
            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;
        }
    }

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == ConversationUserId) return;

        UserProfile newConversationUserProfile = userProfileBridge.Get(newConversationUserId);

        ConversationUserId = newConversationUserId;
        conversationProfile = newConversationUserProfile;

        var userStatus = friendsController.GetUserStatus(ConversationUserId);

        view.Setup(newConversationUserProfile,
            userStatus.presence == PresenceStatus.ONLINE,
            userProfileBridge.GetOwn().IsBlocked(ConversationUserId));

        chatHudController.ClearAllEntries();

        ReloadAllChats().Forget();
    }

    public void SetVisibility(bool visible)
    {
        if (view.IsActive == visible) return;

        if (visible)
        {
            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(ConversationUserId);
                view.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(ConversationUserId));
            }

            view.Show();
            chatHudController.FocusInputField();
            MarkUserChatMessagesAsRead(ConversationUserId);
        }
        else
        {
            view.Hide();
        }
    }

    public void Dispose()
    {
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelection;
        view.OnPressBack -= View_OnPressBack;
        view.OnClose -= OnCloseView;
        view.OnMinimize -= OnMinimizeView;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        view?.Dispose();
    }

    private async UniTaskVoid ReloadAllChats()
    {
        chatHudController.ClearAllEntries();

        const int entriesPerFrame = 10;
        var list = chatController.GetEntries();
        if (list.Count == 0) return;
        
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var message = list[i];
            if (i % entriesPerFrame == 0) await UniTask.NextFrame();
            if (!IsMessageFomCurrentConversation(message)) continue;
            HandleMessageReceived(message);
        }
    }

    private void HandleSendChatMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.body)) return;
        if (string.IsNullOrEmpty(conversationProfile.userName)) return;
        
        message.messageType = ChatMessage.Type.PRIVATE;
        message.recipient = conversationProfile.userName;

        bool isValidMessage = !string.IsNullOrEmpty(message.body)
                              && !string.IsNullOrWhiteSpace(message.body)
                              && !string.IsNullOrEmpty(message.recipient);

        chatHudController.ResetInputField();
        chatHudController.FocusInputField();

        if (!isValidMessage) return;

        // If Kernel allowed for private messages without the whisper param we could avoid this line
        message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
    }

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => OnCloseView();

    private void OnMinimizeView() => SetVisibility(false);

    private void HandleMessageReceived(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        chatHudController.AddChatMessage(message);

        if (conversationProfile != null && conversationProfile.userId == ConversationUserId)
        {
            // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
            MarkUserChatMessagesAsRead(ConversationUserId);
        }
    }

    private void OnCloseView() => SetVisibility(false);

    private void View_OnPressBack() => OnPressBack?.Invoke();

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == ConversationUserId || message.recipient == ConversationUserId);
    }

    private void MarkUserChatMessagesAsRead(string userId) =>
        lastReadMessagesService.MarkAllRead(userId);

    private void HandleInputFieldSelection()
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead(ConversationUserId);
    }
}