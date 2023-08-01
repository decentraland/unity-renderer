using System;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Social.Chat
{
    public class ChannelJoinErrorWindowControllerShould
    {
        private IChannelJoinErrorWindowView view;
        private ChannelJoinErrorWindowController controller;
        private DataStore dataStore;
        private IChatController chatController;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IChannelJoinErrorWindowView>();
            dataStore = new DataStore();
            chatController = Substitute.For<IChatController>();
            controller = new ChannelJoinErrorWindowController(view, chatController, dataStore);
            view.ClearReceivedCalls();
        }

        [Test]
        public void Show()
        {
            chatController.GetAllocatedChannel("channel")
                .Returns(new Channel("channel", "channelName", 0, 1, false, false, ""));
            dataStore.channels.joinChannelError.Set("channel");
            view.Received(1).Show("channelName");
        }

        [Test]
        public void HideWhenViewCloses()
        {
            view.OnClose += Raise.Event<Action>();

            view.Received(1).Hide();
        }

        [Test]
        public void RequestJoinWhenRetry()
        {
            dataStore.channels.joinChannelError.Set("randomChannel");
            view.ClearReceivedCalls();

            view.OnRetry += Raise.Event<Action>();

            chatController.Received(1).JoinOrCreateChannel("randomChannel");
            view.Received(1).Hide();
        }
    }
}
