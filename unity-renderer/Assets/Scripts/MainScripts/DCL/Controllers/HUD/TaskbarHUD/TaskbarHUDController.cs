using System;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

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
    private PublicChatChannelController publicChatChannel;
    public FriendsHUDController friendsHud;

    IMouseCatcher mouseCatcher;
    protected IChatController chatController;
    protected IFriendsController friendsController;

    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;
    private Transform experiencesViewerTransform;

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference { get => view.socialTooltipReference; }

    internal BaseVariable<bool> isEmotesWheelInitialized => DataStore.i.emotesCustomization.isWheelInitialized;
    internal BaseVariable<bool> isEmotesVisible => DataStore.i.HUDs.emotesVisible;
    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

    protected internal virtual TaskbarHUDView CreateView() { return TaskbarHUDView.Create(this); }

    public void Initialize(
        IMouseCatcher mouseCatcher,
        IChatController chatController,
        IFriendsController friendsController)
    {
        this.friendsController = friendsController;
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        view = CreateView();

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.leftWindowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;
        view.OnEmotesToggleOff += View_OnEmotesToggleOff;
        view.OnEmotesToggleOn += View_OnEmotesToggleOn;
        view.OnExperiencesToggleOff += View_OnExperiencesToggleOff;
        view.OnExperiencesToggleOn += View_OnExperiencesToggleOn;
        view.OnFriendsInitializationRetry += RetryFriendsInitialization;

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

        if (friendsController != null)
        {
            if (friendsController.isInitialized)
            {
                view.SetFiendsAsLoading(false);
                view.SetFriendsAsFailed(false);
            }
            else
            {
                friendsController.OnInitialized -= FriendsController_OnInitialized;
                friendsController.OnInitialized += FriendsController_OnInitialized;
                if (friendsController.hasInitializationFailed)
                    ShowFriendsFailedPanel();
                else
                    view.SetFiendsAsLoading(true);
            }
        }
    }

    private void View_OnFriendsToggleOn()
    {
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        friendsHud?.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnFriendsToggleOff() { friendsHud?.SetVisibility(false); }

    private void View_OnEmotesToggleOn()
    {
        isEmotesVisible.Set(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnEmotesToggleOff() { isEmotesVisible.Set(false); }
    
    private void View_OnExperiencesToggleOn()
    {
        isExperiencesViewerOpen.Set(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnExperiencesToggleOff() { isExperiencesViewerOpen.Set(false); }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (!view.friendsButton.transform.parent.gameObject.activeSelf)
            return;

        OnFriendsToggleInputPress();
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action) { OnWorldChatToggleInputPress(); }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action) { OnCloseWindowToggleInputPress(); }

    private void View_OnChatToggleOn()
    {
        OpenChatList();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnChatToggleOff()
    {
        worldChatWindowHud.SetVisibility(false);
    }

    private void MouseCatcher_OnMouseUnlock() { view.leftWindowContainerAnimator.Show(); }

    private void MouseCatcher_OnMouseLock()
    {
        view.leftWindowContainerAnimator.Hide();

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }
    }

    public void AddWorldChatWindow(WorldChatWindowController controller)
    {
        if (controller == null || controller.View == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer)
            return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.View.OnClose += () => { view.friendsButton.SetToggleState(false, false); };
        view.chatButton.SetToggleState(false);
    }

    public void OpenFriendsWindow() { view.friendsButton.SetToggleState(true); }

    public void OpenPrivateChatTo(string userId)
    {
        privateChatWindow.Configure(userId);
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(true);
        friendsHud.SetVisibility(false);
    }

    public void OpenPublicChatChannel(string channelId)
    {
        worldChatWindowHud.SetVisibility(false);
        publicChatChannel.Setup(channelId);
        publicChatChannel.SetVisibility(true);
        friendsHud.SetVisibility(false);
    }
    
    public void OpenChatList()
    {
        worldChatWindowHud.SetVisibility(true);
        friendsHud.SetVisibility(false);
        publicChatChannel.SetVisibility(false);
        privateChatWindow.SetVisibility(false);
    }

    public void AddPrivateChatWindow(PrivateChatWindowController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer)
            return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        privateChatWindow = controller;
    }

    public void AddPublicChatChannel(PublicChatChannelController controller)
    {
        if (controller?.view == null)
        {
            Debug.LogWarning("AddPublicChatChannel >>> Public Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer) return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();
        
        publicChatChannel = controller;
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer)
            return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        friendsHud = controller;
        view.OnAddFriendsWindow();
        friendsHud.view.OnClose += () =>
        {
            view.friendsButton.SetToggleState(false, false);
        };
    }

    private void InitializeEmotesSelector(bool current, bool previous) 
    {
        if (!current)
            return;

        view.OnAddEmotesWindow(); 
    }

    private void IsEmotesVisibleChanged(bool current, bool previous)
    {
        if (current && !isEmotesVisible.Get())
            return;

        view.emotesButton.SetToggleState(current, false);
    }

    internal void InitializeExperiencesViewer(Transform currentViewTransform, Transform previousViewTransform)
    {
        if (currentViewTransform == null)
            return;

        experiencesViewerTransform = currentViewTransform;
        experiencesViewerTransform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform.SetAsLastSibling();

        view.OnAddExperiencesWindow();
    }

    private void IsExperiencesViewerOpenChanged(bool current, bool previous)
    {
        if (current)
            return;

        view.experiencesButton.SetToggleState(false, false);
    }

    private void NumOfLoadedExperiencesChanged(int current, int previous)
    {
        view.SetExperiencesVisbility(current > 0);

        if (current == 0)
            View_OnExperiencesToggleOff();
    }

    public void OnAddVoiceChat() { view.OnAddVoiceChat(); }

    public void DisableFriendsWindow()
    {
        view.friendsButton.transform.parent.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.OnChatToggleOff -= View_OnChatToggleOff;
            view.OnChatToggleOn -= View_OnChatToggleOn;
            view.OnFriendsToggleOff -= View_OnFriendsToggleOff;
            view.OnFriendsToggleOn -= View_OnFriendsToggleOn;
            view.OnEmotesToggleOff -= View_OnEmotesToggleOff;
            view.OnEmotesToggleOn -= View_OnEmotesToggleOn;
            view.OnExperiencesToggleOff -= View_OnExperiencesToggleOff;
            view.OnExperiencesToggleOn -= View_OnExperiencesToggleOn;
            view.OnFriendsInitializationRetry -= RetryFriendsInitialization;

            Object.Destroy(view.gameObject);
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

        if (friendsController != null)
            friendsController.OnInitialized -= FriendsController_OnInitialized;
    }

    public void SetVisibility(bool visible, bool previus) { SetVisibility(visible); }

    public void SetVisibility(bool visible) { view.SetVisibility(visible); }

    public void OnWorldChatToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

        if (anyInputFieldIsSelected) return;
        
        friendsHud.SetVisibility(false);
        worldChatWindowHud.OpenLastActiveChat();
    }

    public void OnCloseWindowToggleInputPress()
    {
        if (mouseCatcher.isLocked)
            return;

        view.chatButton.SetToggleState(false, false);
        publicChatChannel.ResetInputField();
    }

    public void SetVoiceChatRecording(bool recording) { view?.voiceChatButton.SetOnRecording(recording); }

    public void SetVoiceChatEnabledByScene(bool enabled) { view?.voiceChatButton.SetEnabledByScene(enabled); }

    private void OnFriendsToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null &&
                                       (!worldChatWindowHud.IsInputFieldFocused || !worldChatWindowHud.IsPreview);

        if (anyInputFieldIsSelected)
            return;

        Utils.UnlockCursor();
        view.leftWindowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }

    private void FriendsController_OnInitialized(bool isInitialized)
    {
        if (isInitialized)
        {
            friendsController.OnInitialized -= FriendsController_OnInitialized;
            view.SetFiendsAsLoading(false);
        }
        else
        {
            ShowFriendsFailedPanel();
        }
    }

    private void ShowFriendsFailedPanel()
    {
        view.SetFiendsAsLoading(false);
        view.SetFriendsAsFailed(true);
    }

    private void RetryFriendsInitialization()
    {
        view.SetFriendsAsFailed(false);
        view.SetFiendsAsLoading(true);
        WebInterface.RetryFriendsInitialization();
    }
}