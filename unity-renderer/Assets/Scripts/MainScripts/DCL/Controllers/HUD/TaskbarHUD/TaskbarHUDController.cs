using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskbarHUDController : IHUD
{
    public TaskbarHUDView view;
    public WorldChatWindowHUDController worldChatWindowHud;
    public PrivateChatWindowHUDController privateChatWindowHud;
    public FriendsHUDController friendsHud;

    IMouseCatcher mouseCatcher;
    IChatController chatController;

    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;

    public event System.Action OnAnyTaskbarButtonClicked;

    public void Initialize(IMouseCatcher mouseCatcher, IChatController chatController,
        IFriendsController friendsController)
    {
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        view = TaskbarHUDView.Create(this, chatController, friendsController);

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.chatHeadsGroup.OnHeadToggleOn += ChatHeadsGroup_OnHeadOpen;
        view.chatHeadsGroup.OnHeadToggleOff += ChatHeadsGroup_OnHeadClose;

        view.windowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;

        toggleFriendsTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;
        toggleFriendsTrigger.OnTriggered += ToggleFriendsTrigger_OnTriggered;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;
        closeWindowTrigger.OnTriggered += CloseWindowTrigger_OnTriggered;

        toggleWorldChatTrigger = Resources.Load<InputAction_Trigger>("ToggleWorldChat");
        toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;
        toggleWorldChatTrigger.OnTriggered += ToggleWorldChatTrigger_OnTriggered;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        view.windowContainerAnimator.Show();
    }

    private void ChatHeadsGroup_OnHeadClose(TaskbarButton obj)
    {
        privateChatWindowHud.SetVisibility(false);
    }

    private void View_OnFriendsToggleOn()
    {
        friendsHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnFriendsToggleOff()
    {
        friendsHud.SetVisibility(false);
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (!view.friendsButton.gameObject.activeSelf) return;

        OnFriendsToggleInputPress();
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action)
    {
        OnWorldChatToggleInputPress();
    }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action)
    {
        OnCloseWindowToggleInputPress();
    }

    private void View_OnChatToggleOn()
    {
        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.MarkWorldChatMessagesAsRead();
        worldChatWindowHud.view.DeactivatePreview();
        worldChatWindowHud.OnPressReturn();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnChatToggleOff()
    {
        if (view.AllButtonsToggledOff())
        {
            worldChatWindowHud.SetVisibility(true);
            worldChatWindowHud.view.ActivatePreview();
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


    private void MouseCatcher_OnMouseUnlock()
    {
        view.windowContainerAnimator.Show();
    }

    private void MouseCatcher_OnMouseLock()
    {
        view.windowContainerAnimator.Hide();

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }

        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.view.ActivatePreview();

        if (!AnyWindowsDifferentThanChatIsOpen())
            worldChatWindowHud.MarkWorldChatMessagesAsRead();
    }

    public void AddWorldChatWindow(WorldChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.view.OnClose += () => { view.friendsButton.SetToggleState(false, false); };

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false);
    }

    public void OpenFriendsWindow()
    {
        view.friendsButton.SetToggleState(true);
    }

    public void OpenPrivateChatTo(string userId)
    {
        var button = view.chatHeadsGroup.AddChatHead(userId, (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        button.toggleButton.onClick.Invoke();
    }

    public void AddPrivateChatWindow(PrivateChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        privateChatWindowHud = controller;

        privateChatWindowHud.view.OnMinimize += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                    (x) => x is ChatHeadButton &&
                           (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
                btn.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };

        privateChatWindowHud.view.OnClose += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                    (x) => x is ChatHeadButton &&
                           (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
            {
                btn.SetToggleState(false, false);
                view.chatHeadsGroup.RemoveChatHead(btn);
            }

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        friendsHud = controller;
        view.OnAddFriendsWindow();
        friendsHud.view.OnClose += () =>
        {
            view.friendsButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };

        friendsHud.view.friendsList.OnDeleteConfirmation += (entry) =>
        {
            view.chatHeadsGroup.RemoveChatHead(entry.userId);
        };
    }

    public void DisableFriendsWindow()
    {
        view.friendsButton.gameObject.SetActive(false);
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
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

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

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false, false);
        worldChatWindowHud.view.chatHudView.ResetInputField();
        worldChatWindowHud.view.ActivatePreview();
    }

    private void OnFriendsToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null &&
            (!worldChatWindowHud.view.chatHudView.inputField.isFocused || !worldChatWindowHud.view.isInPreview);

        if (anyInputFieldIsSelected)
            return;

        Utils.UnlockCursor();
        view.windowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!AnyWindowsDifferentThanChatIsOpen() && message.messageType == ChatMessage.Type.PUBLIC)
            worldChatWindowHud.MarkWorldChatMessagesAsRead((long)message.timestamp);
    }

    private bool AnyWindowsDifferentThanChatIsOpen()
    {
        return (friendsHud != null && friendsHud.view.gameObject.activeSelf) ||
               (privateChatWindowHud != null && privateChatWindowHud.view.gameObject.activeSelf);
    }
}