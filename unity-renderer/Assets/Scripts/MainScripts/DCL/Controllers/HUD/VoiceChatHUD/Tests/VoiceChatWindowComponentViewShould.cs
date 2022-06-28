using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowComponentViewShould
{
    private VoiceChatWindowComponentView voiceChatWindowComponentView;
    private string[] testPlayersId = { "playerId1", "playerId2", "playerId3" };

    [SetUp]
    public void SetUp()
    {
        voiceChatWindowComponentView = BaseComponentView.Create<VoiceChatWindowComponentView>("SocialBarV1/VoiceChatHUD");
        voiceChatWindowComponentView.currentPlayers = CreateTestCurrentPlayers();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestCurrentPlayers();
        voiceChatWindowComponentView.Dispose();
    }

    [Test]
    public void ConfigureCorrectly()
    {
        // Arrange
        VoiceChatWindowComponentModel testModel = new VoiceChatWindowComponentModel
        {
            isJoined = true,
            numberOfPlayers = 10
        };

        // Act
        voiceChatWindowComponentView.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, voiceChatWindowComponentView.model);
    }

    [Test]
    public void ShowCorrectly()
    {
        // Arrange
        voiceChatWindowComponentView.gameObject.SetActive(false);

        // Act
        voiceChatWindowComponentView.Show();

        // Assert
        Assert.IsTrue(voiceChatWindowComponentView.gameObject.activeSelf);
    }

    [Test]
    public void HideCorrectly()
    {
        // Arrange
        voiceChatWindowComponentView.gameObject.SetActive(true);

        // Act
        voiceChatWindowComponentView.Hide();

        // Assert
        Assert.IsFalse(voiceChatWindowComponentView.gameObject.activeSelf);
    }

    [Test]
    [TestCase(0)]
    [TestCase(8)]
    public void SetNumberOfPlayersCorrectly(int testNumberOfPlayers)
    {
        // Arrange
        voiceChatWindowComponentView.model.numberOfPlayers = 0;
        voiceChatWindowComponentView.playersText.text = "";
        voiceChatWindowComponentView.playersText.gameObject.SetActive(testNumberOfPlayers == 0);
        voiceChatWindowComponentView.emptyListGameObject.SetActive(testNumberOfPlayers > 0);

        // Act
        voiceChatWindowComponentView.SetNumberOfPlayers(testNumberOfPlayers);

        // Assert
        Assert.AreEqual(testNumberOfPlayers, voiceChatWindowComponentView.model.numberOfPlayers);
        Assert.AreEqual($"PLAYERS ({testNumberOfPlayers})", voiceChatWindowComponentView.playersText.text);
        Assert.AreEqual(testNumberOfPlayers > 0, voiceChatWindowComponentView.playersText.gameObject.activeSelf);
        Assert.AreEqual(testNumberOfPlayers == 0, voiceChatWindowComponentView.emptyListGameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsJoinedCorrectly(bool isJoined)
    {
        // Arrange
        voiceChatWindowComponentView.model.isJoined = !isJoined;
        voiceChatWindowComponentView.joinButton.gameObject.SetActive(isJoined);
        voiceChatWindowComponentView.leaveButton.gameObject.SetActive(!isJoined);

        // Act
        voiceChatWindowComponentView.SetAsJoined(isJoined);

        // Assert
        foreach (string playerId in testPlayersId)
        {
            Assert.AreEqual(isJoined, voiceChatWindowComponentView.currentPlayers[playerId].model.isJoined);
        }

        Assert.AreEqual(isJoined, voiceChatWindowComponentView.model.isJoined);
        Assert.AreEqual(!isJoined, voiceChatWindowComponentView.joinButton.gameObject.activeSelf);
        Assert.AreEqual(isJoined, voiceChatWindowComponentView.leaveButton.gameObject.activeSelf);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public void SelectAllowUsersOptionCorrectly(int optionIndex)
    {
        // Arrange
        voiceChatWindowComponentView.ConfigureAllowUsersFilter();
        voiceChatWindowComponentView.allowUsersDropdown.GetOption(optionIndex).isOn = false;

        // Act
        voiceChatWindowComponentView.SelectAllowUsersOption(optionIndex);

        // Assert
        Assert.IsTrue(voiceChatWindowComponentView.allowUsersDropdown.GetOption(optionIndex).isOn);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlayerMutedCorrectly(bool isMuted)
    {
        // Act
        voiceChatWindowComponentView.SetPlayerMuted(testPlayersId[1], isMuted);

        // Assert
        Assert.AreEqual(isMuted, voiceChatWindowComponentView.currentPlayers[testPlayersId[1]].model.isMuted);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlayerRecordingCorrectly(bool isRecording)
    {
        foreach (string playerId in testPlayersId)
        {
            // Act
            voiceChatWindowComponentView.SetPlayerRecording(playerId, isRecording);

            // Assert
            Assert.AreEqual(isRecording, voiceChatWindowComponentView.currentPlayers[playerId].model.isTalking);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlayerBlockedCorrectly(bool isBlocked)
    {
        foreach (string playerId in testPlayersId)
        {
            // Act
            voiceChatWindowComponentView.SetPlayerBlocked(playerId, isBlocked);

            // Assert
            Assert.AreEqual(isBlocked, voiceChatWindowComponentView.currentPlayers[playerId].model.isBlocked);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlayerAsFriendCorrectly(bool isFriend)
    {
        foreach (string playerId in testPlayersId)
        {
            // Act
            voiceChatWindowComponentView.SetPlayerAsFriend(playerId, isFriend);

            // Assert
            Assert.AreEqual(isFriend, voiceChatWindowComponentView.currentPlayers[playerId].model.isFriend);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlayerAsJoinedCorrectly(bool isJoined)
    {
        foreach (string playerId in testPlayersId)
        {
            // Act
            voiceChatWindowComponentView.SetPlayerAsJoined(playerId, isJoined);

            // Assert
            Assert.AreEqual(isJoined, voiceChatWindowComponentView.currentPlayers[playerId].model.isJoined);
        }
    }

    [Test]
    public void AddOrUpdatePlayerCorrectly()
    {
        // Arrange
        string testPlayerId = "playerId4";
        string testPlayerName = "Test Player";

        UserProfile testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(new UserProfileModel
        {
            userId = testPlayerId,
            name = testPlayerName
        });

        UserProfileController.userProfilesCatalog.Add(testPlayerId, testUserProfile);

        // Act
        voiceChatWindowComponentView.AddOrUpdatePlayer(testUserProfile);

        // Assert
        Assert.IsTrue(voiceChatWindowComponentView.currentPlayers.ContainsKey(testPlayerId));
        Assert.AreEqual(testPlayerId, voiceChatWindowComponentView.currentPlayers[testPlayerId].model.userId);
        Assert.AreEqual(testPlayerName, voiceChatWindowComponentView.currentPlayers[testPlayerId].model.userName);
        Assert.IsTrue(voiceChatWindowComponentView.currentPlayers[testPlayerId].gameObject.activeSelf);
    }

    [Test]
    public void RemoveUserCorrectly()
    {
        // Act
        voiceChatWindowComponentView.RemoveUser(testPlayersId[0]);

        // Assert
        Assert.IsFalse(voiceChatWindowComponentView.currentPlayers.ContainsKey(testPlayersId[0]));
    }

    [Test]
    public void GetUserTalkingByIndexCorrectly()
    {
        // Arrange
        string resultPlayer = "";
        voiceChatWindowComponentView.usersTalking.Add(testPlayersId[0]);
        voiceChatWindowComponentView.usersTalking.Add(testPlayersId[1]);

        // Act
        resultPlayer = voiceChatWindowComponentView.GetUserTalkingByIndex(1);

        // Assert
        Assert.AreEqual(testPlayersId[1], resultPlayer);
    }

    [Test]
    public void ConfigureAllowUsersFilterCorrectly()
    {
        // Act
        voiceChatWindowComponentView.ConfigureAllowUsersFilter();

        // Assert
        Assert.AreEqual(VoiceChatAllow.ALL_USERS.ToString(), voiceChatWindowComponentView.allowUsersDropdown.GetOption(0).id);
        Assert.AreEqual(VoiceChatAllow.VERIFIED_ONLY.ToString(), voiceChatWindowComponentView.allowUsersDropdown.GetOption(1).id);
        Assert.AreEqual(VoiceChatAllow.FRIENDS_ONLY.ToString(), voiceChatWindowComponentView.allowUsersDropdown.GetOption(2).id);
    }

    [Test]
    public void RaiseAllowUsersOptionChangedCorrectly()
    {
        // Arrange
        string testId = "TestId";
        string testOptionName = "Test Name";
        string resultOptionId = "";
        voiceChatWindowComponentView.OnAllowUsersFilterChange += (optionId) => resultOptionId = optionId;

        // Act
        voiceChatWindowComponentView.AllowUsersOptionChanged(true, testId, testOptionName);

        // Assert
        Assert.AreEqual(testOptionName, voiceChatWindowComponentView.allowUsersDropdown.model.title);
        Assert.AreEqual(testId, resultOptionId);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnMuteAllToggleChangedCorrectly(bool isOn)
    {
        // Arrange
        bool resultIsOn = false;
        voiceChatWindowComponentView.OnMuteAll += (isOn) => resultIsOn = isOn;

        // Act
        voiceChatWindowComponentView.OnMuteAllToggleChanged(isOn, "", "");

        // Assert
        Assert.AreEqual(isOn, resultIsOn);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MuteUserCorrectly(bool isMute)
    {
        // Arrange
        string resultUserId = "";
        bool resultIsMute = false;
        voiceChatWindowComponentView.OnMuteUser += (userId, isMute) =>
        {
            resultUserId = userId;
            resultIsMute = isMute;
        };

        // Act
        voiceChatWindowComponentView.MuteUser(testPlayersId[1], isMute);

        // Assert
        Assert.AreEqual(testPlayersId[1], resultUserId);
        Assert.AreEqual(isMute, resultIsMute);
    }

    private Dictionary<string, VoiceChatPlayerComponentView> CreateTestCurrentPlayers()
    {
        Dictionary<string, VoiceChatPlayerComponentView> result = new Dictionary<string, VoiceChatPlayerComponentView>();
        foreach (string playerId in testPlayersId)
        {
            result.Add(playerId, VoiceChatPlayerComponentView.Create() as VoiceChatPlayerComponentView);
        }

        return result;
    }

    private void DestroyTestCurrentPlayers()
    {
        foreach (var player in voiceChatWindowComponentView.currentPlayers)
        {
            GameObject.Destroy(player.Value.gameObject);
        }

        voiceChatWindowComponentView.currentPlayers.Clear();
    }
}
