using DCL;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
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
    private DataStore dataStore;
    private string[] testPlayersId = { "playerId1", "playerId2", "playerId3" };

    [SetUp]
    public void SetUp()
    {
        voiceChatWindowComponentView = Substitute.For<IVoiceChatWindowComponentView>();
        voiceChatWindowComponentView.Configure().CreateNewPlayerInstance().Returns(info => voiceChatWindowController.CreateVoiceChatPlayerView());
        voiceChatBarComponentView = Substitute.For<IVoiceChatBarComponentView>();
        voiceChatWindowController = Substitute.ForPartsOf<VoiceChatWindowController>();
        voiceChatWindowController.Configure().CreateVoiceChatWindowView().Returns(info => voiceChatWindowComponentView);
        voiceChatWindowController.Configure().CreateVoiceChatBatView().Returns(info => voiceChatBarComponentView);
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.Configure().GetOwn().Returns(info => ScriptableObject.CreateInstance<UserProfile>());
        friendsController = Substitute.For<IFriendsController>();
        friendsController.Configure().GetFriends().Returns(info => new Dictionary<string, FriendsController.UserStatus>());
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        dataStore = new DataStore();

        Settings.CreateSharedInstance(new DefaultSettingsFactory());
        voiceChatWindowController.Initialize(userProfileBridge, friendsController, socialAnalytics, dataStore, Settings.i);
        voiceChatWindowController.currentPlayers = CreateTestCurrentPlayers();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestCurrentPlayers();
        GameObject.Destroy(voiceChatWindowComponentView.ContextMenuPanel);
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
            voiceChatWindowComponentView.Received(1).Hide();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUsersMutedCorrectly(bool isMuted)
    {
        // Act
        voiceChatWindowController.SetUsersMuted(testPlayersId, isMuted);

        // Assert
        Assert.AreEqual(isMuted, voiceChatWindowController.currentPlayers[testPlayersId[0]].model.isMuted);
        Assert.AreEqual(isMuted, voiceChatWindowController.currentPlayers[testPlayersId[1]].model.isMuted);
        Assert.AreEqual(isMuted, voiceChatWindowController.currentPlayers[testPlayersId[2]].model.isMuted);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUserRecordingCorrectly(bool isRecording)
    {
        foreach (string playerId in testPlayersId)
        {
            // Act
            voiceChatWindowController.SetUserRecording(playerId, isRecording);

            // Assert
            Assert.AreEqual(isRecording, voiceChatWindowController.currentPlayers[playerId].model.isTalking);
        }
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
        voiceChatWindowComponentView.Received(1).Hide();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void JoinVoiceChatCorrectly(bool isJoined)
    {
        // Arrange
        dataStore.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, true), false);
        voiceChatWindowController.isOwnPLayerTalking = true;
        voiceChatWindowController.isJoined = !isJoined;

        // Act
        voiceChatWindowController.JoinVoiceChat(isJoined);

        // Assert
        foreach (string playerId in testPlayersId)
        {
            Assert.AreEqual(isJoined, voiceChatWindowController.currentPlayers[playerId].model.isJoined);
        }

        voiceChatWindowComponentView.Received(1).SetAsJoined(isJoined);

        if (isJoined)
        {
            voiceChatBarComponentView.Received(1).Show();
        }
        else
        {
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Key);
            Assert.IsFalse(dataStore.voiceChat.isRecording.Get().Value);
            Assert.IsFalse(voiceChatWindowController.isOwnPLayerTalking);
            voiceChatBarComponentView.Received(1).Hide();
        }

        Assert.AreEqual(isJoined, voiceChatWindowController.isJoined);

        if (!isJoined)
            socialAnalytics.Received(1).SendVoiceChannelDisconnection();
        else
            socialAnalytics.Received(1).SendVoiceChannelConnection(testPlayersId.Length);
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

        UserProfile testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(new UserProfileModel
        {
            userId = testPlayerId,
            name = testPlayerName
        });

        UserProfileController.userProfilesCatalog.Add(testPlayerId, testUserProfile);

        // Act
        voiceChatWindowController.OnOtherPlayersStatusAdded(testPlayerId, testPlayer);

        // Assert
        Assert.IsTrue(voiceChatWindowController.currentPlayers.ContainsKey(testPlayerId));
        Assert.AreEqual(testPlayerId, voiceChatWindowController.currentPlayers[testPlayerId].model.userId);
        Assert.AreEqual(testPlayerName, voiceChatWindowController.currentPlayers[testPlayerId].model.userName);
        Assert.IsTrue(voiceChatWindowController.currentPlayers[testPlayerId].gameObject.activeSelf);
        voiceChatWindowComponentView.Received(1).SetNumberOfPlayers(testPlayersId.Length + 1);
    }

    [Test]
    public void RaiseOnOtherPlayerStatusRemovedCorrectly()
    {
        // Act
        voiceChatWindowController.OnOtherPlayerStatusRemoved(testPlayersId[0], null);

        // Assert
        Assert.IsFalse(voiceChatWindowController.currentPlayers.ContainsKey(testPlayersId[0]));
        voiceChatWindowComponentView.Received(1).SetNumberOfPlayers(testPlayersId.Length - 1);
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
        voiceChatWindowController.usersToUnmute.Clear();
        voiceChatWindowController.usersToMute.Clear();

        // Act
        voiceChatWindowController.MuteUser(testPlayersId[1], isMuted);

        // Assert
        if (isMuted)
        {
            Assert.IsTrue(voiceChatWindowController.usersToMute.Contains(testPlayersId[1]));
            socialAnalytics.Received(1).SendPlayerMuted(testPlayersId[1]);
        }
        else
        {
            Assert.IsTrue(voiceChatWindowController.usersToUnmute.Contains(testPlayersId[1]));
            socialAnalytics.Received(1).SendPlayerUnmuted(testPlayersId[1]);
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
                voiceChatWindowComponentView.Received(1).SelectAllowUsersOption(0);
                break;
            case VoiceChatAllow.VERIFIED_ONLY:
                voiceChatWindowComponentView.Received(1).SelectAllowUsersOption(1);
                break;
            case VoiceChatAllow.FRIENDS_ONLY:
                voiceChatWindowComponentView.Received(1).SelectAllowUsersOption(2);
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
        DestroyTestCurrentPlayers();

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
        voiceChatWindowController.usersTalking.Clear();

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
        voiceChatWindowController.usersTalking.Add(testPlayersId[0]);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(true, testPlayersId[0]);
    }

    [Test]
    public void SetSeveralPlayersIsTalkingCorrectly()
    {
        // Arrange
        voiceChatWindowController.isOwnPLayerTalking = false;
        voiceChatWindowController.usersTalking.Add(testPlayersId[0]);
        voiceChatWindowController.usersTalking.Add(testPlayersId[1]);

        // Act
        voiceChatWindowController.SetWhichPlayerIsTalking();

        // Assert
        voiceChatBarComponentView.Received(1).SetTalkingMessage(true, VoiceChatWindowController.TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING);
    }

    private Dictionary<string, VoiceChatPlayerComponentView> CreateTestCurrentPlayers()
    {
        Dictionary<string, VoiceChatPlayerComponentView> result = new Dictionary<string, VoiceChatPlayerComponentView>();
        foreach (string playerId in testPlayersId)
        {
            result.Add(playerId, voiceChatWindowController.CreateVoiceChatPlayerView() as VoiceChatPlayerComponentView);
        }

        return result;
    }

    private void DestroyTestCurrentPlayers()
    {
        foreach (var player in voiceChatWindowController.currentPlayers)
        {
            GameObject.Destroy(player.Value.gameObject);
        }
        voiceChatWindowController.currentPlayers.Clear();
    }
}
