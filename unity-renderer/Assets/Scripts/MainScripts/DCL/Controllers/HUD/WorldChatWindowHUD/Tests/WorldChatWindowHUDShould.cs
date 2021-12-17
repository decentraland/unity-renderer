using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using DCL.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class WorldChatWindowHUDShould : IntegrationTestSuite_Legacy
{
    private WorldChatWindowHUDController controller;
    private WorldChatWindowHUDView view;
    private ChatController_Mock chatController;
    private MouseCatcher_Mock mouseCatcher;

    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;

    private UserProfileController userProfileController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        userProfileController = TestUtils.CreateComponentWithGameObject<UserProfileController>("UserProfileController");
        userProfileController.ClearProfilesCatalog();

        var ownProfile = UserProfile.GetOwnUserProfile();

        ownProfileModel = new UserProfileModel();
        ownProfileModel.userId = "my-user-id";
        ownProfileModel.name = "NO_USER";
        ownProfile.UpdateData(ownProfileModel);

        testProfileModel = new UserProfileModel();
        testProfileModel.userId = "my-user-id-2";
        testProfileModel.name = "TEST_USER";
        userProfileController.AddUserProfileToCatalog(testProfileModel);

        //NOTE(Brian): This profile is added by the LoadProfile message in the normal flow.
        //             Adding this here because its used by the chat flow in ChatMessageToChatEntry.
        userProfileController.AddUserProfileToCatalog(ownProfileModel);

        controller = new WorldChatWindowHUDController();
        chatController = new ChatController_Mock();
        mouseCatcher = new MouseCatcher_Mock();

        controller.Initialize(chatController, mouseCatcher);
        this.view = controller.view;

        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(userProfileController.gameObject);
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void HandlePrivateMessagesProperly()
    {
        var sentPM = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = ownProfileModel.userId,
            recipient = testProfileModel.userId
        };

        chatController.RaiseAddMessage(sentPM);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        ChatEntry entry = controller.view.chatHudView.entries[0];

        Assert.AreEqual("<b>To TEST_USER:</b>", entry.username.text);
        Assert.AreEqual("<b>To TEST_USER:</b> test message", entry.body.text);

        var receivedPM = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = testProfileModel.userId,
            recipient = ownProfileModel.userId
        };

        chatController.RaiseAddMessage(receivedPM);

        ChatEntry entry2 = controller.view.chatHudView.entries[1];

        Assert.AreEqual("<b>From TEST_USER:</b>", entry2.username.text);
        Assert.AreEqual("<b>From TEST_USER:</b> test message", entry2.body.text);
    }

    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        var entry = controller.view.chatHudView.entries[0];

        var chatEntryModel = ChatHUDController.ChatMessageToChatEntry(chatMessage);

        Assert.AreEqual(entry.model, chatEntryModel);
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
        controller.SendChatMessage(new ChatMessage() { body = "test message" });
        Assert.IsTrue(messageWasSent);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield return null;
        yield return null;
        yield return null;
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

    [UnityTest]
    public IEnumerator KeepWhisperCommandAfterUsage()
    {
        string baseCommand = "/w testUser ";

        controller.resetInputFieldOnSubmit = false;

        controller.view.chatHudView.inputField.text = baseCommand + "testMessage";
        yield return null;

        controller.view.chatHudView.inputField.onSubmit.Invoke(controller.view.chatHudView.inputField.text);

        yield return null;

        Assert.AreEqual(baseCommand, controller.view.chatHudView.inputField.text);

        baseCommand = "/w testUser ";

        controller.view.chatHudView.inputField.text = baseCommand + "testMessage";
        yield return null;

        controller.view.chatHudView.inputField.onSubmit.Invoke(controller.view.chatHudView.inputField.text);

        yield return null;

        Assert.AreEqual(baseCommand, controller.view.chatHudView.inputField.text);
        yield break;
    }

    [UnityTest]
    public IEnumerator WhisperLastPrivateMessageSenderOnReply()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var model = new UserProfileModel()
        {
            userId = "testUserId",
            name = "testUserName",
        };

        userProfileController.AddUserProfileToCatalog(model);

        var msg = new ChatMessage()
        {
            body = "test message",
            sender = model.userId,
            recipient = ownProfile.userId,
            messageType = ChatMessage.Type.PRIVATE
        };

        yield return null;

        chatController.AddMessageToChatWindow(JsonUtility.ToJson(msg));

        yield return null;

        Assert.AreEqual(controller.lastPrivateMessageReceivedSender, model.name);

        controller.view.chatHudView.inputField.text = "/r ";

        Assert.AreEqual($"/w {model.name} ", controller.view.chatHudView.inputField.text);

        yield return null;
    }
}