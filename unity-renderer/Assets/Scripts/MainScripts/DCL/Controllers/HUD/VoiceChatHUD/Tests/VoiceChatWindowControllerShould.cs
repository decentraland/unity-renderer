using DCL;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
using DCl.Social.Friends;
using DCL.Social.Friends;
using UnityEngine;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowControllerShould
{
    private VoiceChatWindowController voiceChatWindowController;
    private IVoiceChatWindowComponentView voiceChatWindowComponentView;
    private IVoiceChatBarComponentView voiceChatBarComponentView;
    private IUserProfileBridge userProfileBridge;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private IMouseCatcher mouseCatcher;
    private DataStore dataStore;

    [SetUp]
    public void SetUp()
    {
        voiceChatWindowComponentView = Substitute.For<IVoiceChatWindowComponentView>();
        voiceChatBarComponentView = Substitute.For<IVoiceChatBarComponentView>();
        voiceChatWindowController = Substitute.ForPartsOf<VoiceChatWindowController>();
        voiceChatWindowController.Configure().CreateVoiceChatWindowView().Returns(info => voiceChatWindowComponentView);
        voiceChatWindowController.Configure().CreateVoiceChatBatView().Returns(info => voiceChatBarComponentView);
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        mouseCatcher = Substitute.For<IMouseCatcher>();
        userProfileBridge.Configure().GetOwn().Returns(info => ScriptableObject.CreateInstance<UserProfile>());
        userProfileBridge.Configure().Get(Arg.Any<string>()).Returns(info => ScriptableObject.CreateInstance<UserProfile>());
        friendsController = Substitute.For<IFriendsController>();
        friendsController.Configure().GetAllocatedFriends().Returns(info => new Dictionary<string, UserStatus>());
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        dataStore = new DataStore();

        FeatureFlag testFeatureFlag = new FeatureFlag();
        testFeatureFlag.flags.Add("voice_chat", true);
        dataStore.featureFlags.flags.Set(testFeatureFlag);

        Settings.CreateSharedInstance(new DefaultSettingsFactory());
        voiceChatWindowController.Initialize(userProfileBridge, friendsController, socialAnalytics, dataStore, Settings.i, mouseCatcher);
    }

    [TearDown]
    public void TearDown()
    {
        voiceChatWindowController.Dispose();
    }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(voiceChatWindowComponentView, voiceChatWindowController.VoiceChatWindowView);
        Assert.AreEqual(voiceChatBarComponentView, voiceChatWindowController.VoiceChatBarView);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Act
        voiceChatWindowController.SetVisibility(isVisible);

        // Assert
        if (isVisible)
            voiceChatWindowComponentView.Received(1).Show();
        else
            voiceChatWindowComponentView.ReceivedWithAnyArgs(1).Hide();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUsersMutedCorrectly(bool isMuted)
    {
        // Arange
        string[] testPlayersId = { "playerId1", "playerId2", "playerId3" };

        // Act
        voiceChatWindowController.SetUsersMuted(testPlayersId, isMuted);

        // Assert
        foreach (var playerId in testPlayersId)
        {
            voiceChatWindowComponentView.Received(1).SetPlayerMuted(playerId, isMuted);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUserRecordingCorrectly(bool isRecording)
    {
        // Arrange
        string testUserId = "TestUserId";

        // Act
        voiceChatWindowController.SetUserRecording(testUserId, isRecording);

        // Assert
        voiceChatWindowComponentView.Received(1).SetPlayerRecording(testUserId, isRecording);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVoiceChatRecordingCorrectly(bool isRecording)
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = !isRecording;

        // Act
        voiceChatWindowController.SetVoiceChatRecording(isRecording);

        // Assert
        voiceChatBarComponentView.Received(1).PlayVoiceChatRecordingAnimation(isRecording);
        Assert.AreEqual(isRecording, voiceChatWindowController.isOwnPLayerTalking);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVoiceChatEnabledBySceneCorrectly(bool isEnabled)
    {
        // Act
        voiceChatWindowController.SetVoiceChatEnabledByScene(isEnabled);

        // Assert
        voiceChatBarComponentView.Received(1).SetVoiceChatEnabledByScene(isEnabled);
    }

    [Test]
    public void CloseViewCorrectly()
    {
        // Act
        voiceChatWindowController.CloseView();

        // Assert
        voiceChatWindowComponentView.ReceivedWithAnyArgs(1).Hide();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RequestJoinVoiceChatCorrectly(bool isJoined)
    {
        // Arrange
        dataStore.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, true), false);

        // Act
        voiceChatWindowController.RequestJoinVoiceChat(isJoined);

        // Assert
        if (!isJoined)
        {
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Key);
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Value);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnVoiceChatStatusUpdatedCorrectly(bool isJoined)
    {
        // Arrange
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayers.Returns(info => 1);
        dataStore.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, true), false);
        voiceChatWindowController.isOwnPLayerTalking = true;

        // Act
        voiceChatWindowController.OnVoiceChatStatusUpdated(isJoined, false);

        // Assert
        voiceChatWindowComponentView.Received().SetAsJoined(isJoined);
        voiceChatBarComponentView.Received().SetAsJoined(isJoined);

        if (isJoined)
        {
            socialAnalytics.Received(1).SendVoiceChannelConnection(1);
        }
        else
        {
            socialAnalytics.Received().SendVoiceChannelDisconnection();
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Key);
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Value);
            Assert.IsFalse(voiceChatWindowController.isOwnPLayerTalking);
        }
    }

    [Test]
    public void RaiseOnOtherPlayersStatusAddedCorrectly()
    {
        // Arrange
        string testPlayerId = "playerId4";
        string testPlayerName = "Test Player";
        Player testPlayer = new Player
        {
            id = testPlayerId,
            name = testPlayerName
        };

        // Act
        voiceChatWindowController.OnOtherPlayersStatusAdded(testPlayerId, testPlayer);

        // Assert
        voiceChatWindowComponentView.Received(1).AddOrUpdatePlayer(Arg.Any<UserProfile>());
        voiceChatWindowComponentView.Received(1).SetPlayerMuted(testPlayerId, Arg.Any<bool>());
        voiceChatWindowComponentView.Received(1).SetPlayerBlocked(testPlayerId, Arg.Any<bool>());
        voiceChatWindowComponentView.Received(1).SetPlayerAsFriend(testPlayerId, Arg.Any<bool>());
        voiceChatWindowComponentView.Received(1).SetPlayerAsJoined(testPlayerId, Arg.Any<bool>());
    }

    [Test]
    public void RaiseOnOtherPlayerStatusRemovedCorrectly()
    {
        // Arrange
        string testPlayerId = "TestPlayerId";

        // Act
        voiceChatWindowController.OnOtherPlayerStatusRemoved(testPlayerId, null);

        // Assert
        voiceChatWindowComponentView.Received(1).RemoveUser(testPlayerId);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnMuteAllToggledCorrectly(bool isMute)
    {
        // Arrange
        voiceChatWindowController.isMuteAll = !isMute;

        // Act
        voiceChatWindowController.OnMuteAllToggled(isMute);

        // Assert
        Assert.AreEqual(isMute, voiceChatWindowController.isMuteAll);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MuteAllCorrectly(bool isMuted)
    {
        // Arrange
        voiceChatWindowController.isMuteAll = !isMuted;
        voiceChatWindowController.usersToUnmute.Add("test");
        voiceChatWindowController.usersToMute.Add("test");

        // Act
        voiceChatWindowController.MuteAll(isMuted);

        // Assert
        Assert.AreEqual(isMuted, voiceChatWindowController.isMuteAll);

        if (isMuted)
            Assert.AreEqual(0, voiceChatWindowController.usersToUnmute.Count);
        else
            Assert.AreEqual(0, voiceChatWindowController.usersToMute.Count);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MuteUserCorrectly(bool isMuted)
    {
        // Arrange
        string testPlayerId = "TestPlayerId";
        voiceChatWindowController.usersToUnmute.Clear();
        voiceChatWindowController.usersToMute.Clear();

        // Act
        voiceChatWindowController.MuteUser(testPlayerId, isMuted);

        // Assert
        if (isMuted)
        {
            Assert.IsTrue(voiceChatWindowController.usersToMute.Contains(testPlayerId));
            socialAnalytics.Received(1).SendPlayerMuted(testPlayerId);
        }
        else
        {
            Assert.IsTrue(voiceChatWindowController.usersToUnmute.Contains(testPlayerId));
            socialAnalytics.Received(1).SendPlayerUnmuted(testPlayerId);
        }
    }

    [Test]
    [TestCase(VoiceChatAllow.ALL_USERS)]
    [TestCase(VoiceChatAllow.FRIENDS_ONLY)]
    [TestCase(VoiceChatAllow.VERIFIED_ONLY)]
    public void RaiseChangeAllowUsersFilterCorrectly(VoiceChatAllow optionId)
    {
        // Act
        voiceChatWindowController.ChangeAllowUsersFilter(optionId.ToString());

        // Assert
        if (optionId == VoiceChatAllow.ALL_USERS)
            Assert.AreEqual(VoiceChatAllow.ALL_USERS, Settings.i.generalSettings.Data.voiceChatAllow);
        else if (optionId == VoiceChatAllow.VERIFIED_ONLY)
            Assert.AreEqual(VoiceChatAllow.VERIFIED_ONLY, Settings.i.generalSettings.Data.voiceChatAllow);
        else if (optionId == VoiceChatAllow.FRIENDS_ONLY)
            Assert.AreEqual(VoiceChatAllow.FRIENDS_ONLY, Settings.i.generalSettings.Data.voiceChatAllow);
    }

    [Test]
    public void RaiseOnUserProfileUpdatedCorrectly()
    {
        // Arrange
        string testUserId = "UserId1";
        voiceChatWindowController.trackedUsersHashSet.Add(testUserId);

        // Act
        voiceChatWindowController.OnUserProfileUpdated(userProfileBridge.Get("test"));

        // Assert
        voiceChatWindowComponentView.Received(1).SetPlayerBlocked(testUserId, Arg.Any<bool>());
    }

    [Test]
    [TestCase(FriendshipAction.APPROVED)]
    [TestCase(FriendshipAction.REJECTED)]
    public void RaiseOnUpdateFriendshipCorrectly(FriendshipAction action)
    {
        // Arrange
        string testUserId = "UserId";

        // Act
        voiceChatWindowController.OnUpdateFriendship(testUserId, action);

        // Assert
        voiceChatWindowComponentView.Received(1).SetPlayerAsFriend(testUserId, action == FriendshipAction.APPROVED);
    }

    [Test]
    [TestCase(VoiceChatAllow.ALL_USERS)]
    [TestCase(VoiceChatAllow.FRIENDS_ONLY)]
    [TestCase(VoiceChatAllow.VERIFIED_ONLY)]
    public void RaiseOnSettingsChangedCorrectly(VoiceChatAllow voiceChatAllow)
    {
        // Arrange
        var testSettings = Settings.i.generalSettings.Data;
        testSettings.voiceChatAllow = voiceChatAllow;

        // Act
        voiceChatWindowController.OnSettingsChanged(testSettings);

        // Assert
        switch (voiceChatAllow)
        {
            case VoiceChatAllow.ALL_USERS:
                voiceChatWindowComponentView.Received().SelectAllowUsersOption(0);
                break;
            case VoiceChatAllow.VERIFIED_ONLY:
                voiceChatWindowComponentView.Received().SelectAllowUsersOption(1);
                break;
            case VoiceChatAllow.FRIENDS_ONLY:
                voiceChatWindowComponentView.Received().SelectAllowUsersOption(2);
                break;
        }

        socialAnalytics.Received(1).SendVoiceChatPreferencesChanged(voiceChatAllow);
    }

    [Test]
    public void SetOwnPlayerIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = true;

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(true, VoiceChatWindowController.TALKING_MESSAGE_YOU);
    }

    [Test]
    public void SetJustYouIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = false;
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayers.Returns(info => 0);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(false, VoiceChatWindowController.TALKING_MESSAGE_JUST_YOU_IN_THE_VOICE_CHAT);
    }

    [Test]
    public void SetNobodyIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = false;
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayers.Returns(info => 1);
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayersTalking.Returns(info => 0);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(false, VoiceChatWindowController.TALKING_MESSAGE_NOBODY_TALKING);
    }

    [Test]
    public void SetOtherPlayerIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = false;
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayers.Returns(info => 1);
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayersTalking.Returns(info => 1);
        voiceChatWindowController.VoiceChatWindowView.Configure().GetUserTalkingByIndex(Arg.Any<int>()).Returns(info => "test");
        userProfileBridge.Configure().Get(Arg.Any<string>()).Returns(info => null);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(true, "test");
    }

    [Test]
    public void SetSeveralPlayersIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = false;
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayers.Returns(info => 1);
        voiceChatWindowController.VoiceChatWindowView.Configure().numberOfPlayersTalking.Returns(info => 2);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(true, VoiceChatWindowController.TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING);
    }
}
