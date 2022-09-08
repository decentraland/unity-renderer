using DCL;
using DCL.Components;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AvatarAttach_Tests
{
    public class AvatarAttachPlayerHandlerShould
    {
        private UserProfile userProfile;

        [SetUp]
        public void SetUp()
        {
            userProfile = UserProfile.GetOwnUserProfile();
            userProfile.UpdateData(new UserProfileModel() { userId = "ownUserId" });
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            if (userProfile != null && AssetDatabase.Contains(userProfile))
            {
                Resources.UnloadAsset(userProfile);
            }
        }

        [Test]
        public void GetOwnPlayerWhenExists()
        {
            const string playerId = "Temptation";

            userProfile = UserProfile.GetOwnUserProfile();
            userProfile.UpdateData(new UserProfileModel() { userId = playerId });

            BaseVariable<Player> ownPlayer = DataStore.i.player.ownPlayer;
            ownPlayer.Set(new Player() { id = playerId });

            GetAnchorPointsHandler handler = new GetAnchorPointsHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsTrue(found);
        }

        [Test]
        public void WaitOwnPlayerToExists()
        {
            const string playerId = "Temptation";

            userProfile.UpdateData(new UserProfileModel() { userId = playerId });

            BaseVariable<Player> ownPlayer = DataStore.i.player.ownPlayer;

            GetAnchorPointsHandler handler = new GetAnchorPointsHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsFalse(found);

            ownPlayer.Set(new Player() { id = playerId });

            Assert.IsTrue(found);
        }

        [Test]
        public void GetOtherPlayerWhenExists()
        {
            const string playerId = "Temptation";

            BaseDictionary<string, Player> otherPlayers = DataStore.i.player.otherPlayers;
            otherPlayers.Add(playerId, new Player() { id = playerId });

            GetAnchorPointsHandler handler = new GetAnchorPointsHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsTrue(found);
        }

        [Test]
        public void WaitOtherPlayerToExists()
        {
            const string playerId = "Temptation";

            BaseDictionary<string, Player> otherPlayers = DataStore.i.player.otherPlayers;

            GetAnchorPointsHandler handler = new GetAnchorPointsHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsFalse(found);

            otherPlayers.Add(playerId, new Player() { id = playerId });

            Assert.IsTrue(found);
        }

        [Test]
        public void TriggerPlayerDisconnected()
        {
            const string playerId = "Temptation";

            BaseDictionary<string, Player> otherPlayers = DataStore.i.player.otherPlayers;
            otherPlayers.Add(playerId, new Player() { id = playerId });

            GetAnchorPointsHandler handler = new GetAnchorPointsHandler();

            bool disconnectedTriggered = false;
            handler.SearchAnchorPoints(playerId, null);
            handler.OnAvatarRemoved += () => disconnectedTriggered = true;

            Assert.IsFalse(disconnectedTriggered);

            otherPlayers.Remove(playerId);
            Assert.IsTrue(disconnectedTriggered);
        }
    }
}