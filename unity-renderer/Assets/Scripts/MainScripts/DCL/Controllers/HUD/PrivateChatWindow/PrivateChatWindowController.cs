using System;
using System.Linq;
using DCL;
using DCL.Interface;

public class PrivateChatWindowController : IHUD
{
    public IPrivateChatComponentView view;

    public string conversationUserId { get; private set; } = string.Empty;

    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;
    private readonly InputAction_Trigger closeWindowTrigger;
    private readonly ILastReadMessagesService lastReadMessagesService;
    private string conversationUserName = string.Empty;
    private ChatHUDController chatHudController;
    private UserProfile conversationProfile;

    private bool isSocialBarV1Enabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("social_bar_v1");

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
        if (view == null)
        {
            if (isSocialBarV1Enabled)
                view = PrivateChatWindowComponentView.Create();
            else
                view = PrivateChatWindowHUDView.Create();
        }

        this.view = view;
        view.OnPressBack -= View_OnPressBack;
        view.OnPressBack += View_OnPressBack;
        view.OnClose -= OnCloseView;
        view.OnClose += OnCloseView;
        view.OnMinimize += OnMinimizeView;
        closeWindowTrigger.OnTriggered -= HandleCloseInputTriggered;
        closeWindowTrigger.OnTriggered += HandleCloseInputTriggered;

        chatHudController = new ChatHUDController(DataStore.i, userProfileBridge, false);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnInputFieldSelected -= HandleInputFieldSelection;
        chatHudController.OnInputFieldSelected += HandleInputFieldSelection;
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.FocusInputField();

        if (chatController != null)
        {
            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;
        }
    }

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId)
            return;

        UserProfile newConversationUserProfile = userProfileBridge.Get(newConversationUserId);

        conversationUserId = newConversationUserId;
        conversationUserName = newConversationUserProfile.userName;
        conversationProfile = newConversationUserProfile;

        var userStatus = friendsController.GetUserStatus(conversationUserId);

        view.Setup(newConversationUserProfile,
            userStatus.presence == PresenceStatus.ONLINE,
            userProfileBridge.GetOwn().IsBlocked(conversationUserId));

        chatHudController.ClearAllEntries();

        var messageEntries = chatController.GetEntries()
            .Where(IsMessageFomCurrentConversation)
            .ToList();
        foreach (var v in messageEntries)
            HandleMessageReceived(v);
    }

    public void SendChatMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.body)) return;
        if (string.IsNullOrEmpty(conversationUserName)) return;
        
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

    public void SetVisibility(bool visible)
    {
        if (view.IsActive == visible) return;

        if (visible)
        {
            if (conversationProfile != null)
            {
                var userStatus = friendsController.GetUserStatus(conversationUserId);
                view.Setup(conversationProfile,
                    userStatus.presence == PresenceStatus.ONLINE,
                    userProfileBridge.GetOwn().IsBlocked(conversationUserId));
            }

            view.Show();
            chatHudController.FocusInputField();
            MarkUserChatMessagesAsRead(conversationUserId);
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

    private void HandleCloseInputTriggered(DCLAction_Trigger action) => OnCloseView();

    private void OnMinimizeView() => SetVisibility(false);

    private void HandleMessageReceived(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        chatHudController.AddChatMessage(message);

        if (conversationProfile.userId == conversationUserId)
        {
            // The messages from 'conversationUserId' are marked as read if his private chat window is currently open
            MarkUserChatMessagesAsRead(conversationUserId);
        }
    }

    private void OnCloseView() => SetVisibility(false);

    private void View_OnPressBack() => OnPressBack?.Invoke();

    private bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE &&
               (message.sender == conversationUserId || message.recipient == conversationUserId);
    }

    private void MarkUserChatMessagesAsRead(string userId) =>
        lastReadMessagesService.MarkAllRead(userId);

    private void HandleInputFieldSelection()
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead(conversationUserId);
    }
}