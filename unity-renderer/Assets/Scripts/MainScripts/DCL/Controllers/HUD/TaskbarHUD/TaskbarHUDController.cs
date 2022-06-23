using System;
using DCL;
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
    public PublicChatChannelController publicChatChannel;
    public FriendsHUDController friendsHud;

    private IMouseCatcher mouseCatcher;
    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;
    private Transform experiencesViewerTransform;
    private IHUD chatToggleTargetWindow;
    private IHUD chatInputTargetWindow;
    private IHUD chatBackWindow;

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference => view.socialTooltipReference;

    internal BaseVariable<bool> isEmotesWheelInitialized => DataStore.i.emotesCustomization.isWheelInitialized;
    internal BaseVariable<bool> isEmotesVisible => DataStore.i.HUDs.emotesVisible;
    internal BaseVariable<bool> emoteJustTriggeredFromShortcut => DataStore.i.HUDs.emoteJustTriggeredFromShortcut;
    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

    protected virtual TaskbarHUDView CreateView() { return TaskbarHUDView.Create(); }

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
            OpenPublicChannelOnPreviewMode();
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
            OpenPublicChannelOnPreviewMode();
            ShowEmotes();
        }
        else
        {
            view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Emotes);
            isEmotesVisible.Set(false);
            OpenPublicChannelOnPreviewMode();
        }
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowEmotes()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
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
            OpenPublicChannelOnPreviewMode();
        }
            
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowExperiences()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(true);
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (friendsHud == null) return;

        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

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
            OpenPublicChannelOnPreviewMode();
        }
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action)
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

        if (anyInputFieldIsSelected) return;
        
        mouseCatcher.UnlockCursor();
        chatBackWindow = worldChatWindowHud;

        if (!worldChatWindowHud.View.IsActive
            && !privateChatWindow.View.IsActive
            && !publicChatChannel.View.IsActive)
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
        OpenPublicChannelOnPreviewMode();
    }

    private void HandleChatToggle(bool show)
    {
        if (show)
        {
            chatBackWindow = publicChatChannel;
            var openedWindow = OpenLastActiveChatWindow(chatToggleTargetWindow);
            if (openedWindow == publicChatChannel)
                publicChatChannel.DeactivatePreview();
            else if (openedWindow == privateChatWindow)
                privateChatWindow.DeactivatePreviewMode();
        }
        else
        {
            if (chatToggleTargetWindow == publicChatChannel)
                publicChatChannel.ActivatePreview();
            else if (chatToggleTargetWindow == privateChatWindow)
                privateChatWindow.ActivatePreviewMode();
            else
            {
                CloseAnyChatWindow();
                OpenPublicChannelOnPreviewMode();
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
        isExperiencesViewerOpen.Set(false);

        if (!privateChatWindow.View.IsActive
            && !publicChatChannel.View.IsActive)
            OpenPublicChannelOnPreviewMode();
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
        worldChatWindowHud.View.OnClose += OpenPublicChannelOnPreviewMode;
    }

    private void OpenPublicChannelOnPreviewMode()
    {
        chatToggleTargetWindow = publicChatChannel;
        publicChatChannel.SetVisibility(true, false);
        publicChatChannel.ActivatePreviewModeInstantly();
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void OpenFriendsWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
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
        publicChatChannel.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        privateChatWindow.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        chatToggleTargetWindow = privateChatWindow;
        chatInputTargetWindow = privateChatWindow;
    }

    public void ShowPrivateChatLoading(bool isActive) { privateChatWindow.SetLoadingMessagesActive(isActive); }
    
    private IHUD OpenLastActiveChatWindow(IHUD lastActiveWindow)
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);

        IHUD visibleWindow;

        if (lastActiveWindow == publicChatChannel)
        {
            publicChatChannel.SetVisibility(true, true);
            visibleWindow = lastActiveWindow;
        }
        else if (lastActiveWindow != null)
        {
            lastActiveWindow.SetVisibility(true);
            visibleWindow = lastActiveWindow;
        }
        else
        {
            publicChatChannel.SetVisibility(true, true);
            visibleWindow = publicChatChannel;
        }

        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);

        return visibleWindow;
    }

    private void CloseAnyChatWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    public void OpenPublicChatChannel(string channelId, bool focusInputField)
    {
        publicChatChannel?.Setup(channelId);
        worldChatWindowHud?.SetVisibility(false);
        privateChatWindow?.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen?.Set(false);
        isEmotesVisible?.Set(false);
        publicChatChannel?.SetVisibility(true, focusInputField);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        chatToggleTargetWindow = publicChatChannel;
        chatInputTargetWindow = publicChatChannel;
    }
    
    private void OpenChatList()
    {
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
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

        controller.OnClosed += OpenPublicChannelOnPreviewMode;
        controller.OnPreviewModeChanged += HandlePrivateChannelPreviewMode;
    }

    public void AddPublicChatChannel(PublicChatChannelController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddPublicChatChannel >>> Public Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer) return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();
        
        publicChatChannel = controller;

        controller.OnClosed += OpenPublicChannelOnPreviewMode;
        controller.OnPreviewModeChanged += HandlePublicChannelPreviewModeChanged;
    }

    private void HandlePublicChannelPreviewModeChanged(bool isPreviewMode)
    {
        if (!publicChatChannel.View.IsActive) return;
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
            OpenPublicChannelOnPreviewMode();
        };
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
        OpenPublicChannelOnPreviewMode();
    }

    private void NumOfLoadedExperiencesChanged(int current, int previous)
    {
        view.SetExperiencesVisibility(current > 0);

        if (current == 0)
            isExperiencesViewerOpen.Set(false);
    }

    public void OnAddVoiceChat() { view.ShowVoiceChat(); }

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

    private void SetVisibility(bool visible, bool previus) { SetVisibility(visible); }

    public void SetVisibility(bool visible) { view.SetVisibility(visible); }

    public void SetVoiceChatRecording(bool recording) { view?.voiceChatButton.SetOnRecording(recording); }

    public void SetVoiceChatEnabledByScene(bool enabled) { view?.voiceChatButton.SetEnabledByScene(enabled); }

    public void GoBackFromChat()
    {
        if (chatBackWindow == friendsHud)
            OpenFriendsWindow();
        else
            OpenChatList();
    }
}