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
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<IAvatarNamesHUDView>();
            hudController = Substitute.ForPartsOf<AvatarNamesHUDController>();
            hudController.Configure().CreateView().Returns(info => hudView);
        }

        [Test]
        public void InitializeProperly()
        {
            hudController.Initialize();
            Assert.AreEqual(hudView, hudController.view);
            hudView.Received().Initialize();
        }

        [Test]
        public void InitializeWithDataProperly()
        {
            otherPlayers.Add("user0", new Player { id = "user0", name = "user0" });
            otherPlayers.Add("user1", new Player { id = "user1", name = "user1" });
            otherPlayers.Add("user2", new Player { id = "user2", name = "user2" });
            otherPlayers.Add("user3", new Player { id = "user3", name = "user3" });

            hudController.Initialize();

            Assert.AreEqual(4, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user0"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user2"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user3"));
        }

        [Test]
        public void ReactToDataStoreChanges()
        {
            hudController.Initialize();
            var user0 = new Player { id = "user0", name = "user0" };
            var user1 = new Player { id = "user1", name = "user1" };
            otherPlayers.Add("user0", user0);
            otherPlayers.Add("user1", user1);

            hudController.Received().OnOtherPlayersStatusAdded("user0", user0);
            hudController.Received().OnOtherPlayersStatusAdded("user1", user1);

            hudController.ClearReceivedCalls();
            otherPlayers.Remove("user0");
            hudController.Received().OnOtherPlayersStatusRemoved("user0", user0);
        }

        [Test]
        public void TrackPlayersProperly()
        {
            hudController.Initialize();

            var user0 = new Player { id = "user0", name = "user0" };
            var user1 = new Player { id = "user1", name = "user1" };
            hudController.OnOtherPlayersStatusAdded("user0", user0);
            hudController.OnOtherPlayersStatusAdded("user1", user1);

            Assert.AreEqual(2, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user0"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            hudView.Received().TrackPlayer(user0);
            hudView.Received().TrackPlayer(user1);
        }

        [Test]
        public void UntrackPlayersProperly_TrackedPlayer()
        {
            hudController.Initialize();
            hudController.trackingPlayers.Add("user0");
            hudController.trackingPlayers.Add("user1");
            hudController.trackingPlayers.Add("user2");

            hudController.OnOtherPlayersStatusRemoved("user0", new Player { id = "user0", name = "user0" });

            hudView.Received().UntrackPlayer("user0");
            Assert.AreEqual(3, hudController.trackingPlayers.Count);
            Assert.IsTrue(hudController.trackingPlayers.Contains("user1"));
            Assert.IsTrue(hudController.trackingPlayers.Contains("user2"));
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }

}