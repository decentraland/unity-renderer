using DCL;
using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

class ChatController_Mock : IChatController
{
    public event Action<ChatController.ChatMessage> OnAddMessage;

    public List<ChatController.ChatMessage> GetEntries()
    {
        return new List<ChatController.ChatMessage>();
    }

    public void RaiseAddMessage(ChatController.ChatMessage chatMessage) { OnAddMessage?.Invoke(chatMessage); }
}
class MouseCatcher_Mock : IMouseCatcher
{
    public event Action OnMouseUnlock;
    public event Action OnMouseLock;

    public void RaiseMouseUnlock() { OnMouseUnlock?.Invoke(); }
    public void RaiseMouseLock() { OnMouseLock?.Invoke(); }
}

public class WorldChatWindowHUDShould : TestsBase
{
    private WorldChatWindowHUDController controller;
    private WorldChatWindowHUDView view;
    private ChatController_Mock chatController;
    private MouseCatcher_Mock mouseCatcher;

    protected override IEnumerator SetUp()
    {
        controller = new WorldChatWindowHUDController();
        chatController = new ChatController_Mock();
        mouseCatcher = new MouseCatcher_Mock();
        controller.Initialize(chatController, mouseCatcher);
        this.view = controller.view;
        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");
        yield break;
    }

    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.PUBLIC,
            body = "test message",
            sender = "test user"
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        var entry = controller.view.chatHudView.entries[0];

        Assert.AreEqual(entry.message, chatMessage);
    }

    [Test]
    public void HandleMouseCatcherProperly()
    {
        mouseCatcher.RaiseMouseLock();
        Assert.AreEqual(0, view.group.alpha);
    }


    [UnityTest]
    public IEnumerator SendChatMessageProperly()
    {
        bool messageWasSent = false;

        System.Action<string, string> messageCallback =
            (type, msg) =>
            {
                if (type == "SendChatMessage" && msg.Contains("test message"))
                {
                    messageWasSent = true;
                }
            };

        WebInterface.OnMessageFromEngine += messageCallback;
        controller.resetInputFieldOnSubmit = false;
        controller.SendChatMessage("test message");
        Assert.IsTrue(messageWasSent);
        Assert.AreEqual("", controller.view.chatHudView.inputField.text);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield break;
    }

    [Test]
    public void CloseWhenButtonPressed()
    {
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);
        controller.view.closeButton.onClick.Invoke();
        Assert.AreEqual(false, controller.view.gameObject.activeSelf);
    }
}
