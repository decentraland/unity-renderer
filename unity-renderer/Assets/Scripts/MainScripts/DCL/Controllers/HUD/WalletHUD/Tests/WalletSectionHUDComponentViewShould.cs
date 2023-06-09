using NUnit.Framework;
using System.Collections;
using System.Globalization;
using UnityEngine.TestTools;

namespace DCL.Wallet
{
    public class WalletSectionHUDComponentViewShould
    {
        private WalletSectionHUDComponentView walletSectionHUDView;

        [SetUp]
        public void SetUp()
        {
            walletSectionHUDView = WalletSectionHUDComponentView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            walletSectionHUDView.Dispose();
        }

        [Test]
        public void RaiseOnBuyManaClickedFromEthereumButtonCorrectly()
        {
            // Arrange
            var buyManaClicked = false;
            walletSectionHUDView.OnBuyManaClicked += () => buyManaClicked = true;

            // Act
            walletSectionHUDView.buyEthereumManaButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(buyManaClicked);
        }

        [Test]
        public void RaiseOnBuyManaClickedFromPolygonButtonCorrectly()
        {
            // Arrange
            var buyManaClicked = false;
            walletSectionHUDView.OnBuyManaClicked += () => buyManaClicked = true;

            // Act
            walletSectionHUDView.buyPolygonManaButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(buyManaClicked);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetWalletSectionAsGuestCorrectly(bool isGuest)
        {
            // Act
            walletSectionHUDView.SetWalletSectionAsGuest(isGuest);

            // Assert
            Assert.AreEqual(!isGuest, walletSectionHUDView.signedInWalletContainer.gameObject.activeSelf);
            Assert.AreEqual(isGuest, walletSectionHUDView.guestContainer.gameObject.activeSelf);
        }

        [Test]
        public void SetWalletAddressCorrectly()
        {
            // Arrange
            const string TEST_ADDRESS = "testAddress";

            // Act
            walletSectionHUDView.SetWalletAddress(TEST_ADDRESS);

            // Assert
            Assert.AreEqual(TEST_ADDRESS, walletSectionHUDView.walletAddressText.text);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEthereumManaLoadingActiveCorrectly(bool isActive)
        {
            // Act
            walletSectionHUDView.SetEthereumManaLoadingActive(isActive);

            // Assert
            Assert.AreEqual(!isActive, walletSectionHUDView.ethereumManaBalanceText.gameObject.activeSelf);
            Assert.AreEqual(isActive, walletSectionHUDView.ethereumManaBalanceLoading.gameObject.activeSelf);
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetEthereumManaBalanceCorrectly(double balance)
        {
            // Act
            walletSectionHUDView.SetEthereumManaBalance(balance);

            // Assert
            Assert.AreEqual(balance.ToString(CultureInfo.InvariantCulture), walletSectionHUDView.ethereumManaBalanceText.text);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPolygonManaLoadingActiveCorrectly(bool isActive)
        {
            // Act
            walletSectionHUDView.SetPolygonManaLoadingActive(isActive);

            // Assert
            Assert.AreEqual(!isActive, walletSectionHUDView.polygonManaBalanceText.gameObject.activeSelf);
            Assert.AreEqual(isActive, walletSectionHUDView.polygonManaBalanceLoading.gameObject.activeSelf);
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetPolygonManaBalanceCorrectly(double balance)
        {
            // Act
            walletSectionHUDView.SetPolygonManaBalance(balance);

            // Assert
            Assert.AreEqual(balance.ToString(CultureInfo.InvariantCulture), walletSectionHUDView.polygonManaBalanceText.text);
        }

        [UnityTest]
        public IEnumerator CopyWalletAddressCorrectly()
        {
            // Arrange
            var walletCopied = false;
            walletSectionHUDView.OnCopyWalletAddress += () => walletCopied = true;

            // Act
            walletSectionHUDView.copyWalletAddressButton.onClick.Invoke();
            yield return null;

            // Assert
            Assert.IsTrue(walletSectionHUDView.copyWalletAddressToast.gameObject.activeSelf);
            Assert.IsTrue(walletCopied);
        }
    }
}
