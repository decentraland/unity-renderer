using System;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowControllerShould
    {
        private IChannelLeaveErrorWindowView view;
        private ChannelLeaveErrorWindowController controller;
        private DataStore dataStore;
        private IChatController chatController;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IChannelLeaveErrorWindowView>();
            dataStore = new DataStore();
            chatController = Substitute.For<IChatController>();
            controller = new ChannelLeaveErrorWindowController(view, chatController, dataStore);
            view.ClearReceivedCalls();
        }

        [Test]
        public void Show()
        {
            chatController.GetAllocatedChannel("channel")
                .Returns(new Channel("channel", "channelName", 0, 1, false, false, ""));
            dataStore.channels.leaveChannelError.Set("channel");
            view.Received(1).Show("channelName");
        }

        [Test]
        public void HideWhenViewCloses()
        {
            view.OnClose += Raise.Event<Action>();
            
            view.Received(1).Hide();
        }

        [Test]
        public void RequestLeaveWhenRetry()
        {
            dataStore.channels.leaveChannelError.Set("randomChannel");
            view.ClearReceivedCalls();
            
            view.OnRetry += Raise.Event<Action>();
            
            chatController.Received(1).LeaveChannel("randomChannel");
            view.Received(1).Hide();
        }
    }
}