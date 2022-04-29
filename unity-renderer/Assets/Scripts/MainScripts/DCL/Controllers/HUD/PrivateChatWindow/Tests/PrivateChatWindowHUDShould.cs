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
    private IUserProfileBridge userProfileBridge;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        view = Substitute.For<IPrivateChatComponentView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        view.ChatHUD.Returns(internalChatView);

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

        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(ownProfileModel);
        var testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(testProfileModel);
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        userProfileBridge.Get(ownProfileModel.userId).Returns(ownUserProfile);
        userProfileBridge.Get(testProfileModel.userId).Returns(testUserProfile);
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(notificationsController.gameObject);
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void ProcessCurrentMessagesWhenInitialize()
    {
        IChatController chatController = Substitute.For<IChatController>();
        chatController.GetEntries()
            .ReturnsForAnyArgs(new List<ChatMessage>
            {
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message1")
                    {recipient = testProfileModel.userId},
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message2")
                    {recipient = testProfileModel.userId},
                new ChatMessage(ChatMessage.Type.PRIVATE, testProfileModel.userId, "message3")
                    {recipient = testProfileModel.userId},
            });
        InitializeChatWindowController(chatController);

        internalChatView.Received(3).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == ChatMessage.Type.PRIVATE
            && model.senderId == testProfileModel.userId));

        Received.InOrder(() =>
        {
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message1</noparse>"));
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message2</noparse>"));
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message3</noparse>"));
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

        internalChatView.Received(3).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == ChatMessage.Type.PRIVATE
            && model.senderId == testProfileModel.userId));

        Received.InOrder(() =>
        {
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message1</noparse>"));
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message2</noparse>"));
            internalChatView.AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == "<noparse>message3</noparse>"));
        });
    }

    [Test]
    public void SendChatMessageWhenViewTriggers()
    {
        var chatController = Substitute.For<IChatController>();
        chatController.GetEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        InitializeChatWindowController(chatController);

        internalChatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage {body = "test message"});

        chatController.Received(1).Send(Arg.Is<ChatMessage>(message => message.body == $"/w {testProfileModel.name} test message"
                                                                 && message.recipient == testProfileModel.name));
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
            userProfileBridge,
            chatController,
            Substitute.For<IFriendsController>(),
            ScriptableObject.CreateInstance<InputAction_Trigger>(),
            Substitute.For<ILastReadMessagesService>());
        controller.Initialize(view);
        controller.Setup(testProfileModel.userId);
        controller.SetVisibility(true);
    }
}