using System;
using DCL;
using DCL.Chat.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskbarHUDController : IHUD
{
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
    private IHUD chatToggleTargetWindow;
    private IHUD chatInputTargetWindow;
    private IHUD chatBackWindow;
    private SearchChannelsWindowController searchChannelsHud;
    private CreateChannelWindowController channelCreationWindow;

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference => view.socialTooltipReference;

    internal BaseVariable<bool> isEmotesWheelInitialized => DataStore.i.emotesCustomization.isWheelInitialized;
    internal BaseVariable<bool> isEmotesVisible => DataStore.i.HUDs.emotesVisible;
    internal BaseVariable<bool> emoteJustTriggeredFromShortcut => DataStore.i.HUDs.emoteJustTriggeredFromShortcut;
    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

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
        DataStore.i.builderInWorld.showTaskBar.OnChange += SetVisibility;

        isExperiencesViewerInitialized.OnChange += InitializeExperiencesViewer;
        InitializeExperiencesViewer(isExperiencesViewerInitialized.Get(), null);

        numOfLoadedExperiences.OnChange += NumOfLoadedExperiencesChanged;
        NumOfLoadedExperiencesChanged(numOfLoadedExperiences.Get(), 0);
    }

    private void HandleFriendsToggle(bool show)
    {
        if (show)
            OpenFriendsWindow();
        else
        {
            friendsHud?.SetVisibility(false);
            OpenPublicChatOnPreviewMode();
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
            OpenPublicChatOnPreviewMode();
            ShowEmotes();
        }
        else
        {
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Emotes);
            isEmotesVisible.Set(false);
            OpenPublicChatOnPreviewMode();
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
            OpenPublicChatOnPreviewMode();
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
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (friendsHud == null) return;

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
            OpenPublicChatOnPreviewMode();
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
            OpenLastActiveChatWindow(chatInputTargetWindow);
    }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (mouseCatcher.isLocked) return;
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);
        voiceChatHud?.SetVisibility(false);
        OpenPublicChatOnPreviewMode();
    }

    private void HandleChatToggle(bool show)
    {
        if (show)
        {
            chatBackWindow = publicChatWindow;
            var openedWindow = OpenLastActiveChatWindow(chatToggleTargetWindow);
            if (openedWindow == publicChatWindow)
                publicChatWindow.DeactivatePreview();
            else if (openedWindow == privateChatWindow)
                privateChatWindow.DeactivatePreview();
            else if (openedWindow == channelChatWindow)
                channelChatWindow.DeactivatePreviewMode();
        }
        else
        {
            if (chatToggleTargetWindow == publicChatWindow)
                publicChatWindow.ActivatePreview();
            else if (chatToggleTargetWindow == privateChatWindow)
                privateChatWindow.ActivatePreview();
            else if (chatToggleTargetWindow == channelChatWindow)
                channelChatWindow.ActivatePreviewMode();
            else
            {
                CloseAnyChatWindow();
                OpenPublicChatOnPreviewMode();
            }
        }

        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void MouseCatcher_OnMouseUnlock()
    {
        // TODO: temporary deactivated current window fadein/fadeout until we get the full chat notifications feature implemented
        // view.leftWindowContainerAnimator.Show();
        // view.RestoreLastToggle();
    }

    private void MouseCatcher_OnMouseLock()
    {
        // TODO: temporary deactivated current window fadein/fadeout until we get the full chat notifications feature implemented
        // view.leftWindowContainerAnimator.Hide();
        // view.ToggleAllOff();
        CloseFriendsWindow();
        CloseChatList();
        CloseVoiceChatWindow();
        isExperiencesViewerOpen.Set(false);

        if (!privateChatWindow.View.IsActive
            && !publicChatWindow.View.IsActive)
            OpenPublicChatOnPreviewMode();
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
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.ShowChatButton();
        worldChatWindowHud.View.OnClose += OpenPublicChatOnPreviewMode;
        worldChatWindowHud.OnOpenChannelCreation += OpenChannelCreation;
    }

    private void OpenPublicChatOnPreviewMode()
    {
        chatToggleTargetWindow = publicChatWindow;
        publicChatWindow.SetVisibility(true, false);
        publicChatWindow.ActivatePreviewModeInstantly();
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

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
    }

    private void CloseFriendsWindow()
    {
        friendsHud?.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
    }

    public void OpenPrivateChat(string userId)
    {
        privateChatWindow.Setup(userId);
        worldChatWindowHud.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        voiceChatHud?.SetVisibility(false);
        privateChatWindow.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        chatToggleTargetWindow = privateChatWindow;
        chatInputTargetWindow = privateChatWindow;
    }

    private IHUD OpenLastActiveChatWindow(IHUD lastActiveWindow)
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatWindow.SetVisibility(false);
        channelChatWindow?.SetVisibility(false);
        searchChannelsHud.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);
        voiceChatHud?.SetVisibility(false);

        IHUD visibleWindow;

        if (lastActiveWindow == publicChatWindow)
        {
            publicChatWindow.SetVisibility(true, true);
            visibleWindow = lastActiveWindow;
        }
        else if (lastActiveWindow != null)
        {
            lastActiveWindow.SetVisibility(true);
            visibleWindow = lastActiveWindow;
        }
        else
        {
            publicChatWindow.SetVisibility(true, true);
            visibleWindow = publicChatWindow;
        }

        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);

        return visibleWindow;
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
        
        chatToggleTargetWindow = channelChatWindow;
        chatInputTargetWindow = channelChatWindow;
    }

    public void OpenPublicChat(string channelId, bool focusInputField)
    {
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
        
        chatToggleTargetWindow = publicChatWindow;
        chatInputTargetWindow = publicChatWindow;
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
        worldChatWindowHud.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        chatToggleTargetWindow = worldChatWindowHud;
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
    }

    private void CloseVoiceChatWindow()
    {
        voiceChatHud?.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.VoiceChat);
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
        experiencesViewerTransform?.SetAsLastSibling();

        privateChatWindow = controller;

        controller.OnClosed += OpenPublicChatOnPreviewMode;
        controller.OnPreviewModeChanged += HandlePrivateChannelPreviewMode;
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
        experiencesViewerTransform?.SetAsLastSibling();

        publicChatWindow = controller;

        controller.OnClosed += OpenPublicChatOnPreviewMode;
        controller.OnPreviewModeChanged += HandlePublicChannelPreviewModeChanged;
    }

    private void HandlePublicChannelPreviewModeChanged(bool isPreviewMode)
    {
        if (!publicChatWindow.View.IsActive) return;
        if (isPreviewMode)
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        else
            view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void HandleChannelPreviewModeChanged(bool isPreviewMode)
    {
        if (!channelChatWindow.View.IsActive) return;
        if (isPreviewMode)
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        else
            view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void HandlePrivateChannelPreviewMode(bool isPreviewMode)
    {
        if (!privateChatWindow.View.IsActive) return;
        if (isPreviewMode)
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        else
            view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
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
        experiencesViewerTransform?.SetAsLastSibling();

        friendsHud = controller;
        view.ShowFriendsButton();
        friendsHud.View.OnClose += () =>
        {
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
            OpenPublicChatOnPreviewMode();
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
        voiceChatHud.VoiceChatWindowView.OnClose += () => view.ToggleOff(TaskbarHUDView.TaskbarButtonType.VoiceChat);

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

    private void IsEmotesVisibleChanged(bool current, bool previous) => HandleEmotesToggle(current);

    private void InitializeExperiencesViewer(Transform currentViewTransform, Transform previousViewTransform)
    {
        if (currentViewTransform == null)
            return;

        experiencesViewerTransform = currentViewTransform;
        experiencesViewerTransform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform.SetAsLastSibling();

        view.ShowExperiencesButton();
    }

    private void IsExperiencesViewerOpenChanged(bool current, bool previous)
    {
        if (current)
            return;

        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Experiences);
        OpenPublicChatOnPreviewMode();
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

        DataStore.i.builderInWorld.showTaskBar.OnChange -= SetVisibility;
        isEmotesWheelInitialized.OnChange -= InitializeEmotesSelector;
        isEmotesVisible.OnChange -= IsEmotesVisibleChanged;
        isExperiencesViewerOpen.OnChange -= IsExperiencesViewerOpenChanged;
        isExperiencesViewerInitialized.OnChange -= InitializeExperiencesViewer;
        numOfLoadedExperiences.OnChange -= NumOfLoadedExperiencesChanged;
    }

    private void SetVisibility(bool visible, bool previus)
    {
        SetVisibility(visible);
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

        controller.OnClosed += OpenPublicChatOnPreviewMode;
        controller.OnPreviewModeChanged += HandleChannelPreviewModeChanged;
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
            OpenPublicChatOnPreviewMode();
        };
        controller.OnBack += GoBackFromChat;
        controller.OnOpenChannelCreation += OpenChannelCreation;
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
        chatToggleTargetWindow = worldChatWindowHud;
    }
}