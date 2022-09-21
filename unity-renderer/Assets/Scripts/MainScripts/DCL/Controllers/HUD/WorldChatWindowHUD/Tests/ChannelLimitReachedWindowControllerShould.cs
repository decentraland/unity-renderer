using System;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowControllerShould
    {
        private IChannelLimitReachedWindowView view;
        private ChannelLimitReachedWindowController controller;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IChannelLimitReachedWindowView>();
            dataStore = new DataStore();
            controller = new ChannelLimitReachedWindowController(view, dataStore);
            view.ClearReceivedCalls();
        }

        [Test]
        public void Show()
        {
            dataStore.channels.currentChannelLimitReached.Set("channel");
            view.Received(1).Show();
        }

        [Test]
        public void HideWhenViewCloses()
        {
            view.OnClose += Raise.Event<Action>();
            
            view.Received(1).Hide();
        }
    }
}