using System;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowControllerShould
    {
        private IChannelLimitReachedWindowView view;
        private ChannelLimitReachedWindowController controller;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IChannelLimitReachedWindowView>();
            controller = new ChannelLimitReachedWindowController(view);
        }

        [Test]
        public void Show()
        {
            controller.SetVisibility(true);
            view.Received(1).Show();
        }

        [Test]
        public void Hide()
        {
            controller.SetVisibility(false);
            view.Received(1).Hide();
        }

        [Test]
        public void HideWhenViewCloses()
        {
            controller.SetVisibility(true);
            view.ClearReceivedCalls();
            
            view.OnClose += Raise.Event<Action>();
            
            view.Received(1).Hide();
        }
    }
}