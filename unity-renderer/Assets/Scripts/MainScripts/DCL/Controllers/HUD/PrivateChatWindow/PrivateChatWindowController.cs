using System;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;

public class PrivateChatWindowController : IHUD
{
    public IPrivateChatComponentView View { get; private set; }

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
    public event Action OnClosed;

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
        this.View = view;
        view.OnPressBack -= HandlePressBack;
        view.OnPressBack += HandlePressBack;
        view.OnClose -= Hide;
        view.OnClose += Hide;
        view.OnMinimize += MinimizeView;
        view.OnUnfriend += Unfriend;
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

    public void Setup(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == ConversationUserId) return;

        UserProfile newConversationUserProfile = userProfileBridge.Get(newConversationUserId);

        ConversationUserId = newConversationUserId;
        conversationProfile = newConversationUserProfile;

        var userStatus = friendsController.GetUserStatus(ConversationUserId);

        View.Setup(newConversationUserProfile,
            userStatus.presence == PresenceStatus.ONLINE,
            userProfileBridge.GetOwn().IsBlocked(ConversationUserId));

        ReloadAllChats().Forget();
    }

    public void SetVisibility(bool visible)
    {
        if (View.IsActive == visible) return;

        if (visible)
        {
            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(ConversationUserId);
                View.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(ConversationUserId));
            }

            View.Show();
            chatHudController.FocusInputField();
            MarkUserChatMessagesAsRead();
        }
        else
        {
            View.Hide();
        }
    }

    public void Dispose()
    {
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelection;
        View.OnPressBack -= HandlePressBack;
        View.OnClose -= Hide;
        View.OnMinimize -= MinimizeView;
        View.OnUnfriend -= Unfriend;

        if (chatController != null)
            chatController.OnAddMessage -= HandleMessageReceived;

        View?.Dispose();
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
            if (i != 0 && i % entriesPerFrame == 0) await UniTask.NextFrame();
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

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => Hide();

    private void MinimizeView() => SetVisibility(false);

    private void HandleMessageReceived(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        chatHudController.AddChatMessage(message);

        if (View.IsActive)
        {
            // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
            MarkUserChatMessagesAsRead();
        }
    }

    private void Hide()
    {
        SetVisibility(false);
        OnClosed?.Invoke();
    }

    private void HandlePressBack() => OnPressBack?.Invoke();

    private void Unfriend(string friendId)
    {
        friendsController.RemoveFriend(friendId);
        Hide();
    }

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == ConversationUserId || message.recipient == ConversationUserId);
    }

    private void MarkUserChatMessagesAsRead() =>
        lastReadMessagesService.MarkAllRead(ConversationUserId);

    private void HandleInputFieldSelection()
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead();
    }
}