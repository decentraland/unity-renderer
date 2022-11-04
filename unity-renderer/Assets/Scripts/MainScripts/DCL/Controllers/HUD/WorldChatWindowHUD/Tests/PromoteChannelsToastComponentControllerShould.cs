using DCL;
using DCL.Chat.HUD;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;

public class PromoteChannelsToastComponentControllerShould
{
    private PromoteChannelsToastComponentController promoteChannelsToastController;
    private IPromoteChannelsToastComponentView promoteChannelsToastView;
    private IPlayerPrefs playerPrefs;
    private DataStore dataStore;
    private RendererState rendererState;

    [SetUp]
    public void SetUp()
    {
        promoteChannelsToastView = Substitute.For<IPromoteChannelsToastComponentView>();
        playerPrefs = Substitute.For<IPlayerPrefs>();
        dataStore = new DataStore();
        rendererState = new RendererState();
        
        promoteChannelsToastController = new PromoteChannelsToastComponentController(
            promoteChannelsToastView,
            playerPrefs,
            dataStore,
            rendererState);
}

    [TearDown]
    public void TearDown()
    {
        promoteChannelsToastController.Dispose();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShowToastCorrectly(bool alreadyDismissed)
    {
        // Arrange
        playerPrefs.GetBool(PromoteChannelsToastComponentController.PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, false)
            .Returns(alreadyDismissed);

        // Action
        rendererState.Set(true);

        // Assert
        if (alreadyDismissed)
        {
            Assert.IsFalse(dataStore.channels.isPromoteToastVisible.Get());
            promoteChannelsToastView.Received().Hide();
        }
        else
        {
            Assert.IsTrue(dataStore.channels.isPromoteToastVisible.Get());
            promoteChannelsToastView.Received().Show();
        }
    }

    [Test]
    public void DismissToastCorrectly()
    {
        // Action
        promoteChannelsToastView.OnClose += Raise.Event<Action>();

        // Assert
        Assert.IsFalse(dataStore.channels.isPromoteToastVisible.Get());
        promoteChannelsToastView.Received().Hide();
        playerPrefs.Received().Set(PromoteChannelsToastComponentController.PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, true);
        playerPrefs.Received().Save();

    }
}
