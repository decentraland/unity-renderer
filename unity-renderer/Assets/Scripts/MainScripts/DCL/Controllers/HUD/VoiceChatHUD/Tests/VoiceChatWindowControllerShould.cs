using DCL;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections.Generic;

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
        voiceChatBarComponentView = Substitute.For<IVoiceChatBarComponentView>();
        voiceChatWindowController = Substitute.ForPartsOf<VoiceChatWindowController>();
        voiceChatWindowController.Configure().CreateVoiceChatWindowView().Returns(info => voiceChatWindowComponentView);
        voiceChatWindowController.Configure().CreateVoiceChatBatView().Returns(info => voiceChatBarComponentView);
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.Configure().GetOwn().Returns(info => new UserProfile());
        friendsController = Substitute.For<IFriendsController>();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        dataStore = new DataStore();

        Settings.CreateSharedInstance(new DefaultSettingsFactory());
        voiceChatWindowController.Initialize(userProfileBridge, friendsController, socialAnalytics, dataStore, Settings.i);
    }

    [TearDown]
    public void TearDown() { voiceChatWindowController.Dispose(); }

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
        // Arrange
        voiceChatWindowController.currentPlayers = GetTestCurrentPlayers();

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
        // Arrange
        voiceChatWindowController.currentPlayers = GetTestCurrentPlayers();

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

    private Dictionary<string, VoiceChatPlayerComponentView> GetTestCurrentPlayers()
    {
        Dictionary<string, VoiceChatPlayerComponentView> result = new Dictionary<string, VoiceChatPlayerComponentView>();
        foreach (string playerId in testPlayersId)
        {
            result.Add(playerId, voiceChatWindowController.CreateVoiceChatPlayerView() as VoiceChatPlayerComponentView);
        }

        return result;
    }
}
