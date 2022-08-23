using System;
using System.Collections;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowControllerShould
    {
        private IChatController chatController;
        private SearchChannelsWindowController controller;
        private ISearchChannelsWindowView view;
        private IMouseCatcher mouseCatcher;

        [SetUp]
        public void SetUp()
        {
            chatController = Substitute.For<IChatController>();
            mouseCatcher = Substitute.For<IMouseCatcher>();
            view = Substitute.For<ISearchChannelsWindowView>();
            controller = new SearchChannelsWindowController(chatController, mouseCatcher);
            controller.Initialize(view);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void ShowView()
        {
            controller.SetVisibility(true);
            view.Received(1).Show();
        }

        [Test]
        public void HideView()
        {
            controller.SetVisibility(false);
            view.Received(1).Hide();
        }

        [Test]
        public void LoadForFirstTime()
        {
            controller.SetVisibility(true);

            view.Received(1).ClearSearchInput();
            view.Received(1).ClearAllEntries();
            view.Received(1).ShowLoading();

            chatController.Received(1).GetChannels(SearchChannelsWindowController.LOAD_PAGE_SIZE, 0);
        }

        [Test]
        public void ShowChannel()
        {
            view.IsActive.Returns(true);
            controller.SetVisibility(true);

            var channel = new Channel("channel", 15, 11, false, false, "desc", 0);
            chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(channel);

            view.Received(1).HideLoading();
            view.Received(1).ShowLoadingMore();
            view.Received(1).Set(Arg.Is<Channel>(c => c.Equals(channel)));
        }

        [Test]
        public void AvoidShowingChannelWhenIsHidden()
        {
            controller.SetVisibility(false);

            chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(
                new Channel("channel", 15, 11, false, false, "desc", 0));

            view.DidNotReceiveWithAnyArgs().Set(default);
        }

        [UnityTest]
        public IEnumerator LoadMoreChannels()
        {
            view.EntryCount.Returns(16);
            view.IsActive.Returns(true);
            controller.SetVisibility(true);

            // must wait at least 2 seconds to keep loading channels since the last time requested
            yield return new WaitForSeconds(3);
            view.ClearReceivedCalls();
            chatController.ClearReceivedCalls();

            view.OnRequestMoreChannels += Raise.Event<Action>();

            view.Received(1).HideLoadingMore();
            chatController.Received(1).GetChannels(SearchChannelsWindowController.LOAD_PAGE_SIZE, 16);
        }

        [UnityTest]
        public IEnumerator SearchChannels()
        {
            view.IsActive.Returns(true);
            controller.SetVisibility(true);

            // must wait at least 2 seconds to keep loading channels since the last time requested
            yield return new WaitForSeconds(3);
            view.ClearReceivedCalls();
            chatController.ClearReceivedCalls();

            view.OnSearchUpdated += Raise.Event<Action<string>>("bleh");

            view.Received(1).ClearAllEntries();
            view.Received(1).ShowLoading();
            chatController.Received(1).GetChannels(SearchChannelsWindowController.LOAD_PAGE_SIZE, 0, "bleh");
        }
        
        [UnityTest]
        public IEnumerator LoadFirstPageChannelsWhenClearingTheSearch()
        {
            view.IsActive.Returns(true);
            controller.SetVisibility(true);

            // must wait at least 2 seconds to keep loading channels since the last time requested
            yield return new WaitForSeconds(3);
            view.ClearReceivedCalls();
            chatController.ClearReceivedCalls();

            view.OnSearchUpdated += Raise.Event<Action<string>>("");

            view.Received(1).ClearAllEntries();
            view.Received(1).ShowLoading();
            chatController.Received(1).GetChannels(SearchChannelsWindowController.LOAD_PAGE_SIZE, 0);
        }

        [Test]
        public void JoinChannel()
        {
            controller.SetVisibility(true);

            view.OnJoinChannel += Raise.Event<Action<string>>("channelId");
            
            chatController.Received(1).JoinOrCreateChannel("channelId");
        }
        
        [Test]
        public void LeaveChannel()
        {
            controller.SetVisibility(true);

            string testChannelId = "channelId";
            string channelToLeave = "";
            controller.OnOpenChannelLeave += channelId =>
            {
                channelToLeave = channelId;
            };
            view.OnLeaveChannel += Raise.Event<Action<string>>(testChannelId);
            
            Assert.AreEqual(channelToLeave, testChannelId);
        }
    }
}