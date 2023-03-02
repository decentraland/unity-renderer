using DCL.Guests.HUD.ConnectWallet;
using NUnit.Framework;

public class ConnectWalletComponentViewShould
{
    private ConnectWalletComponentView connectWalletComponentView;

    [SetUp]
    public void Setup()
    {
        connectWalletComponentView = ConnectWalletComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        connectWalletComponentView.Dispose();
    }

    [Test]
    public void ClickOnBackgroundCorrectly()
    {
        // Arrange
        bool isBackgroundClicked = false;
        connectWalletComponentView.OnCancel += () => isBackgroundClicked = true;

        // Act
        connectWalletComponentView.backgroundButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(isBackgroundClicked);
    }

    [Test]
    public void ClickOnCloseButtonCorrectly()
    {
        // Arrange
        bool isCancelClicked = false;
        connectWalletComponentView.OnCancel += () => isCancelClicked = true;

        // Act
        connectWalletComponentView.closeButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(isCancelClicked);
    }

    [Test]
    public void ClickOnConnectButtonCorrectly()
    {
        // Arrange
        bool isConnectClicked = false;
        connectWalletComponentView.OnConnect += () => isConnectClicked = true;

        // Act
        connectWalletComponentView.connectButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(isConnectClicked);
    }

    [Test]
    public void ClickOnHelpButtonCorrectly()
    {
        // Arrange
        bool isHelpClicked = false;
        connectWalletComponentView.OnHelp += () => isHelpClicked = true;

        // Act
        connectWalletComponentView.helpButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(isHelpClicked);
    }
}
