using System;
using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using UnityEngine.TestTools;

public class PrivateChatWindowHUDShould : IntegrationTestSuite_Legacy
{
    private PrivateChatWindowHUDController controller;
    private PrivateChatWindowHUDView view;

    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;

    protected override bool justSceneSetUp => true;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        UserProfileController.i.ClearProfilesCatalog();

        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

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
    }

    private void InitializeChatWindowController(IChatController chatController)
    {
        controller = new PrivateChatWindowHUDController();
        controller.Initialize(chatController);
        controller.Configure(testProfileModel.userId);
        controller.SetVisibility(true);
        view = controller.view;
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void ProcessCurrentMessagesOnControllerInitialize()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>
        {
            new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message1"),
            new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message2"),
            new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message3"),
        });
        InitializeChatWindowController(chatController);

        Assert.AreEqual(3, controller.view.chatHudView.entries.Count);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(0).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(0).senderId);
        Assert.AreEqual("message1", GetViewEntryModel(0).bodyText);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(1).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(1).senderId);
        Assert.AreEqual("message2", GetViewEntryModel(1).bodyText);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(2).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(2).senderId);
        Assert.AreEqual("message3", GetViewEntryModel(2).bodyText);
    }

    [Test]
    public void ReceivesOneMessageProperly()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

        ChatMessage chatMessage1 = new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message1");
        ChatMessage chatMessage2 = new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message2");
        ChatMessage chatMessage3 = new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message3");
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(chatMessage1);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(chatMessage2);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(chatMessage3);

        Assert.AreEqual(3, controller.view.chatHudView.entries.Count);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(0).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(0).senderId);
        Assert.AreEqual("message1", GetViewEntryModel(0).bodyText);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(1).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(1).senderId);
        Assert.AreEqual("message2", GetViewEntryModel(1).bodyText);

        Assert.AreEqual(ChatMessage.Type.PRIVATE, GetViewEntryModel(2).messageType);;
        Assert.AreEqual(testProfileModel.userId, GetViewEntryModel(2).senderId);
        Assert.AreEqual("message3", GetViewEntryModel(2).bodyText);
    }

    [UnityTest]
    public IEnumerator SendChatMessageProperly()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

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
        controller.SendChatMessage(new ChatMessage() {body = "test message", recipient = "testUser"});
        Assert.IsTrue(messageWasSent);
        Assert.AreEqual("", controller.view.chatHudView.inputField.text);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield break;
    }

    [Test]
    public void CloseOnCloseButtonPressed()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);
        controller.view.minimizeButton.onClick.Invoke();
        Assert.AreEqual(false, controller.view.gameObject.activeSelf);
    }

    [Test]
    public void CloseOnBackButtonPressed()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

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
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

        // Initialize friends HUD
        NotificationsController.i.Initialize(new NotificationHUDController());

        FriendsHUDController friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(new FriendsController_Mock(), UserProfile.GetOwnUserProfile());

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");

        // initialize private chat
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);

        controller.view.backButton.onClick.Invoke();
        yield return null;

        Assert.AreEqual(true, friendsHudController.view.gameObject.activeSelf);

        friendsHudController.Dispose();
        NotificationsController.i.Dispose();
    }

    private ChatEntry.Model GetViewEntryModel(int index)
    {
        return controller.view.chatHudView.entries[index].model;
    }
}
