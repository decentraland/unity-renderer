
using DCL;
using DCL.Interface;
using System.Collections;
using UnityEngine;

public class WorldChatWindowHUDController : IHUD
{
    private ChatHUDController chatHudController;
    public WorldChatWindowHUDView view;

    private string userName;

    private IChatController chatController;
    private IMouseCatcher mouseCatcher;

    internal bool resetInputFieldOnSubmit = true;

    public void Initialize(IChatController chatController, IMouseCatcher mouseCatcher)
    {
        view = WorldChatWindowHUDView.Create();

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView, SendChatMessage);

        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;

        if (chatController != null)
        {
            chatController.OnAddMessage -= view.chatHudView.controller.AddChatMessage;
            chatController.OnAddMessage += view.chatHudView.controller.AddChatMessage;
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock += view.ActivatePreview;
        }

        userName = "NO_USER";

        var profileUserName = UserProfile.GetOwnUserProfile().userName;

        if (!string.IsNullOrEmpty(profileUserName))
            userName = profileUserName;

        if (chatController != null)
            chatHudController.view.RepopulateAllChatMessages(chatController.GetEntries());

        view.chatHudView.ForceUpdateLayout();
    }
    public void Dispose()
    {
        if (chatController != null)
            chatController.OnAddMessage -= view.chatHudView.controller.AddChatMessage;

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= view.ActivatePreview;
        }

        Object.Destroy(view);
    }

    int invalidSubmitLastFrame = 0;

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
        var data = new ChatController.ChatMessage()
        {
            body = msgBody,
            sender = userName,
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
        if (view.chatHudView.inputField.isFocused || (Time.frameCount - invalidSubmitLastFrame) < 2)
            return false;

        SetVisibility(true);
        view.chatHudView.FocusInputField();
        view.DeactivatePreview();
        InitialSceneReferences.i.mouseCatcher.UnlockCursor();
        return true;
    }
}
