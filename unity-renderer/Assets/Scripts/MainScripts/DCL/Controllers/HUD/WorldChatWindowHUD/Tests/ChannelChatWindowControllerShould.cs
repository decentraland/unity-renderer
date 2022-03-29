using System;
using System.Collections;
using DCL.Helpers;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class ChannelChatWindowControllerShould : IntegrationTestSuite_Legacy
{
    private PublicChatChannelController controller;
    private IChannelChatWindowView view;
    private IChatHUDComponentView internalChatView;
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

        controller = new PublicChatChannelController(chatController, mouseCatcher,
            Substitute.For<IPlayerPrefs>(),
            ScriptableObject.CreateInstance<LongVariable>());
        chatController = new ChatController_Mock();
        mouseCatcher = new MouseCatcher_Mock();

        view = Substitute.For<IChannelChatWindowView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        view.ChatHUD.Returns(internalChatView);
        controller.Initialize(view);

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
        var sentPM = new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = ownProfileModel.userId,
            recipient = testProfileModel.userId
        };
        
        chatController.RaiseAddMessage(sentPM);
        
        internalChatView.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model =>
            model.Equals(ChatHUDController.ChatMessageToChatEntry(sentPM))));
    }

    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message",
            sender = testProfileModel.userId
        };
        
        chatController.RaiseAddMessage(chatMessage);
        
        var chatEntryModel = ChatHUDController.ChatMessageToChatEntry(chatMessage);
        internalChatView.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model =>
            model.Equals(chatEntryModel)));
    }

    [UnityTest]
    public IEnumerator SendChatMessageProperly()
    {
        bool messageWasSent = false;

        Action<string, string> messageCallback =
            (type, msg) =>
            {
                if (type == "SendChatMessage" && msg.Contains("test message"))
                {
                    messageWasSent = true;
                }
            };

        WebInterface.OnMessageFromEngine += messageCallback;
        controller.SendChatMessage(new ChatMessage { body = "test message" });
        Assert.IsTrue(messageWasSent);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield return null;
        yield return null;
        yield return null;
    }

    [Test]
    public void CloseWhenButtonPressed()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        view.IsActive.Returns(info => isViewActive);
        controller.SetVisibility(true);
        Assert.IsTrue(view.IsActive);
        controller.view.OnClose += Raise.Event<Action>();
        Assert.IsFalse(view.IsActive);
    }

    [UnityTest]
    public IEnumerator WhisperLastPrivateMessageSenderOnReply()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        
        var model = new UserProfileModel
        {
            userId = "testUserId",
            name = "testUserName",
        };
        
        userProfileController.AddUserProfileToCatalog(model);
        
        var msg = new ChatMessage
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

        view.OnMessageUpdated += Raise.Event<Action<string>>("/r ");
        
        internalChatView.Received(1).SetInputFieldText($"/w {model.name} ");
    }
    
    [Test]
    public void HandleMouseCatcherProperly()
    {
        mouseCatcher.RaiseMouseLock();
        view.Received(1).ActivatePreview();
    }
}