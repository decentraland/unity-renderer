using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class PrivateChatWindowHUDShould : TestsBase
{
    private PrivateChatWindowHUDController controller;
    private PrivateChatWindowHUDView view;
    private ChatController_Mock chatController;

    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;

    protected override bool justSceneSetUp => true;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        UserProfileController.i.ClearProfilesCatalog();

        var ownProfile = UserProfile.GetOwnUserProfile();

        ownProfileModel = new UserProfileModel();
        ownProfileModel.userId = "my-user-id";
        ownProfileModel.name = "NO_USER";
        ownProfile.UpdateData(ownProfileModel, false);

        testProfileModel = new UserProfileModel();
        testProfileModel.userId = "my-user-id-2";
        testProfileModel.name = "TEST_USER";
        UserProfileController.i.AddUserProfileToCatalog(testProfileModel);

        //NOTE(Brian): This profile is added by the LoadProfile message in the normal flow.
        //             Adding this here because its used by the chat flow in ChatMessageToChatEntry.
        UserProfileController.i.AddUserProfileToCatalog(ownProfileModel);

        controller = new PrivateChatWindowHUDController();
        chatController = new ChatController_Mock();

        controller.Initialize(chatController);
        controller.Configure(testProfileModel.userId);
        controller.SetVisibility(true);

        this.view = controller.view;
        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");

        yield break;
    }

    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        var entry = controller.view.chatHudView.entries[0];

        var chatEntryModel = ChatHUDController.ChatMessageToChatEntry(chatMessage);

        Assert.AreEqual(entry.model, chatEntryModel);
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
        controller.SendChatMessage(new ChatMessage() { body = "test message", recipient = "testUser" });
        Assert.IsTrue(messageWasSent);
        Assert.AreEqual("", controller.view.chatHudView.inputField.text);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield break;
    }

    [Test]
    public void DisplayCorrectUserMessages()
    {
        // Send 2 private messages from correct user
        var chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message 2",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(2, controller.view.chatHudView.entries.Count);

        // Send 1 PUBLIC message from correct user
        chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message 3 - PUBLIC",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(2, controller.view.chatHudView.entries.Count);

        // Send 1 PRIVATE message from incorrect user
        chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message 3 - INCORRECT-USER",
            sender = "other-user-id"
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(2, controller.view.chatHudView.entries.Count);
    }

    [Test]
    public void CloseOnCloseButtonPressed()
    {
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);
        controller.view.minimizeButton.onClick.Invoke();
        Assert.AreEqual(false, controller.view.gameObject.activeSelf);
    }

    [Test]
    public void CloseOnBackButtonPressed()
    {
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);
        bool pressedBack = false;
        controller.view.OnPressBack += () => { pressedBack = true; };
        controller.view.backButton.onClick.Invoke();
        Assert.IsTrue(pressedBack);
    }

    [UnityTest]
    public IEnumerator OpenFriendsHUDOnBackButtonPressed()
    {
        // Initialize friends HUD
        NotificationsController.i.Initialize(new NotificationHUDController());

        var friendsHUDController = new FriendsHUDController();
        friendsHUDController.Initialize(new FriendsController_Mock(), UserProfile.GetOwnUserProfile());

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");

        // initialie private chat
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);

        controller.view.backButton.onClick.Invoke();
        yield return null;

        Assert.AreEqual(true, friendsHUDController.view.gameObject.activeSelf);
    }
}
