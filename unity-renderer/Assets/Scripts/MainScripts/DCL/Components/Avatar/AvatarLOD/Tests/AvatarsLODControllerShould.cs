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
        public void UpdateImpostorsBillboardsRotation()
        {
            controller.enabled = true;
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Populate with minimum avatars to trigger impostors
            int maxNonLODAvatars = DataStore.i.avatarsLOD.maxAvatars.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CreateExtraPlayers(maxNonLODAvatars, new Vector3(0, 0, lodDistance / 2));

            // Create impostor avatar
            Player impostorPlayer = CreateMockPlayer("impostorPlayer", out IAvatarRenderer impostorPlayerRenderer);
            otherPlayers.Add(impostorPlayer.id, impostorPlayer);
            controller.lodControllers.Add(impostorPlayer.id, Substitute.For<IAvatarLODController>());
            impostorPlayer.worldPosition = cameraPosition + Vector3.forward * lodDistance * 1.1f;

            controller.Update();
            controller.UpdateLODsBillboard();

            impostorPlayerRenderer.Received().SetImpostorForward(new Vector3(0, 0, 1));
        }

        [Test]
        public void AvoidInvisiblesBillboardRotation()
        {
            controller.enabled = true;
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Populate with minimum avatars to trigger impostors
            int maxNonLODAvatars = DataStore.i.avatarsLOD.maxAvatars.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CreateExtraPlayers(maxNonLODAvatars, new Vector3(0, 0, lodDistance / 2));

            // Create impostor avatar
            Player impostorPlayer = CreateMockPlayer("impostorPlayer", out IAvatarRenderer impostorPlayerRenderer);
            otherPlayers.Add(impostorPlayer.id, impostorPlayer);
            controller.lodControllers.Add(impostorPlayer.id, Substitute.For<IAvatarLODController>());
            impostorPlayer.worldPosition = cameraPosition - Vector3.forward * lodDistance;

            controller.Update();
            controller.UpdateLODsBillboard();

            impostorPlayerRenderer.DidNotReceive().SetImpostorForward(new Vector3(0, 0, -1));
        }

        [Test]
        public void UpdateAllLODsCorrectly_Distance()
        {
            controller.enabled = true;
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Populate with minimum avatars to trigger impostors in front of the camera
            int maxNonLODAvatars = DataStore.i.avatarsLOD.maxAvatars.Get() - 2; // those 2 missing 3D avatars are created and evaluated at the end
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CreateExtraPlayers(maxNonLODAvatars, new Vector3(0, 0, lodDistance / 2));

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            Player simpleAvatarPlayer = CreateMockPlayer("simpleAvatar");
            Player impostorAvatarPlayer = CreateMockPlayer("impostorAvatar");
            Player invisibleAvatarPlayer = CreateMockPlayer("invisibleAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            IAvatarLODController simpleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            IAvatarLODController impostorAvatarPlayerController = Substitute.For<IAvatarLODController>();
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            otherPlayers.Add(fullAvatarPlayer.id, fullAvatarPlayer);
            otherPlayers.Add(simpleAvatarPlayer.id, simpleAvatarPlayer);
            otherPlayers.Add(impostorAvatarPlayer.id, impostorAvatarPlayer);
            otherPlayers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            controller.lodControllers.Add(simpleAvatarPlayer.id, simpleAvatarPlayerController);
            controller.lodControllers.Add(impostorAvatarPlayer.id, impostorAvatarPlayerController);
            controller.lodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);

            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (DCL.AvatarsLODController.SIMPLE_AVATAR_DISTANCE * 0.25f); //Close player => Full Avatar
            simpleAvatarPlayer.worldPosition = Vector3.forward * (DCL.AvatarsLODController.SIMPLE_AVATAR_DISTANCE * 1.05f); //Near By player => Simple Avatar
            impostorAvatarPlayer.worldPosition = Vector3.forward * (lodDistance * 1.05f); //Far Away player => LOD
            invisibleAvatarPlayer.worldPosition = -Vector3.forward * 10f; //player behind camera => Invisible

            controller.Update();
            controller.UpdateAllLODs();

            fullAvatarPlayerController.Received().SetFullAvatar();
            simpleAvatarPlayerController.Received().SetSimpleAvatar();
            impostorAvatarPlayerController.Received().SetSimpleAvatar();  //This faraway player is an avatar because we didnt reach the cap
            // impostorAvatarPlayerController.Received().SetImpostorState();
            invisibleAvatarPlayerController.Received().SetInvisible();
        }

        [Test]
        public void SetNonRenderedAvatarsAsInvisible()
        {
            controller.enabled = true;
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Populate with minimum avatars to trigger impostors
            int maxNonLODAvatars = DataStore.i.avatarsLOD.maxAvatars.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CreateExtraPlayers(maxNonLODAvatars, new Vector3(0, 0, lodDistance / 2));

            // Create impostor avatar
            Player invisibleAvatarPlayer = CreateMockPlayer("impostorPlayer");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            otherPlayers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayer);
            controller.lodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = cameraPosition - Vector3.forward * 5;

            controller.Update();
            controller.UpdateLODsBillboard();

            invisibleAvatarPlayerController.Received().SetInvisible();
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

             controller.lodControllers["player0"].Received().SetInvisible();
             controller.lodControllers["player1"].Received().SetFullAvatar();
             controller.lodControllers["player2"].Received().SetFullAvatar();
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

        private void CreateExtraPlayers(int amount, Vector3 position)
        {
            for (int i = 0; i < amount; i++)
            {
                Player player = CreateMockPlayer("player" + (i + 100));
                otherPlayers.Add(player.id, player);
                controller.lodControllers.Add(player.id, Substitute.For<IAvatarLODController>());
                player.worldPosition = position;
            }
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            DataStore.Clear();
        }
    }
}