using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using Channel = DCL.Chat.Channels.Channel;

public class PublicChatWindowController : IHUD
{
    public IPublicChatWindowView View { get; private set; }
    
    public event Action OnBack;
    public event Action OnClosed;

    private readonly IChatController chatController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;
    private readonly IMouseCatcher mouseCatcher;
    private readonly InputAction_Trigger toggleChatTrigger;
    private ChatHUDController chatHudController;
    private string channelId;
    private bool skipChatInputTrigger;

    private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;

    public PublicChatWindowController(IChatController chatController,
        IUserProfileBridge userProfileBridge,
        DataStore dataStore,
        IProfanityFilter profanityFilter,
        IMouseCatcher mouseCatcher,
        InputAction_Trigger toggleChatTrigger)
    {
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
        this.mouseCatcher = mouseCatcher;
        this.toggleChatTrigger = toggleChatTrigger;
    }

    public void Initialize(IPublicChatWindowView view = null)
    {
        view ??= PublicChatWindowComponentView.Create();
        View = view;
        view.OnClose += HandleViewClosed;
        view.OnBack += HandleViewBacked;
        view.OnMuteChanged += MuteChannel;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += Hide;

        chatHudController = new ChatHUDController(dataStore,
            userProfileBridge,
            true,
            profanityFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnSendMessage += SendChatMessage;
        chatHudController.OnMessageSentBlockedBySpam += HandleMessageBlockedBySpam;

        chatController.OnAddMessage -= HandleMessageReceived;
        chatController.OnAddMessage += HandleMessageReceived;
        chatController.OnChannelUpdated -= HandleChannelUpdated;
        chatController.OnChannelUpdated += HandleChannelUpdated;

        toggleChatTrigger.OnTriggered += HandleChatInputTriggered;
    }
    
    public void Setup(string channelId)
    {
        if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;
        this.channelId = channelId;

        var channel = chatController.GetAllocatedChannel(channelId);
        View.Configure(ToPublicChatModel(channel));

        ReloadAllChats().Forget();
    }

    public void Dispose()
    {
        View.OnClose -= HandleViewClosed;
        View.OnBack -= HandleViewBacked;
        View.OnMuteChanged -= MuteChannel;

        if (chatController != null)
        {
            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnChannelUpdated -= HandleChannelUpdated;
        }

        chatHudController.OnSendMessage -= SendChatMessage;
        chatHudController.OnMessageSentBlockedBySpam -= HandleMessageBlockedBySpam;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= Hide;
        
        toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

        if (View != null)
        {
            View.Dispose();
        }
    }

    public void SetVisibility(bool visible, bool focusInputField)
    {
        SetVisiblePanelList(visible);
        if (visible)
        {
            View.Show();
            MarkChannelMessagesAsRead();
            
            if (focusInputField)
                chatHudController.FocusInputField();
        }
        else
        {   
            chatHudController.UnfocusInputField();
            View.Hide();
        }
    }

    private void SendChatMessage(ChatMessage message)
    {
        var isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
        var isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;

        if (isValidMessage)
        {
            chatHudController.ResetInputField();
            chatHudController.FocusInputField();
        }
        else
        {
            HandleViewClosed();
            SetVisibility(false);
            return;
        }

        if (isPrivateMessage)
            message.body = $"/w {message.recipient} {message.body}";

        chatController.Send(message);
    }

    public void SetVisibility(bool visible) => SetVisibility(visible, false);

    private void SetVisiblePanelList(bool visible)
    {
        HashSet<string> newSet = visibleTaskbarPanels.Get();
        if (visible)
            newSet.Add("PublicChatChannel");
        else 
            newSet.Remove("PublicChatChannel");

        visibleTaskbarPanels.Set(newSet, true);
    }

    private async UniTaskVoid ReloadAllChats()
    {
        chatHudController.ClearAllEntries();

        const int entriesPerFrame = 10;
        // TODO: filter entries by channelId
        var list = chatController.GetAllocatedEntries();
        if (list.Count == 0) return;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var message = list[i];
            if (i % entriesPerFrame == 0) await UniTask.NextFrame();
            HandleMessageReceived(message);
        }
    }

    internal void MarkChannelMessagesAsRead() => chatController.MarkChannelMessagesAsSeen(channelId);

    private void HandleViewClosed()
    {
        OnClosed?.Invoke();
    }

    private void HandleViewBacked() 
    {
        OnBack?.Invoke(); 
    }

    private void HandleMessageReceived(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PUBLIC
            && message.messageType != ChatMessage.Type.SYSTEM) return;
        if (!string.IsNullOrEmpty(message.recipient)) return;

        chatHudController.AddChatMessage(message, View.IsActive);

        if (View.IsActive)
            MarkChannelMessagesAsRead();
    }

    private void Hide() => SetVisibility(false);

    private void HandleChatInputTriggered(DCLAction_Trigger action)
    {
        if (!View.IsActive) return;
        chatHudController.FocusInputField();
    }
    
    private void HandleMessageBlockedBySpam(ChatMessage message)
    {
        chatHudController.AddChatMessage(new ChatEntryModel
        {
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            bodyText = "You sent too many messages in a short period of time. Please wait and try again later.",
            messageId = Guid.NewGuid().ToString(),
            messageType = ChatMessage.Type.SYSTEM,
            subType = ChatEntryModel.SubType.RECEIVED
        });
    }
    
    private void MuteChannel(bool muted)
    {
        if (muted)
            chatController.MuteChannel(channelId);
        else
            chatController.UnmuteChannel(channelId);
    }
    
    private PublicChatModel ToPublicChatModel(Channel channel)
    {
        return new PublicChatModel(channel.ChannelId,
            channel.Name,
            channel.Description,
            channel.Joined,
            channel.MemberCount,
            channel.Muted);
    }
    
    private void HandleChannelUpdated(Channel updatedChannel)
    {
        if (updatedChannel.ChannelId != channelId) return;
        View.Configure(ToPublicChatModel(updatedChannel));
    }
}