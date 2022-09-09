using DCL.Chat.Channels;
using DCL.Chat.HUD;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;

public class ChannelMembersHUDControllerShould
{
    private ChannelMembersHUDController channelMembersHUDController;
    private IChannelMembersComponentView channelMembersComponentView;
    private IChatController chatController;
    private IUserProfileBridge userProfileBridge;

    [SetUp]
    public void SetUp()
    {
        channelMembersComponentView = Substitute.For<IChannelMembersComponentView>();
        chatController = Substitute.For<IChatController>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        channelMembersHUDController = new ChannelMembersHUDController(channelMembersComponentView, chatController, userProfileBridge);
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
            channelMembersComponentView.Received(1).ClearSearchInput();
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
        channelMembersComponentView.Received(1).ClearSearchInput();
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
        channelMembersComponentView.Received(1).HideLoadingMore();
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
        // Arrange
        channelMembersComponentView.Configure().IsActive.Returns(info => true);

        string testChannelId = "testChannelId";

        UserProfile testUserId1Profile = new UserProfile();
        testUserId1Profile.UpdateData(new UserProfileModel
        {
            userId = "testUserId1",
            name = "testUserId1",
            snapshots = new UserProfileModel.Snapshots
            {
                face256 = ""
            }
        });
        userProfileBridge.Configure().GetByName("testUserId1").Returns(info => testUserId1Profile);

        UserProfile testUserId2Profile = new UserProfile();
        testUserId2Profile.UpdateData(new UserProfileModel
        {
            userId = "testUserId2",
            name = "testUserId2",
            snapshots = new UserProfileModel.Snapshots
            {
                face256 = ""
            }
        });
        userProfileBridge.Configure().GetByName("testUserId2").Returns(info => testUserId1Profile);

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
        userProfileBridge.Received(testChannelMembers.Length).GetByName(Arg.Any<string>());
        channelMembersComponentView.Received(testChannelMembers.Length).Set(Arg.Any<ChannelMemberEntryModel>());
        channelMembersComponentView.Received(1).ShowLoadingMore();
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
        channelMembersComponentView.Received(1).HideLoadingMore();
        chatController.Received(1).GetChannelMembers(Arg.Any<string>(), 30, channelMembersComponentView.EntryCount);
    }
}
