using DCL;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ChannelChatWindowController : IHUD
{
    private const string PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES = "LastReadWorldChatMessages";

    public IChannelChatWindowView view;

    private ChatHUDController chatHudController;
    private IChatController chatController;
    private IMouseCatcher mouseCatcher;
    private int invalidSubmitLastFrame;

    internal string lastPrivateMessageReceivedSender = string.Empty;

    private UserProfile ownProfile => UserProfile.GetOwnUserProfile();

    public event UnityAction<string> OnPressPrivateMessage;
    public event System.Action OnOpen;

    public void Initialize(IChatController chatController,
        IMouseCatcher mouseCatcher,
        IChannelChatWindowView view = null)
    {
        view ??= ChannelChatWindowView.Create();
        this.view = view;
        view.OnClose += OnViewClosed;
        view.OnMessageUpdated += OnMessageUpdated;
        view.OnSendMessage += SendChatMessage;

        chatHudController = new ChatHUDController(DataStore.i, ProfanityFilterSharedInstances.regexFilter);
        chatHudController.Initialize(view.ChatHUD);
        chatHudController.OnPressPrivateMessage -= ChatHUDController_OnPressPrivateMessage;
        chatHudController.OnPressPrivateMessage += ChatHUDController_OnPressPrivateMessage;
        LoadLatestReadWorldChatMessagesStatus();

        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock += view.ActivatePreview;
        }
    }

    public void Dispose()
    {
        view.OnClose -= OnViewClosed;
        view.OnMessageUpdated -= OnMessageUpdated;

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        if (chatHudController != null)
            chatHudController.OnPressPrivateMessage -= ChatHUDController_OnPressPrivateMessage;

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= view.ActivatePreview;

        view.OnSendMessage -= SendChatMessage;

        view?.Dispose();
    }

    //NOTE(Brian): Send chat responsibilities must be on the chatHud containing window like this one, this way we ensure
    //             it can be reused by the private messaging windows down the road.
    public void SendChatMessage(ChatMessage message)
    {
        bool isValidMessage = !string.IsNullOrEmpty(message.body) && !string.IsNullOrWhiteSpace(message.body);
        bool isPrivateMessage = message.messageType == ChatMessage.Type.PRIVATE;

        if (!isValidMessage)
        {
            view.ResetInputField(true);

            if (!isPrivateMessage && !view.IsPreview)
            {
                view.ActivatePreview();
                SceneReferences.i.mouseCatcher.LockCursor();
                invalidSubmitLastFrame = Time.frameCount;
            }

            return;
        }

        view.ResetInputField();
        view.FocusInputField();

        if (isPrivateMessage)
        {
            message.body = $"/w {message.recipient} {message.body}";
        }

        WebInterface.SendChatMessage(message);
    }

    public void SetVisibility(bool visible)
    {
        if (!visible)
            view.Hide();
        else
            view.Show();
    }

    public bool OnPressReturn()
    {
        OnOpen?.Invoke();

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            return false;

        if ((Time.frameCount - invalidSubmitLastFrame) < 2)
            return false;

        ForceFocus();
        return true;
    }

    public void ForceFocus(string setInputText = null)
    {
        SetVisibility(true);
        view.FocusInputField();
        view.DeactivatePreview();
        SceneReferences.i?.mouseCatcher.UnlockCursor();

        if (!string.IsNullOrEmpty(setInputText))
            view.SetInputField(setInputText);
    }

    private void ChatHUDController_OnPressPrivateMessage(string friendUserId)
    {
        OnPressPrivateMessage?.Invoke(friendUserId);
    }

    private void OnViewClosed() => SetVisibility(false);

    private void OnMessageUpdated(string message)
    {
        if (!string.IsNullOrEmpty(lastPrivateMessageReceivedSender) && message == "/r ")
            view.SetInputField($"/w {lastPrivateMessageReceivedSender} ");
    }

    private bool IsOldPrivateMessage(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE)
            return false;

        double timestampAsSeconds = message.timestamp / 1000.0f;

        if (timestampAsSeconds < chatController.initTime)
            return true;

        return false;
    }

    private void OnAddMessage(ChatMessage message)
    {
        if (IsOldPrivateMessage(message))
            return;

        chatHudController.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message), view.IsPreview);

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageReceivedSender = UserProfileController.userProfilesCatalog.Get(message.sender).userName;
    }

    private void LoadLatestReadWorldChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(0);
        string storedLastReadWorldChatMessagesString =
            PlayerPrefsUtils.GetString(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES);
        CommonScriptableObjects.lastReadWorldChatMessages.Set(System.Convert.ToInt64(
            string.IsNullOrEmpty(storedLastReadWorldChatMessagesString)
                ? 0
                : System.Convert.ToInt64(storedLastReadWorldChatMessagesString)));
    }
}