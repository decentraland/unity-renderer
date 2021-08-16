using System;
using DCL;
using KernelConfigurationTypes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Tests.AvatarsLODController
{
    public class AvatarsLODControllerShould
    {
        private DCL.AvatarsLODController controller;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        [SetUp]
        public void SetUp() { controller = Substitute.ForPartsOf<DCL.AvatarsLODController>(); }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.IsFalse(controller.enabled);
            Assert.AreEqual(0, controller.lodControllers.Count);
        }

        [Test]
        public void BeInitializedProperly()
        {
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true } });

            Assert.IsTrue(controller.enabled);
            Assert.AreEqual(0, controller.lodControllers.Count);
        }

        [Test]
        public void BeInitializedWithExistantPlayersProperly()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);

            otherPlayers.Add("player0", new Player { name = "player0", id = "player0", renderer = Substitute.For<IAvatarRenderer>() });
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true } });

            Assert.IsTrue(controller.enabled);
            Assert.AreEqual(1, controller.lodControllers.Count);
            Assert.AreEqual(lodController, controller.lodControllers["player0"]);
        }

        [Test]
        public void ReactToOtherPlayerAdded()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true } });

            otherPlayers.Add("player0", CreateMockPlayer("player0"));

            Assert.AreEqual(1, controller.lodControllers.Count);
            Assert.AreEqual(lodController, controller.lodControllers["player0"]);
        }

        [Test]
        public void ReactToOtherPlayerRemoved()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            otherPlayers.Add("player0", CreateMockPlayer("player0"));
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true } });

            otherPlayers.Remove("player0");

            Assert.AreEqual(0, controller.lodControllers.Count);
        }

        [Test]
        public void RegisterPlayerProperly()
        {
            IAvatarLODController lodController = null;
            controller.Configure()
                      .CreateLodController(Arg.Any<Player>())
                      .Returns(x =>
                      {
                          lodController = Substitute.For<IAvatarLODController>();
                          return lodController;
                      });
            controller.enabled = true;

            controller.RegisterAvatar("player0", CreateMockPlayer("player0"));

            Assert.NotNull(lodController);
            Assert.AreEqual(1, controller.lodControllers.Count);
            Assert.AreEqual(lodController, controller.lodControllers["player0"]);
        }

        [Test]
        public void RegisterPlayerProperly_AlreadyRegistered()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            controller.enabled = true;
            Player player0 = CreateMockPlayer("player0");
            controller.lodControllers.Add("player0", lodController);

            controller.RegisterAvatar("player0", player0);

            Assert.AreEqual(1, controller.lodControllers.Count);
            Assert.AreEqual(lodController, controller.lodControllers["player0"]);
        }

        [Test]
        public void UnregisterPlayerProperly()
        {
            Player player = CreateMockPlayer("player0");
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.enabled = true;
            controller.lodControllers.Add("player0", lodController);

            controller.UnregisterAvatar("player0", player);

            Assert.AreEqual(0, controller.lodControllers.Count);
            lodController.Received().Dispose();
        }

        [Test]
        public void UnregisterPlayerProperly_NotRegistered()
        {
            Player player = CreateMockPlayer("player0");
            controller.enabled = true;

            controller.UnregisterAvatar("player0", player);

            Assert.AreEqual(0, controller.lodControllers.Count);
        }

        [Test]
        public void UpdateBillboardsRotation()
        {
            Player player0 = CreateMockPlayer("player0", out IAvatarRenderer player0Renderer);
            Player player1 = CreateMockPlayer("player1", out IAvatarRenderer player1Renderer);
            otherPlayers.Add(player0.id, player0);
            otherPlayers.Add(player1.id, player1);
            controller.lodControllers.Add(player0.id, Substitute.For<IAvatarLODController>());
            controller.lodControllers.Add(player1.id, Substitute.For<IAvatarLODController>());

            player0.worldPosition = new Vector3(1, 0, 0);
            player1.worldPosition = new Vector3(0, 0, 1);

            // Player position is (0,0,0) so the forward vector matches the world position
            CommonScriptableObjects.cameraPosition.Set(Vector3.zero);
            controller.UpdateLODsBillboard();

            player0Renderer.Received().SetImpostorForward(new Vector3(1, 0, 0));
            player1Renderer.Received().SetImpostorForward(new Vector3(0, 0, 1));
        }

        [Test]
        public void UpdateAllLODsCorrectly_Distance()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.25f)); //Close player => Full Avatar
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.05f)); //Near By player => Simple Avatar
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.05f)); //Far Awaya player => LOD
            controller.lodControllers.Add("player0", CreateMockLODController(otherPlayers["player0"]));
            controller.lodControllers.Add("player1", CreateMockLODController(otherPlayers["player1"]));
            controller.lodControllers.Add("player2", CreateMockLODController(otherPlayers["player2"]));

            controller.UpdateAllLODs();

            controller.lodControllers["player0"].Received().SetFullAvatar();
            controller.lodControllers["player1"].Received().SetSimpleAvatar(); //This faraway player is an avatar because we didnt reach the cap
            controller.lodControllers["player2"].Received().SetSimpleAvatar();
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxFullAvatar()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.02f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.03f));
            controller.lodControllers.Add("player0", CreateMockLODController(otherPlayers["player0"]));
            controller.lodControllers.Add("player1", CreateMockLODController(otherPlayers["player1"]));
            controller.lodControllers.Add("player2", CreateMockLODController(otherPlayers["player2"]));

            controller.UpdateAllLODs(maxAvatars: 2);

            controller.lodControllers["player0"].Received().SetFullAvatar();
            controller.lodControllers["player1"].Received().SetFullAvatar();
            controller.lodControllers["player2"].Received().SetInvisible();
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxSimpleAvatar()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.02f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.03f));
            controller.lodControllers.Add("player0", CreateMockLODController(otherPlayers["player0"]));
            controller.lodControllers.Add("player1", CreateMockLODController(otherPlayers["player1"]));
            controller.lodControllers.Add("player2", CreateMockLODController(otherPlayers["player2"]));

            controller.UpdateAllLODs(maxAvatars: 2);

            controller.lodControllers["player0"].Received().SetSimpleAvatar();
            controller.lodControllers["player1"].Received().SetSimpleAvatar();
            controller.lodControllers["player2"].Received().SetInvisible();
        }

        [Test]
        public void UpdateAllLODsCorrectly_FullAvatarPrioritized()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.02f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.03f));
            controller.lodControllers.Add("player0", CreateMockLODController(otherPlayers["player0"]));
            controller.lodControllers.Add("player1", CreateMockLODController(otherPlayers["player1"]));
            controller.lodControllers.Add("player2", CreateMockLODController(otherPlayers["player2"]));

            controller.UpdateAllLODs(maxAvatars: 2);

            controller.lodControllers["player0"].Received().SetSimpleAvatar();
            controller.lodControllers["player1"].Received().SetFullAvatar();
            controller.lodControllers["player2"].Received().SetInvisible(); // Due to performance issues, we dont sort by distance
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxImpostors()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.02f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.03f));
            otherPlayers.Add("player3", CreateMockPlayer("player3", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.02f));
            otherPlayers.Add("player4", CreateMockPlayer("player4", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.04f));
            controller.lodControllers.Add("player0", CreateMockLODController(otherPlayers["player0"]));
            controller.lodControllers.Add("player1", CreateMockLODController(otherPlayers["player1"]));
            controller.lodControllers.Add("player2", CreateMockLODController(otherPlayers["player2"]));
            controller.lodControllers.Add("player3", CreateMockLODController(otherPlayers["player3"]));
            controller.lodControllers.Add("player4", CreateMockLODController(otherPlayers["player4"]));

            controller.UpdateAllLODs(maxAvatars: 3, maxImpostors: 1);

            controller.lodControllers["player0"].Received().SetSimpleAvatar(); //Takes one of the free avatar spots 
            controller.lodControllers["player1"].Received().SetFullAvatar();
            controller.lodControllers["player2"].Received().SetFullAvatar();
            controller.lodControllers["player3"].Received().SetImpostor();
            controller.lodControllers["player4"].Received().SetInvisible();
        }

        private Player CreateMockPlayer(string id) => CreateMockPlayer(id, out IAvatarRenderer renderer);
        private Player CreateMockPlayer(string id, Vector3 position) => CreateMockPlayer(id, position, out IAvatarRenderer renderer);

        private Player CreateMockPlayer(string id, out IAvatarRenderer renderer) => CreateMockPlayer(id, Vector3.zero, out renderer);

        private Player CreateMockPlayer(string id, Vector3 worldPosition, out IAvatarRenderer renderer)
        {
            renderer = Substitute.For<IAvatarRenderer>();
            return new Player { name = id, id = id, renderer = renderer, worldPosition = worldPosition };
        }
        private IAvatarLODController CreateMockLODController(Player player)
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            lodController.player.Returns(player);
            return lodController;
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            DataStore.Clear();
        }
    }
}