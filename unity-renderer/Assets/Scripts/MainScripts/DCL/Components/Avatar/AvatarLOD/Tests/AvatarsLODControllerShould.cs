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
        public void UpdateAllLODsCorrectly()
        {
            Player player0 = CreateMockPlayer("player0");
            Player player1 = CreateMockPlayer("player1");
            Player player2 = CreateMockPlayer("player2");
            IAvatarLODController lodController0 = Substitute.For<IAvatarLODController>();
            IAvatarLODController lodController1 = Substitute.For<IAvatarLODController>();
            IAvatarLODController lodController2 = Substitute.For<IAvatarLODController>();
            otherPlayers.Add(player0.id, player0);
            otherPlayers.Add(player1.id, player1);
            otherPlayers.Add(player2.id, player2);
            controller.lodControllers.Add(player0.id, lodController0);
            controller.lodControllers.Add(player1.id, lodController1);
            controller.lodControllers.Add(player2.id, lodController2);

            player0.worldPosition = Vector3.forward * (DCL.AvatarsLODController.SIMPLE_AVATAR_DISTANCE * 0.25f); //Close player => Full Avatar
            player1.worldPosition = Vector3.forward * (DCL.AvatarsLODController.SIMPLE_AVATAR_DISTANCE * 1.05f); //Near By player => Simple Avatar
            player2.worldPosition = Vector3.forward * (DataStore.i.avatarsLOD.LODDistance.Get() * 1.05f); //Far Awaya player => LOD

            controller.UpdateAllLODs();

            lodController0.Received().SetAvatarState();
            lodController1.Received().SetSimpleAvatar();
            lodController2.Received().SetImpostorState();
        }

        private Player CreateMockPlayer(string id) => CreateMockPlayer(id, out IAvatarRenderer renderer);

        private Player CreateMockPlayer(string id, out IAvatarRenderer renderer)
        {
            renderer = Substitute.For<IAvatarRenderer>();
            return new Player { name = id, id = id, renderer = renderer };
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            DataStore.Clear();
        }
    }
}