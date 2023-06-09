using NUnit.Framework;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace DCL.Wallet
{
    public class WalletCardHUDComponentViewShould
    {
        private WalletCardHUDComponentView walletCardHUDView;

        [SetUp]
        public void SetUp()
        {
            WalletCardHUDComponentView prefab = AssetDatabase.LoadAssetAtPath<WalletCardHUDComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/WalletHUD/Prefabs/WalletCard.prefab");

            walletCardHUDView = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            walletCardHUDView.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetWalletCardActiveCorrectly(bool isActive)
        {
            // Act
            walletCardHUDView.SetWalletCardActive(isActive);

            // Assert
            Assert.AreEqual(isActive, walletCardHUDView.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetWalletCardAsGuestCorrectly(bool isGuest)
        {
            // Act
            walletCardHUDView.SetWalletCardAsGuest(isGuest);

            // Assert
            Assert.AreEqual(isGuest, walletCardHUDView.guestButton.gameObject.activeSelf);
            Assert.AreEqual(!isGuest, walletCardHUDView.signedInWalletButton.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEthereumManaLoadingActiveCorrectly(bool isActive)
        {
            // Act
            walletCardHUDView.SetEthereumManaLoadingActive(isActive);

            // Assert
            Assert.AreEqual(!isActive, walletCardHUDView.ethereumManaBalanceText.gameObject.activeSelf);
            Assert.AreEqual(isActive, walletCardHUDView.ethereumManaBalanceLoading.gameObject.activeSelf);
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetEthereumManaBalanceCorrectly(double balance)
        {
            // Act
            walletCardHUDView.SetEthereumManaBalance(balance);

            // Assert
            Assert.AreEqual(balance.ToString(CultureInfo.InvariantCulture), walletCardHUDView.ethereumManaBalanceText.text);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPolygonManaLoadingActiveCorrectly(bool isActive)
        {
            // Act
            walletCardHUDView.SetPolygonManaLoadingActive(isActive);

            // Assert
            Assert.AreEqual(!isActive, walletCardHUDView.polygonManaBalanceText.gameObject.activeSelf);
            Assert.AreEqual(isActive, walletCardHUDView.polygonManaBalanceLoading.gameObject.activeSelf);
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetPolygonManaBalanceCorrectly(double balance)
        {
            // Act
            walletCardHUDView.SetPolygonManaBalance(balance);

            // Assert
            Assert.AreEqual(balance.ToString(CultureInfo.InvariantCulture), walletCardHUDView.polygonManaBalanceText.text);
        }
    }
}
