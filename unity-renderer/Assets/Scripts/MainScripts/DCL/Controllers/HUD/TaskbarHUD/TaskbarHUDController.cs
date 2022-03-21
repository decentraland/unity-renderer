using DCL;
using DCL.Controllers;
using DCL.HelpAndSupportHUD;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public PrivateChatWindowHUDController privateChatWindowHud;
    public FriendsHUDController friendsHud;
    public HelpAndSupportHUDController helpAndSupportHud;

    IMouseCatcher mouseCatcher;
    protected IChatController chatController;
    protected IFriendsController friendsController;

    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;
    private Transform experiencesViewerTransform;

    public event System.Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference { get => view.socialTooltipReference; }

    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

    protected internal virtual TaskbarHUDView CreateView() { return TaskbarHUDView.Create(this, chatController, friendsController); }

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

        view.chatHeadsGroup.OnHeadToggleOn += ChatHeadsGroup_OnHeadOpen;
        view.chatHeadsGroup.OnHeadToggleOff += ChatHeadsGroup_OnHeadClose;

        view.leftWindowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;
        view.OnExperiencesToggleOff += View_OnExperiencesToggleOff;
        view.OnExperiencesToggleOn += View_OnExperiencesToggleOn;

        toggleFriendsTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;
        toggleFriendsTrigger.OnTriggered += ToggleFriendsTrigger_OnTriggered;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;
        closeWindowTrigger.OnTriggered += CloseWindowTrigger_OnTriggered;

        toggleWorldChatTrigger = Resources.Load<InputAction_Trigger>("ToggleWorldChat");
        toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;
        toggleWorldChatTrigger.OnTriggered += ToggleWorldChatTrigger_OnTriggered;

        isExperiencesViewerOpen.OnChange += IsExperiencesViewerOpenChanged;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        view.leftWindowContainerAnimator.Show();

        CommonScriptableObjects.isTaskbarHUDInitialized.Set(true);
        DataStore.i.builderInWorld.showTaskBar.OnChange += SetVisibility;

        isExperiencesViewerInitialized.OnChange += InitializeExperiencesViewer;
        InitializeExperiencesViewer(isExperiencesViewerInitialized.Get(), null);

        numOfLoadedExperiences.OnChange += NumOfLoadedExperiencesChanged;
        NumOfLoadedExperiencesChanged(numOfLoadedExperiences.Get(), 0);
    }

    private void ChatHeadsGroup_OnHeadClose(TaskbarButton obj) { privateChatWindowHud.SetVisibility(false); }

    private void View_OnFriendsToggleOn()
    {
        friendsHud?.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnFriendsToggleOff() { friendsHud?.SetVisibility(false); }

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
        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.MarkWorldChatMessagesAsRead();
        worldChatWindowHud.DeactivatePreview();
        worldChatWindowHud.OnPressReturn();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnChatToggleOff()
    {
        if (view.AllButtonsToggledOff())
        {
            worldChatWindowHud.SetVisibility(true);
            worldChatWindowHud.ActivatePreview();
        }
        else
        {
            worldChatWindowHud.SetVisibility(false);
        }
    }

    private void ChatHeadsGroup_OnHeadOpen(TaskbarButton taskbarBtn)
    {
        ChatHeadButton head = taskbarBtn as ChatHeadButton;

        if (taskbarBtn == null)
            return;

        OpenPrivateChatWindow(head.profile.userId);
    }

    private void MouseCatcher_OnMouseUnlock() { view.leftWindowContainerAnimator.Show(); }

    private void MouseCatcher_OnMouseLock()
    {
        view.leftWindowContainerAnimator.Hide();

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }

        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.ActivatePreview();

        MarkWorldChatAsReadIfOtherWindowIsOpen();
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

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.View.OnClose += () => { view.friendsButton.SetToggleState(false, false); };

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false);
    }

    public void OpenFriendsWindow() { view.friendsButton.SetToggleState(true); }

    public void OpenPrivateChatTo(string userId)
    {
        var button = view.chatHeadsGroup.AddChatHead(userId, (ulong) System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        button.toggleButton.onClick.Invoke();
    }

    public void AddPrivateChatWindow(PrivateChatWindowHUDController controller)
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

        privateChatWindowHud = controller;

        privateChatWindowHud.view.OnMinimize += () =>
        {
            ChatHeadButton btn = view.GetButtonList()
                                     .FirstOrDefault(
                                         (x) => x is ChatHeadButton &&
                                                (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
                btn.SetToggleState(false, false);

            MarkWorldChatAsReadIfOtherWindowIsOpen();
        };

        privateChatWindowHud.view.OnClose += () =>
        {
            ChatHeadButton btn = view.GetButtonList()
                                     .FirstOrDefault(
                                         (x) => x is ChatHeadButton &&
                                                (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
            {
                btn.SetToggleState(false, false);
                view.chatHeadsGroup.RemoveChatHead(btn);
            }

            MarkWorldChatAsReadIfOtherWindowIsOpen();
        };
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
            MarkWorldChatAsReadIfOtherWindowIsOpen();
        };

        friendsHud.view.OnDeleteConfirmation += (userIdToRemove) => { view.chatHeadsGroup.RemoveChatHead(userIdToRemove); };
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
        MarkWorldChatAsReadIfOtherWindowIsOpen();
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
        view.chatHeadsGroup.ClearChatHeads();
    }

    private void OpenPrivateChatWindow(string userId)
    {
        privateChatWindowHud.Configure(userId);
        privateChatWindowHud.SetVisibility(true);
        privateChatWindowHud.ForceFocus();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.chatHeadsGroup.OnHeadToggleOn -= ChatHeadsGroup_OnHeadOpen;
            view.chatHeadsGroup.OnHeadToggleOff -= ChatHeadsGroup_OnHeadClose;

            view.OnChatToggleOff -= View_OnChatToggleOff;
            view.OnChatToggleOn -= View_OnChatToggleOn;
            view.OnFriendsToggleOff -= View_OnFriendsToggleOff;
            view.OnFriendsToggleOn -= View_OnFriendsToggleOn;
            view.OnExperiencesToggleOff -= View_OnExperiencesToggleOff;
            view.OnExperiencesToggleOn -= View_OnExperiencesToggleOn;

            UnityEngine.Object.Destroy(view.gameObject);
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

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        DataStore.i.builderInWorld.showTaskBar.OnChange -= SetVisibility;
        isExperiencesViewerOpen.OnChange -= IsExperiencesViewerOpenChanged;
        isExperiencesViewerInitialized.OnChange -= InitializeExperiencesViewer;
        numOfLoadedExperiences.OnChange -= NumOfLoadedExperiencesChanged;
    }

    public void SetVisibility(bool visible, bool previus) { SetVisibility(visible); }

    public void SetVisibility(bool visible) { view.SetVisibility(visible); }

    public void OnWorldChatToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

        if (anyInputFieldIsSelected)
            return;

        worldChatWindowHud.OnPressReturn();

        if (AnyWindowsDifferentThanChatIsOpen())
        {
            foreach (var btn in view.GetButtonList())
            {
                btn.SetToggleState(btn == view.chatButton);
            }
        }
    }

    public void OnCloseWindowToggleInputPress()
    {
        if (mouseCatcher.isLocked)
            return;

        view.chatButton.SetToggleState(false, false);
        worldChatWindowHud.ResetInputField();
        worldChatWindowHud.ActivatePreview();
    }

    public void SetVoiceChatRecording(bool recording) { view?.voiceChatButton.SetOnRecording(recording); }

    public void SetVoiceChatEnabledByScene(bool enabled) { view?.voiceChatButton.SetEnabledByScene(enabled); }

    private void OnFriendsToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null &&
                                       (!worldChatWindowHud.IsInputFieldFocused || !worldChatWindowHud.IsPreview);

        if (anyInputFieldIsSelected)
            return;

        Utils.UnlockCursor();
        view.leftWindowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!AnyWindowsDifferentThanChatIsOpen() && message.messageType == ChatMessage.Type.PUBLIC)
            worldChatWindowHud.MarkWorldChatMessagesAsRead((long) message.timestamp);
    }

    private bool AnyWindowsDifferentThanChatIsOpen()
    {
        return (friendsHud != null && friendsHud.view.IsActive()) ||
               (privateChatWindowHud != null && privateChatWindowHud.view.IsActive);
    }

    private void MarkWorldChatAsReadIfOtherWindowIsOpen()
    {
        if (!AnyWindowsDifferentThanChatIsOpen())
            worldChatWindowHud.MarkWorldChatMessagesAsRead();
    }
}