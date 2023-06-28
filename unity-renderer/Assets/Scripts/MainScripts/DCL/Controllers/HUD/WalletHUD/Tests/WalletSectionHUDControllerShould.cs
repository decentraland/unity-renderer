using DCL.Browser;
using DCL.Helpers;
using DCL.MyAccount;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCL.Wallet
{
    public class WalletSectionHUDControllerShould
    {
        private const float TEST_ETHEREUM_MANA_BALANCE = 50f;
        private const float TEST_POLYGON_MANA_BALANCE = 124f;

        private WalletSectionHUDController walletSectionHUDController;
        private IWalletSectionHUDComponentView walletSectionHUDComponentView;
        private IUserProfileBridge userProfileBridge;
        private ITheGraph theGraph;
        private DataStore dataStore;
        private UserProfile ownUserProfile;
        private IClipboard clipboard;
        private IBrowserBridge browserBridge;
        private IMyAccountAnalyticsService myAccountAnalyticsService;
        private Promise<double> requestEthereumManaPromise;
        private Promise<double> requestPolygonManaPromise;

        [SetUp]
        public void SetUp()
        {
            walletSectionHUDComponentView = Substitute.For<IWalletSectionHUDComponentView>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            theGraph = Substitute.For<ITheGraph>();
            clipboard = Substitute.For<IClipboard>();
            browserBridge = Substitute.For<IBrowserBridge>();
            dataStore = new DataStore();
            myAccountAnalyticsService = Substitute.For<IMyAccountAnalyticsService>();

            ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel { userId = "ownId" });
            userProfileBridge.Configure().GetOwn().Returns(ownUserProfile);

            requestEthereumManaPromise = new Promise<double>();
            requestEthereumManaPromise.Resolve(TEST_ETHEREUM_MANA_BALANCE);
            theGraph.Configure().QueryMana(Arg.Any<string>(), TheGraphNetwork.Ethereum).Returns(requestEthereumManaPromise);
            requestPolygonManaPromise = new Promise<double>();
            requestPolygonManaPromise.Resolve(TEST_POLYGON_MANA_BALANCE);
            theGraph.Configure().QueryMana(Arg.Any<string>(), TheGraphNetwork.Polygon).Returns(requestPolygonManaPromise);

            walletSectionHUDController = new WalletSectionHUDController(
                walletSectionHUDComponentView,
                dataStore,
                userProfileBridge,
                clipboard,
                browserBridge,
                theGraph,
                myAccountAnalyticsService);
        }

        [TearDown]
        public void TearDown()
        {
            requestEthereumManaPromise.Dispose();
            requestPolygonManaPromise.Dispose();
            Object.Destroy(ownUserProfile);
            walletSectionHUDController.Dispose();
        }

        [TestCase(23f)]
        [TestCase(0f)]
        public void SetEthereumManaBalanceCorrectly(double balance)
        {
            // Act
            dataStore.wallet.currentEthereumManaBalance.Set(balance);

            // Assert
            walletSectionHUDComponentView.Received(1).SetEthereumManaBalance(balance);
        }

        [TestCase(23f)]
        [TestCase(0f)]
        public void SetPolygonManaBalanceCorrectly(double balance)
        {
            // Act
            dataStore.wallet.currentPolygonManaBalance.Set(balance);

            // Assert
            walletSectionHUDComponentView.Received(1).SetPolygonManaBalance(balance);
        }

        [UnityTest]
        public IEnumerator RequestEthereumManaBalanceCorrectly()
        {
            // Act
            dataStore.wallet.isWalletSectionVisible.Set(true, true);
            yield return null;

            // Assert
            walletSectionHUDComponentView.Received(1).SetEthereumManaLoadingActive(true);
            theGraph.Received(1).QueryMana(ownUserProfile.userId, TheGraphNetwork.Ethereum);
            yield return requestEthereumManaPromise;
            Assert.AreEqual(TEST_ETHEREUM_MANA_BALANCE, dataStore.wallet.currentEthereumManaBalance.Get());
            walletSectionHUDComponentView.Received(1).SetEthereumManaBalance(TEST_ETHEREUM_MANA_BALANCE);
            walletSectionHUDComponentView.Received(1).SetEthereumManaLoadingActive(false);
        }

        [UnityTest]
        public IEnumerator RequestPolygonManaBalanceCorrectly()
        {
            // Act
            dataStore.wallet.isWalletSectionVisible.Set(true, true);
            yield return null;

            // Assert
            walletSectionHUDComponentView.Received(1).SetPolygonManaLoadingActive(true);
            theGraph.Received(1).QueryMana(ownUserProfile.userId, TheGraphNetwork.Polygon);
            yield return requestPolygonManaPromise;
            Assert.AreEqual(TEST_POLYGON_MANA_BALANCE, dataStore.wallet.currentPolygonManaBalance.Get());
            walletSectionHUDComponentView.Received(1).SetPolygonManaBalance(TEST_POLYGON_MANA_BALANCE);
            walletSectionHUDComponentView.Received(1).SetPolygonManaLoadingActive(false);
        }

        [Test]
        public void UpdateProfileCorrectly()
        {
            // Assert
            walletSectionHUDComponentView.Received(1).SetWalletAddress(ownUserProfile.userId);
            walletSectionHUDComponentView.Received(1).SetWalletSectionAsGuest(ownUserProfile.isGuest);
        }

        [Test]
        public void CopyWalletAddressCorrectly()
        {
            // Act
            walletSectionHUDComponentView.OnCopyWalletAddress += Raise.Event<Action>();

            // Assert
            clipboard.Received(1).WriteText(ownUserProfile.userId);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GoToManaPurchaseUrlCorrectly(bool isPolygonNetwork)
        {
            // Act
            walletSectionHUDComponentView.OnBuyManaClicked += Raise.Event<Action<bool>>(isPolygonNetwork);

            // Assert
            browserBridge.Received(1).OpenUrl("https://account.decentraland.org?utm_source=dcl_explorer");
            myAccountAnalyticsService.Received(1).SendPlayerWalletBuyManaAnalytic(isPolygonNetwork);
        }

        [Test]
        public void GoToLearnMoreUrlCorrectly()
        {
            // Act
            walletSectionHUDComponentView.OnLearnMoreClicked += Raise.Event<Action>();

            // Assert
            browserBridge.Received(1).OpenUrl("https://docs.decentraland.org/examples/get-a-wallet?utm_source=dcl_explorer");
        }
    }
}
