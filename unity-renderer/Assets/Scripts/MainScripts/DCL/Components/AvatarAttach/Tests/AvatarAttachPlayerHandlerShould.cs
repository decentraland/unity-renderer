using DCL;
using DCL.Components;
using NUnit.Framework;

namespace AvatarAttach_Tests
{
    public class AvatarAttachPlayerHandlerShould
    {
        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void GetOwnPlayerWhenExists()
        {
            const string playerId = "Temptation";

            BaseVariable<Player> ownPlayer = DataStore.i.player.ownPlayer;
            ownPlayer.Set(new Player() { id = playerId });

            AvatarAttachPlayerHandler handler = new AvatarAttachPlayerHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsTrue(found);
        }

        [Test]
        public void WaitOwnPlayerToExists()
        {
            const string playerId = "Temptation";

            BaseVariable<Player> ownPlayer = DataStore.i.player.ownPlayer;

            AvatarAttachPlayerHandler handler = new AvatarAttachPlayerHandler();

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

            AvatarAttachPlayerHandler handler = new AvatarAttachPlayerHandler();

            bool found = false;
            handler.SearchAnchorPoints(playerId, anchorPoints => found = true);

            Assert.IsTrue(found);
        }

        [Test]
        public void WaitOtherPlayerToExists()
        {
            const string playerId = "Temptation";

            BaseDictionary<string, Player> otherPlayers = DataStore.i.player.otherPlayers;

            AvatarAttachPlayerHandler handler = new AvatarAttachPlayerHandler();

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

            AvatarAttachPlayerHandler handler = new AvatarAttachPlayerHandler();

            bool disconnectedTriggered = false;
            handler.SearchAnchorPoints(playerId, null);
            handler.onAvatarDisconnect += () => disconnectedTriggered = true;

            Assert.IsFalse(disconnectedTriggered);

            otherPlayers.Remove(playerId);
            Assert.IsTrue(disconnectedTriggered);
        }
    }
}