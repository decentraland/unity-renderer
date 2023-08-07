using DCL.Chat.Channels;
using DCL.Social.Chat;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChannelMembersHUDControllerShould
    {
        private ChannelMembersHUDController channelMembersHUDController;
        private IChannelMembersComponentView channelMembersComponentView;
        private IChatController chatController;
        private IUserProfileBridge userProfileBridge;
        private DataStore_Channels dataStoreChannels;

        [SetUp]
        public void SetUp()
        {
            channelMembersComponentView = Substitute.For<IChannelMembersComponentView>();
            chatController = Substitute.For<IChatController>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            dataStoreChannels = new DataStore_Channels();
            channelMembersHUDController = new ChannelMembersHUDController(channelMembersComponentView, chatController, userProfileBridge, dataStoreChannels);
            channelMembersHUDController.SetMembersCount(100);
        }

        [TearDown]
        public void TearDown() { channelMembersHUDController.Dispose(); }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(channelMembersComponentView, channelMembersHUDController.View);
        }

        [Test]
        public void SetChannelIdCorrectly()
        {
            // Arrange
            string testId = "testId";

            // Act
            channelMembersHUDController.SetChannelId(testId);

            // Assert
            Assert.AreEqual(testId, channelMembersHUDController.CurrentChannelId);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityCorrectly(bool isVisible)
        {
            // Act
            channelMembersHUDController.SetVisibility(isVisible);

            // Assert
            if (isVisible)
            {
                channelMembersComponentView.Received(1).ClearSearchInput(false);
                channelMembersComponentView.Received(1).Show();
                channelMembersComponentView.Received(1).ClearAllEntries();
                channelMembersComponentView.Received(1).ShowLoading();
                chatController.Received(1).GetChannelInfo(Arg.Any<string[]>());
                chatController.Received(1).GetChannelMembers(Arg.Any<string>(), 0, 0);
            }
            else
            {
                channelMembersComponentView.Received(1).Hide();
            }
        }

        [Test]
        public void LoadMembersCorrectly()
        {
            // Act
            string testChannelId = "testId";
            channelMembersHUDController.SetChannelId(testChannelId);
            channelMembersHUDController.SetVisibility(true);

            // Assert
            channelMembersComponentView.Received(1).ClearSearchInput(false);
            channelMembersComponentView.Received(1).Show();
            channelMembersComponentView.Received(1).ClearAllEntries();
            channelMembersComponentView.Received(1).ShowLoading();
            chatController.Received(1).GetChannelInfo(Arg.Any<string[]>());
            chatController.Received(1).GetChannelMembers(testChannelId, 30, 0);
        }

        [Test]
        [TestCase("test text")]
        [TestCase(null)]
        public void SearchMembersCorrectly(string textToSearch)
        {
            // Act
            string testChannelId = "testId";
            channelMembersHUDController.SetChannelId(testChannelId);
            channelMembersHUDController.SetVisibility(true);
            channelMembersComponentView.OnSearchUpdated += Raise.Event<Action<string>>(textToSearch);

            // Assert
            channelMembersComponentView.Received().ClearAllEntries();
            channelMembersComponentView.Received().HideLoadingMore();
            channelMembersComponentView.Received().ShowLoading();
            if (string.IsNullOrEmpty(textToSearch))
            {
                chatController.Received().GetChannelMembers(testChannelId, 30, 0);
                channelMembersComponentView.Received(1).HideResultsHeader();
            }
            else
            {
                chatController.Received(1).GetChannelMembers(testChannelId, 30, 0, textToSearch);
                channelMembersComponentView.Received(1).ShowResultsHeader();
            }
        }

        [Test]
        public void UpdateChannelMembersCorrectly()
        {
            const string testChannelId = "testChannelId";

            // Arrange
            var testUserId1Profile = ScriptableObject.CreateInstance<UserProfile>();
            testUserId1Profile.UpdateData(new UserProfileModel
            {
                userId = "testUserId1",
                name = "testUserId1",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = ""
                }
            });
            userProfileBridge.Configure().Get("testUserId1").Returns(info => testUserId1Profile);

            var testUserId2Profile = ScriptableObject.CreateInstance<UserProfile>();
            testUserId2Profile.UpdateData(new UserProfileModel
            {
                userId = "testUserId2",
                name = "testUserId2",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = ""
                }
            });
            userProfileBridge.Configure().Get("testUserId2").Returns(info => testUserId2Profile);

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownProfile.UpdateData(new UserProfileModel{userId = "ownProfileId"});
            userProfileBridge.GetOwn().Returns(ownProfile);

            ChannelMember[] testChannelMembers =
            {
                new ChannelMember { userId = "testUserId1", isOnline = false },
                new ChannelMember { userId = "testUserId2", isOnline = true },
            };

            // Act
            channelMembersHUDController.SetChannelId("testId");
            channelMembersHUDController.SetVisibility(true);
            chatController.OnUpdateChannelMembers += Raise.Event<Action<string, ChannelMember[]>>(testChannelId, testChannelMembers);

            // Assert
            channelMembersComponentView.Received(1).HideLoading();
            channelMembersComponentView.Received(1).Set(Arg.Is<ChannelMemberEntryModel>(c =>
                c.userName == "testUserId1"));
            channelMembersComponentView.Received(1).Set(Arg.Is<ChannelMemberEntryModel>(c =>
                c.userName == "testUserId2"));
            channelMembersComponentView.Received(testChannelMembers.Length).Set(Arg.Any<ChannelMemberEntryModel>());
            channelMembersComponentView.Received().ShowLoadingMore();
        }

        [Test]
        public void LoadMoreMembersCorrectly()
        {
            // Arrange
            channelMembersComponentView.ClearSearchInput();
            channelMembersComponentView.Configure().EntryCount.Returns(info => 5);

            // Act
            channelMembersHUDController.SetChannelId("testId");
            channelMembersHUDController.SetVisibility(true);
            channelMembersHUDController.loadStartedTimestamp = DateTime.MinValue;
            channelMembersComponentView.OnRequestMoreMembers += Raise.Event<Action>();

            // Assert
            channelMembersComponentView.Received().HideLoadingMore();
            chatController.Received(1).GetChannelMembers(Arg.Any<string>(), 30, channelMembersComponentView.EntryCount);
        }
    }
}
