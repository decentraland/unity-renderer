using NUnit.Framework;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowComponentViewShould
{
    private VoiceChatWindowComponentView voiceChatWindowComponentView;

    [SetUp]
    public void SetUp()
    {
        voiceChatWindowComponentView = BaseComponentView.Create<VoiceChatWindowComponentView>("VoiceChatHUD");
    }

    [TearDown]
    public void TearDown()
    {
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
        Assert.AreEqual(isJoined, voiceChatWindowComponentView.model.isJoined);
        Assert.AreEqual(!isJoined, voiceChatWindowComponentView.joinButton.gameObject.activeSelf);
        Assert.AreEqual(isJoined, voiceChatWindowComponentView.leaveButton.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetMuteAllIsOnCorrectly(bool isOn)
    {
        // Arrange
        voiceChatWindowComponentView.muteAllToggle.isOn = !isOn;

        // Act
        voiceChatWindowComponentView.SetMuteAllIsOn(isOn);

        // Assert
        Assert.AreEqual(isOn, voiceChatWindowComponentView.muteAllToggle.isOn);
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
}
