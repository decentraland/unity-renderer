using Cysharp.Threading.Tasks;
using DCL;
using DCL.Chat;
using DCL.Chat.HUD;
using DCL.Interface;
using DCL.Social.Friends;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskbarHUDController : IHUD
{
    private const string INTERCOM_URL = "https://intercom.decentraland.org/";

    private readonly IChatController chatController;
    private readonly IFriendsController friendsController;

    [Serializable]
    public struct Configuration
    {
        public bool enableVoiceChat;
        public bool enableQuestPanel;
    }

    public TaskbarHUDView view;
    public WorldChatWindowController worldChatWindowHud;
    public PrivateChatWindowController privateChatWindow;
    public PublicChatWindowController publicChatWindow;
    public ChatChannelHUDController channelChatWindow;
    public FriendsHUDController friendsHud;
    public VoiceChatWindowController voiceChatHud;

    private IMouseCatcher mouseCatcher;
    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;
    private Transform experiencesViewerTransform;
    private Transform notificationViewerTransform;
    private Transform topNotificationViewerTransform;
    private IHUD chatBackWindow;
    private SearchChannelsWindowController searchChannelsHud;
    private CreateChannelWindowController channelCreationWindow;
    private LeaveChannelConfirmationWindowController channelLeaveWindow;
    private CancellationTokenSource openPrivateChatCancellationToken = new ();

    private bool isFriendsFeatureEnabled => DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("friends_enabled");

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference => view.socialTooltipReference;

    internal BaseVariable<bool> isEmotesWheelInitialized => DataStore.i.emotesCustomization.isWheelInitialized;
    internal BaseVariable<bool> isEmotesVisible => DataStore.i.HUDs.emotesVisible;
    internal BaseVariable<bool> emoteJustTriggeredFromShortcut => DataStore.i.HUDs.emoteJustTriggeredFromShortcut;
    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<Transform> notificationPanelTransform => DataStore.i.HUDs.notificationPanelTransform;
    internal BaseVariable<Transform> topNotificationPanelTransform => DataStore.i.HUDs.topNotificationPanelTransform;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;
    internal BaseVariable<string> openChat => DataStore.i.HUDs.openChat;
    internal BaseVariable<bool> isPromoteChannelsToastVisible => DataStore.i.channels.isPromoteToastVisible;

    public TaskbarHUDController(IChatController chatController, IFriendsController friendsController)
    {
        this.chatController = chatController;
        this.friendsController = friendsController;
    }

    protected virtual TaskbarHUDView CreateView()
    {
        return TaskbarHUDView.Create();
    }

    public void Initialize(IMouseCatcher mouseCatcher)
    {
        this.mouseCatcher = mouseCatcher;

        view = CreateView();

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.leftWindowContainerLayout.enabled = false;

        view.OnChatToggle += HandleChatToggle;
        view.OnFriendsToggle += HandleFriendsToggle;
        view.OnEmotesToggle += HandleEmotesToggle;
        view.OnExperiencesToggle += HandleExperiencesToggle;
        view.OnVoiceChatToggle += HandleVoiceChatToggle;
        view.OnIntercomPressed += OpenIntercom;

        toggleFriendsTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;
        toggleFriendsTrigger.OnTriggered += ToggleFriendsTrigger_OnTriggered;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;
        closeWindowTrigger.OnTriggered += CloseWindowTrigger_OnTriggered;

        toggleWorldChatTrigger = Resources.Load<InputAction_Trigger>("ToggleWorldChat");
        toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;
        toggleWorldChatTrigger.OnTriggered += ToggleWorldChatTrigger_OnTriggered;

        isEmotesWheelInitialized.OnChange += InitializeEmotesSelector;
        InitializeEmotesSelector(isEmotesWheelInitialized.Get(), false);
        isEmotesVisible.OnChange += IsEmotesVisibleChanged;

        isExperiencesViewerOpen.OnChange += IsExperiencesViewerOpenChanged;

        view.leftWindowContainerAnimator.Show();

        CommonScriptableObjects.isTaskbarHUDInitialized.Set(true);

        isExperiencesViewerInitialized.OnChange += InitializeExperiencesViewer;
        InitializeExperiencesViewer(isExperiencesViewerInitialized.Get(), null);

        notificationPanelTransform.OnChange += InitializeNotificationPanel;
        InitializeNotificationPanel(notificationPanelTransform.Get(), null);

        topNotificationPanelTransform.OnChange += InitializeTopNotificationPanel;
        InitializeTopNotificationPanel(topNotificationPanelTransform.Get(), null);

        numOfLoadedExperiences.OnChange += NumOfLoadedExperiencesChanged;
        NumOfLoadedExperiencesChanged(numOfLoadedExperiences.Get(), 0);

        openChat.OnChange += OpenChat;
    }

    private void OpenIntercom() =>
        WebInterface.OpenURL(INTERCOM_URL);

    private void HandleFriendsToggle(bool show)
    {
        if (show)
            OpenFriendsWindow();
        else
        {
            friendsHud?.SetVisibility(false);
            ToggleOffChatIcon();
        }

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void HandleEmotesToggle(bool show)
    {
        if (show && emoteJustTriggeredFromShortcut.Get())
        {
            emoteJustTriggeredFromShortcut.Set(false);
            return;
        }

        if (show)
        {
            ToggleOffChatIcon();
            ShowEmotes();
        }
        else
        {
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Emotes);
            isEmotesVisible.Set(false);
            ToggleOffChatIcon();
        }

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowEmotes()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        voiceChatHud?.SetVisibility(false);
        isEmotesVisible.Set(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Emotes);
    }

    private void HandleExperiencesToggle(bool show)
    {
        if (show)
            ShowExperiences();
        else
        {
            isExperiencesViewerOpen.Set(false);
            ToggleOffChatIcon();
        }

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void HandleVoiceChatToggle(bool show)
    {
        if (show)
            OpenVoiceChatWindow();
        else
            voiceChatHud?.SetVisibility(false);

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowExperiences()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(true);
        isPromoteChannelsToastVisible.Set(false);
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (friendsHud == null) return;
        if (!isFriendsFeatureEnabled) return;

        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() !=
                                       null;

        if (anyInputFieldIsSelected) return;

        mouseCatcher.UnlockCursor();

        if (!friendsHud.View.IsActive())
        {
            view.leftWindowContainerAnimator.Show();
            OpenFriendsWindow();
        }
        else
        {
            CloseFriendsWindow();
            ToggleOffChatIcon();
        }
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action)
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() !=
                                       null;

        if (anyInputFieldIsSelected) return;

        mouseCatcher.UnlockCursor();
        chatBackWindow = worldChatWindowHud;

        if (!worldChatWindowHud.View.IsActive
            && !privateChatWindow.View.IsActive
            && !publicChatWindow.View.IsActive)
            OpenLastActiveChatWindow();
    }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (mouseCatcher.IsLocked) return;

        if (publicChatWindow.View.IsActive ||
            channelChatWindow.View.IsActive ||
            privateChatWindow.View.IsActive)
        {
            if (!DataStore.i.mentions.isMentionSuggestionVisible.Get())
            {
                publicChatWindow.SetVisibility(false);
                privateChatWindow.SetVisibility(false);
                channelChatWindow.SetVisibility(false);
                OpenChatList();
            }
        }
        else
        {
            publicChatWindow.SetVisibility(false);
            privateChatWindow.SetVisibility(false);
            channelChatWindow.SetVisibility(false);
            worldChatWindowHud.SetVisibility(false);
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        }

        CloseFriendsWindow();
        CloseVoiceChatWindow();
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);
    }

    private void HandleChatToggle(bool show)
    {
        if (show)
        {
            chatBackWindow = worldChatWindowHud;
            OpenChatList();
        }
        else { CloseAnyChatWindow(); }

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void MouseCatcher_OnMouseUnlock() { }

    private void MouseCatcher_OnMouseLock()
    {
        CloseFriendsWindow();
        CloseChatList();
        CloseVoiceChatWindow();
        isExperiencesViewerOpen.Set(false);

        if (!privateChatWindow.View.IsActive
            && !publicChatWindow.View.IsActive
            && !channelChatWindow.View.IsActive)
            ToggleOffChatIcon();
    }

    public void AddWorldChatWindow(WorldChatWindowController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer) return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        notificationViewerTransform?.SetAsLastSibling();
        topNotificationViewerTransform?.SetAsFirstSibling();
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.ShowChatButton();
        worldChatWindowHud.OnCloseView += ToggleOffChatIcon;
        worldChatWindowHud.OnOpenChannelCreation += OpenChannelCreation;
        worldChatWindowHud.OnOpenChannelLeave += OpenChannelLeaveConfirmation;
    }

    private void ToggleOffChatIcon() =>
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);

    private void OpenFriendsWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow?.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        friendsHud?.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Friends);
        chatBackWindow = friendsHud;
        isPromoteChannelsToastVisible.Set(false);
    }


    private void CloseFriendsWindow()
    {
        if(friendsHud is { IsVisible: true })
        {
            friendsHud?.SetVisibility(false);
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
        }
    }

    public void OpenPrivateChat(string userId)
    {
        worldChatWindowHud.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        openChat.Set(userId, false);
        privateChatWindow.Setup(userId);
        privateChatWindow.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void OpenLastActiveChatWindow()
    {
        string lastOpenedChat = openChat.Get();

        string newChat = string.IsNullOrEmpty(lastOpenedChat)
            ? ChatUtils.NEARBY_CHANNEL_ID
            : lastOpenedChat;

        if (newChat == ChatUtils.CONVERSATION_LIST_ID)
            newChat = ChatUtils.NEARBY_CHANNEL_ID;

        openChat.Set(newChat, true);
    }

    private void CloseAnyChatWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    public void OpenChannelChat(string channelId)
    {
        openChat.Set(channelId, false);
        channelChatWindow?.Setup(channelId);
        channelChatWindow?.SetVisibility(true);
        publicChatWindow?.SetVisibility(false);
        worldChatWindowHud?.SetVisibility(false);
        privateChatWindow?.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen?.Set(false);
        isEmotesVisible?.Set(false);
        voiceChatHud?.SetVisibility(false);

        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    public void OpenPublicChat(string channelId, bool focusInputField)
    {
        openChat.Set(channelId, false);
        publicChatWindow?.Setup(channelId);
        publicChatWindow?.SetVisibility(true, focusInputField);
        channelChatWindow?.SetVisibility(false);
        worldChatWindowHud?.SetVisibility(false);
        privateChatWindow?.SetVisibility(false);
        searchChannelsHud?.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen?.Set(false);
        isEmotesVisible?.Set(false);
        voiceChatHud?.SetVisibility(false);

        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void OpenChatList()
    {
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        openChat.Set(ChatUtils.CONVERSATION_LIST_ID, false);
        worldChatWindowHud.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void CloseChatList()
    {
        if (!worldChatWindowHud.View.IsActive) return;
        worldChatWindowHud.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void OpenVoiceChatWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        friendsHud?.SetVisibility(false);
        voiceChatHud?.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.VoiceChat);
        isPromoteChannelsToastVisible.Set(false);
    }

    private void CloseVoiceChatWindow()
    {
        if(voiceChatHud?.IsVisible ?? false)
        {
            voiceChatHud?.SetVisibility(false);
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.VoiceChat);
        }
    }

    public void AddPrivateChatWindow(PrivateChatWindowController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer)
            return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        notificationViewerTransform?.SetAsLastSibling();
        topNotificationViewerTransform?.SetAsFirstSibling();
        experiencesViewerTransform?.SetAsLastSibling();

        privateChatWindow = controller;

        controller.OnClosed += ToggleOffChatIcon;
    }

    public void AddPublicChatChannel(PublicChatWindowController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddPublicChatChannel >>> Public Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer) return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        notificationViewerTransform?.SetAsLastSibling();
        topNotificationViewerTransform?.SetAsFirstSibling();
        experiencesViewerTransform?.SetAsLastSibling();

        publicChatWindow = controller;

        controller.OnClosed += ToggleOffChatIcon;
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer)
            return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        notificationViewerTransform?.SetAsLastSibling();
        topNotificationViewerTransform?.SetAsFirstSibling();
        experiencesViewerTransform?.SetAsLastSibling();

        friendsHud = controller;

        if (isFriendsFeatureEnabled)
            view.ShowFriendsButton();
        else
            view.HideFriendsButton();

        friendsHud.OnViewClosed += () =>
        {
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
            ToggleOffChatIcon();
        };
    }

    public void AddVoiceChatWindow(VoiceChatWindowController controller)
    {
        if (controller?.VoiceChatWindowView == null)
        {
            Debug.LogWarning("AddVoiceChatWindow >>> Voice Chat window doesn't exist yet!");
            return;
        }

        if (controller.VoiceChatWindowView.Transform.parent == view.leftWindowContainer)
            return;

        controller.VoiceChatWindowView.Transform.SetParent(view.leftWindowContainer, false);

        voiceChatHud = controller;
        view.ShowVoiceChatButton();
        voiceChatHud.OnCloseView += () => view.ToggleOff(TaskbarHUDView.TaskbarButtonType.VoiceChat);

        if (controller?.VoiceChatBarView != null)
        {
            controller.VoiceChatBarView.Transform.SetParent(view.altSectionContainer, false);
            controller.VoiceChatBarView.Transform.SetAsFirstSibling();
        }
    }

    private void InitializeEmotesSelector(bool current, bool previous)
    {
        if (!current) return;
        view.ShowEmotesButton();
    }

    private void IsEmotesVisibleChanged(bool current, bool previous) =>
        HandleEmotesToggle(current);

    private void InitializeExperiencesViewer(Transform currentViewTransform, Transform previousViewTransform)
    {
        if (currentViewTransform == null)
            return;

        experiencesViewerTransform = currentViewTransform;
        experiencesViewerTransform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform.SetAsLastSibling();

        view.ShowExperiencesButton();
    }

    private void InitializeNotificationPanel(Transform currentPanelTransform, Transform previousPanelTransform)
    {
        if (currentPanelTransform == null)
            return;

        notificationViewerTransform = currentPanelTransform;
        notificationViewerTransform.SetParent(view.leftWindowContainer, false);
        notificationViewerTransform.SetAsLastSibling();
        experiencesViewerTransform.SetAsLastSibling();
    }

    private void InitializeTopNotificationPanel(Transform currentPanelTransform, Transform previousPanelTransform)
    {
        if (currentPanelTransform == null)
            return;

        topNotificationViewerTransform = currentPanelTransform;
        topNotificationViewerTransform.SetParent(view.leftWindowContainer, false);
        topNotificationViewerTransform.SetAsFirstSibling();
    }

    private void OpenChat(string chatId, string previous)
    {
        if (chatId == ChatUtils.NEARBY_CHANNEL_ID)
            OpenPublicChat(ChatUtils.NEARBY_CHANNEL_ID, true);
        else if (chatController.GetAllocatedChannel(chatId) != null)
        {
            if (chatController.GetAllocatedChannel(chatId).Joined)
                OpenChannelChat(chatId);
            else
                return;
        }
        else if (chatId == ChatUtils.CONVERSATION_LIST_ID)
            OpenChatList();
        else
        {
            async UniTaskVoid OpenPrivateChatAsync(string chatId, CancellationToken cancellationToken = default)
            {
                var friendshipStatus = await friendsController.GetFriendshipStatus(chatId, cancellationToken);

                if (friendshipStatus == FriendshipStatus.FRIEND)
                    OpenPrivateChat(chatId);
            }

            openPrivateChatCancellationToken.Cancel();
            openPrivateChatCancellationToken.Dispose();
            openPrivateChatCancellationToken = new CancellationTokenSource();
            OpenPrivateChatAsync(chatId, openPrivateChatCancellationToken.Token).Forget();
        }
    }

    private void IsExperiencesViewerOpenChanged(bool current, bool previous)
    {
        if (current)
            return;

        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Experiences);
        ToggleOffChatIcon();
    }

    private void NumOfLoadedExperiencesChanged(int current, int previous)
    {
        view.SetExperiencesVisibility(current > 0);

        if (current == 0)
            isExperiencesViewerOpen.Set(false);
    }

    public void DisableFriendsWindow()
    {
        view.friendsButton.transform.parent.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.OnChatToggle -= HandleChatToggle;
            view.OnFriendsToggle -= HandleFriendsToggle;
            view.OnEmotesToggle -= HandleEmotesToggle;
            view.OnExperiencesToggle -= HandleExperiencesToggle;
            view.OnVoiceChatToggle -= HandleVoiceChatToggle;

            view.Destroy();
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        }

        if (toggleFriendsTrigger != null)
            toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;

        if (closeWindowTrigger != null)
            closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;

        if (toggleWorldChatTrigger != null)
            toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;

        isEmotesWheelInitialized.OnChange -= InitializeEmotesSelector;
        isEmotesVisible.OnChange -= IsEmotesVisibleChanged;
        isExperiencesViewerOpen.OnChange -= IsExperiencesViewerOpenChanged;
        isExperiencesViewerInitialized.OnChange -= InitializeExperiencesViewer;
        numOfLoadedExperiences.OnChange -= NumOfLoadedExperiencesChanged;

        openPrivateChatCancellationToken.Cancel();
        openPrivateChatCancellationToken.Dispose();
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void GoBackFromChat()
    {
        if (chatBackWindow == friendsHud)
            OpenFriendsWindow();
        else
            OpenChatList();
    }

    public void AddChatChannel(ChatChannelHUDController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddChatChannel >>> Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer) return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        channelChatWindow = controller;

        controller.OnClosed += ToggleOffChatIcon;
        controller.OnOpenChannelLeave += OpenChannelLeaveConfirmation;
    }

    public void AddChannelSearch(SearchChannelsWindowController controller)
    {
        if (controller.View.Transform.parent == view.leftWindowContainer) return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        searchChannelsHud = controller;

        controller.OnClosed += () =>
        {
            controller.SetVisibility(false);
            ToggleOffChatIcon();
        };

        controller.OnBack += GoBackFromChat;
        controller.OnOpenChannelCreation += OpenChannelCreation;
        controller.OnOpenChannelLeave += OpenChannelLeaveConfirmation;
    }

    public void AddChannelCreation(CreateChannelWindowController controller)
    {
        if (controller.View.Transform.parent == view.fullScreenWindowContainer) return;

        controller.View.Transform.SetParent(view.fullScreenWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        channelCreationWindow = controller;

        controller.OnNavigateToChannelWindow += channelId =>
        {
            OpenChannelChat(channelId);
            channelCreationWindow.SetVisibility(false);
        };
    }

    private void OpenChannelCreation()
    {
        channelCreationWindow.SetVisibility(true);
    }

    public void AddChannelLeaveConfirmation(LeaveChannelConfirmationWindowController controller)
    {
        if (controller.View.Transform.parent == view.fullScreenWindowContainer) return;

        controller.View.Transform.SetParent(view.fullScreenWindowContainer, false);
        channelLeaveWindow = controller;
    }

    private void OpenChannelLeaveConfirmation(string channelId)
    {
        channelLeaveWindow.SetChannelToLeave(channelId);
        channelLeaveWindow.SetVisibility(true);
    }

    public void OpenChannelSearch()
    {
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(true);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        worldChatWindowHud.SetVisibility(false);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }
}
