using AvatarNamesHUD;
using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace Tests.AvatarNamesHUD
{
    public class AvatarNamessHUDControllerShould
    {
        private const int MAX_AVATAR_NAMES = 3;

        private AvatarNamesHUDController hudController;
        private IAvatarNamesHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarNamesHUDView>();
            hudController = Substitute.ForPartsOf<AvatarNamesHUDController>(MAX_AVATAR_NAMES);
            hudController.Configure().CreateView().Returns(info => hudView);
        }

        [Test]
        public void InitializeProperly()
        {
            hudController.Initialize();
            Assert.AreEqual(hudView, hudController.view);
            hudView.Received().Initialize(MAX_AVATAR_NAMES);
        }

        [Test]
        public void InitializeWithDataProperly()
        {
            DataStore.i.player.otherPlayersStatus.Add("user0", new PlayerStatus { id = "user0", name = "user0" });
            DataStore.i.player.otherPlayersStatus.Add("user1", new PlayerStatus { id = "user1", name = "user1" });
            DataStore.i.player.otherPlayersStatus.Add("user2", new PlayerStatus { id = "user2", name = "user2" });
            DataStore.i.player.otherPlayersStatus.Add("user3", new PlayerStatus { id = "user3", name = "user3" });

            hudController.Initialize();

            Assert.AreEqual(3, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user0"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user2"));
            Assert.AreEqual(1, hudController.reservePlayers.Count);
            Assert.IsTrue(hudController.reservePlayers.Contains("user3"));
        }

        [Test]
        public void ReactToDataStoreChanges()
        {
            hudController.Initialize();
            var user0 = new PlayerStatus { id = "user0", name = "user0" };
            var user1 = new PlayerStatus { id = "user1", name = "user1" };
            DataStore.i.player.otherPlayersStatus.Add("user0", user0);
            DataStore.i.player.otherPlayersStatus.Add("user1", user1);

            hudController.Received().OnOtherPlayersStatusAdded("user0", user0);
            hudController.Received().OnOtherPlayersStatusAdded("user1", user1);

            hudController.ClearReceivedCalls();
            DataStore.i.player.otherPlayersStatus.Remove("user0");
            hudController.Received().OnOtherPlayersStatusRemoved("user0", user0);
        }

        [Test]
        public void TrackPlayersProperly()
        {
            hudController.Initialize();

            var user0 = new PlayerStatus { id = "user0", name = "user0" };
            var user1 = new PlayerStatus { id = "user1", name = "user1" };
            hudController.OnOtherPlayersStatusAdded("user0", user0);
            hudController.OnOtherPlayersStatusAdded("user1", user1);

            Assert.AreEqual(2, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user0"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            hudView.Received().TrackPlayer(user0);
            hudView.Received().TrackPlayer(user1);
        }

        [Test]
        public void TrackPlayersProperly_TrackingFull()
        {
            hudController.Initialize();
            hudController.trackingPlayers.Add("user0");
            hudController.trackingPlayers.Add("user1");
            hudController.trackingPlayers.Add("user2");

            var user3 = new PlayerStatus { id = "user3", name = "user3" };
            var user4 = new PlayerStatus { id = "user4", name = "user4" };
            hudController.OnOtherPlayersStatusAdded("user3", user3);
            hudController.OnOtherPlayersStatusAdded("user4", user4);

            Assert.AreEqual(2, hudController.reservePlayers.Count);
            Assert.IsTrue(hudController.reservePlayers.Contains("user3"));
            Assert.IsTrue(hudController.reservePlayers.Contains("user4"));
            hudView.DidNotReceive().TrackPlayer(user3);
            hudView.DidNotReceive().TrackPlayer(user4);
        }

        [Test]
        public void UntrackPlayersProperly_TrackedPlayer()
        {
            hudController.Initialize();
            hudController.trackingPlayers.Add("user0");
            hudController.trackingPlayers.Add("user1");
            hudController.trackingPlayers.Add("user2");
            hudController.reservePlayers.AddLast("user3");
            hudController.reservePlayers.AddLast("user4");

            hudController.OnOtherPlayersStatusRemoved("user0", new PlayerStatus { id = "user0", name = "user0" });

            hudView.Received().UntrackPlayer("user0");
            Assert.AreEqual(3, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user2"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user3"));
            Assert.AreEqual(1, hudController.reservePlayers.Count);
            Assert.IsTrue(hudController.reservePlayers.Contains("user4"));
        }

        [Test]
        public void UntrackPlayersProperly_ReservePlayer()
        {
            hudController.Initialize();
            hudController.trackingPlayers.Add("user0");
            hudController.trackingPlayers.Add("user1");
            hudController.trackingPlayers.Add("user2");
            hudController.reservePlayers.AddLast("user3");
            hudController.reservePlayers.AddLast("user4");

            hudController.OnOtherPlayersStatusRemoved("user3", new PlayerStatus { id = "user3", name = "user3" });

            hudView.DidNotReceive().UntrackPlayer("user3");
            Assert.AreEqual(3, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user0"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user2"));
            Assert.AreEqual(1, hudController.reservePlayers.Count);
            Assert.IsTrue(hudController.reservePlayers.Contains("user4"));
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }

}