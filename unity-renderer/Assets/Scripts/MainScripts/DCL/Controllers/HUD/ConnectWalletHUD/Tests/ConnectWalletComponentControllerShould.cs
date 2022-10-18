using DCL;
using DCL.Browser;
using DCL.Guests.HUD.ConnectWallet;
using NSubstitute;
using NUnit.Framework;
using System;

public class ConnectWalletComponentControllerShould
{
    private ConnectWalletComponentController connectWalletComponentController;
    private IConnectWalletComponentView connectWalletView;
    private IBrowserBridge browserBridge;
    private IUserProfileBridge userProfileBridge;
    private DataStore dataStore;

    private BaseVariable<bool> connectWalletModalVisible => dataStore.HUDs.connectWalletModalVisible;

    [SetUp]
    public void SetUp()
    {
        connectWalletView = Substitute.For<IConnectWalletComponentView>();
        browserBridge = Substitute.For<IBrowserBridge>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        dataStore = new DataStore();

        connectWalletComponentController = new ConnectWalletComponentController(
            connectWalletView,
            browserBridge,
            userProfileBridge,
            dataStore);
    }

    [TearDown]
    public void TearDown()
    {
        connectWalletComponentController.Dispose();
    }

    [Test]
    public void RaiseOnCancelWalletConnectionCorrectly()
    {
        // Act
        connectWalletModalVisible.Set(true);
        connectWalletView.OnCancel += Raise.Event<Action>();

        // Assert
        connectWalletView.Received(1).Hide();
        Assert.IsFalse(connectWalletModalVisible.Get());
    }

    [Test]
    public void RaiseOnConfirmWalletConnectionCorrectly()
    {
        // Act
        connectWalletModalVisible.Set(true);
        connectWalletView.OnConnect += Raise.Event<Action>();

        // Assert
        connectWalletView.Received(1).Hide();
        Assert.IsFalse(connectWalletModalVisible.Get());
        userProfileBridge.Received(1).SignUp();
    }

    [Test]
    public void RaiseOnConfirmHelpCorrectly()
    {
        // Act
        connectWalletView.OnHelp += Raise.Event<Action>();

        // Assert
        browserBridge.Received(1).OpenUrl(Arg.Any<string>());
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnChangeVisibilityCorrectly(bool isVisible)
    {
        // Act
        connectWalletModalVisible.Set(isVisible, notifyEvent: true);

        // Assert
        if (isVisible)
            connectWalletView.Received(1).Show();
        else
            connectWalletView.Received(1).Hide();
    }
}
