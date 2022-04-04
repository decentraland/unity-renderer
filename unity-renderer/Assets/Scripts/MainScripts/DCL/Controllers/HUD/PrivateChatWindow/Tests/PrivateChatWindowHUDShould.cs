using System;
using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

public class PrivateChatWindowHUDShould : IntegrationTestSuite_Legacy
{
    private NotificationsController notificationsController;
    private PrivateChatWindowController controller;
    private IPrivateChatComponentView view;
    private IChatHUDComponentView internalChatView;
    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;
    private UserProfileController userProfileController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        view = Substitute.For<IPrivateChatComponentView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        view.ChatHUD.Returns(internalChatView);

        userProfileController = TestUtils.CreateComponentWithGameObject<UserProfileController>("UserProfileController");
        notificationsController =
            TestUtils.CreateComponentWithGameObject<NotificationsController>("NotificationsController");

        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

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
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(userProfileController.gameObject);
        UnityEngine.Object.Destroy(notificationsController.gameObject);
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void ProcessCurrentMessagesOnControllerInitialize()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries()
            .ReturnsForAnyArgs(new List<ChatMessage>
            {
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message1"),
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message2"),
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message3"),
            });
        InitializeChatWindowController(chatController);

        internalChatView.Received(3).AddEntry(Arg.Is<ChatEntry.Model>(model =>
            model.messageType == ChatMessage.Type.PRIVATE
            && model.senderId == testProfileModel.userId));

        Received.InOrder(() =>
        {
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message1"));
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message2"));
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message3"));
        });
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

        internalChatView.Received(3).AddEntry(Arg.Is<ChatEntry.Model>(model =>
            model.messageType == ChatMessage.Type.PRIVATE
            && model.senderId == testProfileModel.userId));

        Received.InOrder(() =>
        {
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message1"));
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message2"));
            internalChatView.AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText == "message3"));
        });
    }

    [Test]
    public void SendChatMessageProperly()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);
        
        bool messageWasSent = false;

        void CheckMessageSent(string type, string msg)
        {
            if (type == "SendChatMessage" && msg.Contains("test message"))
            {
                messageWasSent = true;
            }
        }

        WebInterface.OnMessageFromEngine += CheckMessageSent;
        controller.SendChatMessage(new ChatMessage { body = "test message", recipient = "testUser" });
        Assert.IsTrue(messageWasSent);
        WebInterface.OnMessageFromEngine -= CheckMessageSent;
    }

    [Test]
    public void CloseOnCloseButtonPressed()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        view.IsActive.Returns(info => isViewActive);
        
        controller.SetVisibility(true);
        Assert.IsTrue(isViewActive);
        
        view.OnClose += Raise.Event<Action>();
        Assert.IsFalse(isViewActive);
    }

    [UnityTest]
    public IEnumerator OpenFriendsHUDOnBackButtonPressed()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);
        
        // Initialize friends HUD
        notificationsController.Initialize(new NotificationHUDController());
        
        FriendsHUDController friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(new FriendsController_Mock(), UserProfile.GetOwnUserProfile(), chatController);
        
        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");
        
        controller.SetVisibility(true);
        view.OnPressBack += Raise.Event<Action>();
        yield return null;
        
        Assert.AreEqual(true, friendsHudController.view.IsActive());
        
        friendsHudController.Dispose();
        notificationsController.Dispose();
    }

    private void InitializeChatWindowController(IChatController chatController)
    {
        controller = new PrivateChatWindowController(new DataStore(),
            Substitute.For<IUserProfileBridge>(),
            chatController,
            Substitute.For<IFriendsController>(),
            Substitute.For<IPlayerPrefs>(),
            ScriptableObject.CreateInstance<InputAction_Trigger>());
        controller.Initialize(view);
        controller.Configure(testProfileModel.userId);
        controller.SetVisibility(true);
    }
}