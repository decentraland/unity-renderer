using System;
using System.Collections.Generic;
using DCL.Chat.Channels;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChatChannelHUDControllerShould
    {
        private const string CHANNEL_ID = "channelId";

        private ChatChannelHUDController controller;
        private IChatChannelWindowView view;
        private IChatHUDComponentView chatView;
        private IChatController chatController;

        [SetUp]
        public void SetUp()
        {
            var userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownUserId",
                name = "self"
            });
            userProfileBridge.GetOwn().Returns(ownUserProfile);
            
            chatController = Substitute.For<IChatController>();
            chatController.GetAllocatedChannel(CHANNEL_ID)
                .Returns(new Channel(CHANNEL_ID, 4, 12, true, false, "desc", 0));
            chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
            
            controller = new ChatChannelHUDController(new DataStore(),
                userProfileBridge,
                chatController,
                ScriptableObject.CreateInstance<InputAction_Trigger>(),
                Substitute.For<IMouseCatcher>(),
                ScriptableObject.CreateInstance<InputAction_Trigger>());
            view = Substitute.For<IChatChannelWindowView>();
            chatView = Substitute.For<IChatHUDComponentView>();
            view.ChatHUD.Returns(chatView);
            controller.Initialize(view);
            controller.Setup(CHANNEL_ID);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void LeaveChannelViaChatCommand()
        {
            chatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage
            {
                body = "/leave",
                messageType = ChatMessage.Type.PUBLIC
            });

            chatController.Received(1).LeaveChannel(CHANNEL_ID);
        }

        [Test]
        public void GoBackWhenLeavingChannel()
        {
            var backCalled = false;
            controller.OnPressBack += () => backCalled = true;

            chatController.OnChannelLeft += Raise.Event<Action<string>>(CHANNEL_ID);

            Assert.IsTrue(backCalled);
        }

        [Test]
        public void LeaveChannelWhenViewRequests()
        {
            view.OnLeaveChannel += Raise.Event<Action>();
            
            chatController.Received(1).LeaveChannel(CHANNEL_ID);
        }
    }
}