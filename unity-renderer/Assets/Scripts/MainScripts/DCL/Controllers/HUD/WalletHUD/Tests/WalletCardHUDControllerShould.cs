using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Wallet
{
    public class WalletCardHUDControllerShould
    {
        private const float TEST_ETHEREUM_MANA_BALANCE = 50f;
        private const float TEST_POLYGON_MANA_BALANCE = 124f;

        private WalletCardHUDController walletCardHUDController;
        private IWalletCardHUDComponentView walletCardHUDComponentView;
        private IUserProfileBridge userProfileBridge;
        private ITheGraph theGraph;
        private DataStore dataStore;
        private UserProfile ownUserProfile;
        private Promise<double> requestEthereumManaPromise;
        private Promise<double> requestPolygonManaPromise;

        [SetUp]
        public void SetUp()
        {
            ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel { userId = "ownId" });
            walletCardHUDComponentView = Substitute.For<IWalletCardHUDComponentView>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            theGraph = Substitute.For<ITheGraph>();
            dataStore = new DataStore();

            userProfileBridge.Configure().GetOwn().Returns(ownUserProfile);

            requestEthereumManaPromise = new Promise<double>();
            requestEthereumManaPromise.Resolve(TEST_ETHEREUM_MANA_BALANCE);
            theGraph.Configure().QueryEthereumMana(Arg.Any<string>()).Returns(requestEthereumManaPromise);
            requestPolygonManaPromise = new Promise<double>();
            requestPolygonManaPromise.Resolve(TEST_POLYGON_MANA_BALANCE);
            theGraph.Configure().QueryPolygonMana(Arg.Any<string>()).Returns(requestPolygonManaPromise);

            walletCardHUDController = new WalletCardHUDController(
                walletCardHUDComponentView,
                userProfileBridge,
                theGraph,
                dataStore);
        }

        [TearDown]
        public void TearDown()
        {
            requestEthereumManaPromise.Dispose();
            Object.Destroy(ownUserProfile);
            walletCardHUDController.Dispose();
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetEthereumManaBalanceCorrectly(double balance)
        {
            // Act
            dataStore.wallet.currentEthereumManaBalance.Set(balance);

            // Assert
            walletCardHUDComponentView.Received(1).SetEthereumManaBalance(balance);
        }

        [Test]
        [TestCase(23f)]
        [TestCase(0f)]
        public void SetPolygonManaBalanceCorrectly(double balance)
        {
            // Act
            dataStore.wallet.currentPolygonManaBalance.Set(balance);

            // Assert
            walletCardHUDComponentView.Received(1).SetPolygonManaBalance(balance);
        }

        [UnityTest]
        public IEnumerator RequestEthereumManaBalanceCorrectly()
        {
            // Arrange
            dataStore.wallet.isWalletSectionVisible.Set(false, false);

            // Act
            dataStore.wallet.isWalletCardVisible.Set(true, true);
            yield return null;

            // Assert
            walletCardHUDComponentView.Received(1).SetEthereumManaLoadingActive(true);
            theGraph.Received(1).QueryEthereumMana(ownUserProfile.userId);
            yield return requestEthereumManaPromise;
            Assert.AreEqual(TEST_ETHEREUM_MANA_BALANCE, dataStore.wallet.currentEthereumManaBalance.Get());
            walletCardHUDComponentView.Received(1).SetEthereumManaBalance(TEST_ETHEREUM_MANA_BALANCE);
            walletCardHUDComponentView.Received(1).SetEthereumManaLoadingActive(false);
        }

        [UnityTest]
        public IEnumerator RequestPolygonManaBalanceCorrectly()
        {
            // Arrange
            dataStore.wallet.isWalletSectionVisible.Set(false, false);

            // Act
            dataStore.wallet.isWalletCardVisible.Set(true, true);
            yield return null;

            // Assert
            walletCardHUDComponentView.Received(1).SetPolygonManaLoadingActive(true);
            theGraph.Received(1).QueryPolygonMana(ownUserProfile.userId);
            yield return requestPolygonManaPromise;
            Assert.AreEqual(TEST_POLYGON_MANA_BALANCE, dataStore.wallet.currentPolygonManaBalance.Get());
            walletCardHUDComponentView.Received(1).SetPolygonManaBalance(TEST_POLYGON_MANA_BALANCE);
            walletCardHUDComponentView.Received(1).SetPolygonManaLoadingActive(false);
        }
    }
}
