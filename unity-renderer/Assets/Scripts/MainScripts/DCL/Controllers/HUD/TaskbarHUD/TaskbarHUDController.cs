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
    private IHUD lastChatActiveWindow;
    private IHUD chatBackWindow;

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference => view.socialTooltipReference;

    internal BaseVariable<bool> isEmotesWheelInitialized => DataStore.i.emotesCustomization.isWheelInitialized;
    internal BaseVariable<bool> isEmotesVisible => DataStore.i.HUDs.emotesVisible;
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
            friendsHud.SetVisibility(false);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void HandleEmotesToggle(bool show)
    {
        if (show)
            ShowEmotes();
        else
            isEmotesVisible.Set(false);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowEmotes()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Emotes);
    }

    private void HandleExperiencesToggle(bool show)
    {
        if (show)
            ShowExperiences();
        else
            isExperiencesViewerOpen.Set(false);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void ShowExperiences()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(true);
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        // ??????
        if (!view.friendsButton.transform.parent.gameObject.activeSelf) return;

        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

        if (anyInputFieldIsSelected) return;

        mouseCatcher.UnlockCursor();

        if (!friendsHud.view.IsActive())
        {
            view.leftWindowContainerAnimator.Show();
            OpenFriendsWindow();
        }
        else
            CloseFriendsWindow();
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
            OpenLastActiveChatWindow();
    }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (mouseCatcher.isLocked) return;
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);
        view.ToggleAllOff();
    }

    private void HandleChatToggle(bool show)
    {
        if (show)
        {
            chatBackWindow = publicChatChannel;
            OpenLastActiveChatWindow();
        }
        else
            CloseAnyChatWindow();
        
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
    }

    public void AddWorldChatWindow(WorldChatWindowController controller)
    {
        if (controller?.View == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer)
            return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.ShowChatButton();
        worldChatWindowHud.View.OnClose += () =>
        {
            if (!privateChatWindow.View.IsActive
                && !publicChatChannel.View.IsActive)
                view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        };
    }

    private void OpenFriendsWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        friendsHud.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Friends);
        chatBackWindow = friendsHud;
    }

    private void CloseFriendsWindow()
    {
        friendsHud.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
    }

    public void OpenPrivateChat(string userId)
    {
        privateChatWindow.Setup(userId);
        worldChatWindowHud.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        privateChatWindow.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        lastChatActiveWindow = privateChatWindow;
    }
    
    private void OpenLastActiveChatWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isEmotesVisible.Set(false);
        isExperiencesViewerOpen.Set(false);

        if (lastChatActiveWindow != null)
            lastChatActiveWindow.SetVisibility(true);
        else
            publicChatChannel.SetVisibility(true);
        
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    private void CloseAnyChatWindow()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
    }

    public void OpenPublicChatChannel(string channelId)
    {
        publicChatChannel?.Setup(channelId);
        worldChatWindowHud?.SetVisibility(false);
        privateChatWindow?.SetVisibility(false);
        friendsHud?.SetVisibility(false);
        isExperiencesViewerOpen?.Set(false);
        isEmotesVisible?.Set(false);
        publicChatChannel?.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
        lastChatActiveWindow = publicChatChannel;
    }
    
    private void OpenChatList()
    {
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud.SetVisibility(false);
        isExperiencesViewerOpen.Set(false);
        isEmotesVisible.Set(false);
        worldChatWindowHud.SetVisibility(true);
        view.ToggleOn(TaskbarHUDView.TaskbarButtonType.Chat);
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

        controller.OnClosed += () =>
        {
            if (!worldChatWindowHud.View.IsActive
                && !publicChatChannel.View.IsActive)
                view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        };
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

        controller.OnClosed += () =>
        {
            if (!worldChatWindowHud.View.IsActive
                && !privateChatWindow.View.IsActive)
                view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Chat);
        };
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller?.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer)
            return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        friendsHud = controller;
        view.ShowFriendsButton();
        friendsHud.view.OnClose += () => view.ToggleOff(TaskbarHUDView.TaskbarButtonType.Friends);
    }

    private void InitializeEmotesSelector(bool current, bool previous) 
    {
        if (!current) return;
        view.ShowEmotesButton(); 
    }

    private void IsEmotesVisibleChanged(bool current, bool previous)
    {
        if (current && !isEmotesVisible.Get())
            return;

        view.emotesButton.SetToggleState(current, false);
    }

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

        view.experiencesButton.SetToggleState(false, false);
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