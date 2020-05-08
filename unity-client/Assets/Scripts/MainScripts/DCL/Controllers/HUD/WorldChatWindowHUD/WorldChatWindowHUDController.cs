
using DCL;

using DCL.Interface;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldChatWindowHUDController : IHUD
{
    private ChatHUDController chatHudController;
    public WorldChatWindowHUDView view;

    private IChatController chatController;
    private IMouseCatcher mouseCatcher;

    internal bool resetInputFieldOnSubmit = true;
    private int invalidSubmitLastFrame = 0;

    public void Initialize(IChatController chatController, IMouseCatcher mouseCatcher)
    {
        view = WorldChatWindowHUDView.Create(OnEnablePrivateTab, OnEnableWorldTab);

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView, SendChatMessage);

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

        if (chatController != null)
        {
            view.worldFilterButton.onClick.Invoke();
        }

        view.chatHudView.ForceUpdateLayout();
    }
    public void Dispose()
    {
        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= view.ActivatePreview;
        }

        Object.Destroy(view);
    }

    void OnEnableWorldTab()
    {
        view.chatHudView.CleanAllEntries();

        var result = chatController.GetEntries();

        foreach (var v in result)
        {
            OnAddMessage(v);
        }
    }

    void OnEnablePrivateTab()
    {
        view.chatHudView.CleanAllEntries();
        var result = chatController.GetEntries().Where((x) => x.messageType == ChatMessage.Type.PRIVATE).ToList();

        foreach (var v in result)
        {
            OnAddMessage(v);
        }
    }

    void OnAddMessage(ChatMessage message)
    {
        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));
    }

    //NOTE(Brian): Send chat responsibilities must be on the chatHud containing window like this one, this way we ensure
    //             it can be reused by the private messaging windows down the road.
    public void SendChatMessage(string msgBody)
    {
        bool validString = !string.IsNullOrEmpty(msgBody);

        if (msgBody.Length == 1 && (byte)msgBody[0] == 11) //NOTE(Brian): Trim doesn't work. neither IsNullOrWhitespace.
            validString = false;

        if (!validString)
        {
            view.ActivatePreview();
            InitialSceneReferences.i.mouseCatcher.LockCursor();
            invalidSubmitLastFrame = Time.frameCount;
            return;
        }

        if (resetInputFieldOnSubmit)
        {
            view.chatHudView.ResetInputField();
            view.chatHudView.FocusInputField();
        }

        var data = new ChatMessage()
        {
            body = msgBody,
            sender = UserProfile.GetOwnUserProfile().userId,
        };

        WebInterface.SendChatMessage(data);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);

        if (visible)
        {
            view.StartCoroutine(ForceLayoutDelayed());
        }
    }

    IEnumerator ForceLayoutDelayed()
    {
        yield return null;
        view.chatHudView.ForceUpdateLayout();
    }

    public bool OnPressReturn()
    {
        if (EventSystem.current.currentSelectedGameObject != null &&
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
        view.chatHudView.FocusInputField();
        view.DeactivatePreview();
        InitialSceneReferences.i.mouseCatcher.UnlockCursor();

        if (!string.IsNullOrEmpty(setInputText))
        {
            view.chatHudView.inputField.text = setInputText;
            view.chatHudView.inputField.caretPosition = setInputText.Length;
        }
    }
}
