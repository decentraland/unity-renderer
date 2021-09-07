using DCL;
using KernelConfigurationTypes;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
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
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true, enableTutorial = false } });

            Assert.IsTrue(controller.enabled);
            Assert.AreEqual(0, controller.lodControllers.Count);
        }

        [Test]
        public void BeInitializedWithExistantPlayersProperly()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);

            otherPlayers.Add("player0", new Player { name = "player0", id = "player0", renderer = Substitute.For<IAvatarRenderer>() });
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true, enableTutorial = false } });

            Assert.IsTrue(controller.enabled);
            Assert.AreEqual(1, controller.lodControllers.Count);
            Assert.AreEqual(lodController, controller.lodControllers["player0"]);
        }

        [Test]
        public void ReactToOtherPlayerAdded()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true, enableTutorial = false } });

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
            controller.Initialize(new KernelConfigModel { features =  new Features { enableAvatarLODs = true, enableTutorial = false } });

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
        public void UpdateImpostorsBillboardRotationProperly()
        {
            Vector3 cameraPosition = Vector3.zero;
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Create full avatar to reach max 3D avatars
            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (lodDistance * 0.1f);

            // Create impostor avatar
            Player impostorPlayer = CreateMockPlayer("impostorPlayer", out IAvatarRenderer impostorPlayerRenderer);
            otherPlayers.Add(impostorPlayer.id, impostorPlayer);
            var lodController = Substitute.For<IAvatarLODController>();
            lodController.player.Returns(impostorPlayer);
            controller.lodControllers.Add(impostorPlayer.id, lodController);
            impostorPlayer.worldPosition = cameraPosition + Vector3.forward * lodDistance * 1.1f + Vector3.left * 3;

            controller.enabled = true;
            controller.Update();

            impostorPlayerRenderer.Received().SetImpostorForward((cameraPosition - impostorPlayer.worldPosition).normalized);
        }

        [Test]
        public void AvoidInvisiblesBillboardRotation()
        {
            Vector3 cameraPosition = Vector3.zero;
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Create full avatar to reach max 3D avatars
            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (lodDistance * 0.1f);

            // Create impostor avatar
            Player impostorPlayer = CreateMockPlayer("impostorPlayer", out IAvatarRenderer impostorPlayerRenderer);
            otherPlayers.Add(impostorPlayer.id, impostorPlayer);
            var lodController = Substitute.For<IAvatarLODController>();
            lodController.player.Returns(impostorPlayer);
            controller.lodControllers.Add(impostorPlayer.id, lodController);
            impostorPlayer.worldPosition = cameraPosition - Vector3.forward * lodDistance;

            controller.UpdateAllLODs(1);

            impostorPlayerRenderer.DidNotReceive().SetImpostorForward(new Vector3(0, 0, -1));
        }

        [Test]
        public void UpdateAllLODsCorrectly_Distance()
        {
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f); //Close player => Full Avatar

            Player simpleAvatarPlayer = CreateMockPlayer("simpleAvatar");
            IAvatarLODController simpleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            simpleAvatarPlayerController.player.Returns(simpleAvatarPlayer);
            controller.lodControllers.Add(simpleAvatarPlayer.id, simpleAvatarPlayerController);
            simpleAvatarPlayer.worldPosition = Vector3.forward * (simpleAvatarDistance * 1.05f); //Near By player => Simple Avatar

            Player impostorAvatarPlayer = CreateMockPlayer("impostorAvatar");
            IAvatarLODController impostorAvatarPlayerController = Substitute.For<IAvatarLODController>();
            impostorAvatarPlayerController.player.Returns(impostorAvatarPlayer);
            controller.lodControllers.Add(impostorAvatarPlayer.id, impostorAvatarPlayerController);
            impostorAvatarPlayer.worldPosition = Vector3.forward * (lodDistance * 1.05f); //Far Away player => LOD

            Player invisibleAvatarPlayer = CreateMockPlayer("invisibleAvatar");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            invisibleAvatarPlayerController.player.Returns(invisibleAvatarPlayer);
            controller.lodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = -Vector3.forward * 10f; //player behind camera => Invisible

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.enabled = true;
            controller.Update();

            fullAvatarPlayerController.Received().SetFullAvatar();
            simpleAvatarPlayerController.Received().SetSimpleAvatar();
            impostorAvatarPlayerController.Received().SetImpostor();
            invisibleAvatarPlayerController.Received().SetInvisible();
        }

        [Test]
        public void UpdateImpostorsTintAndInterpolationMovement()
        {
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();
            float tintMinDistance = 30f; // default internal value

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f);

            Player impostorAvatarPlayer = CreateMockPlayer("impostorAvatar");
            IAvatarLODController impostorAvatarPlayerController = Substitute.For<IAvatarLODController>();
            impostorAvatarPlayerController.player.Returns(impostorAvatarPlayer);
            controller.lodControllers.Add(impostorAvatarPlayer.id, impostorAvatarPlayerController);
            impostorAvatarPlayer.worldPosition = Vector3.forward * tintMinDistance;

            DataStore.i.avatarsLOD.maxAvatars.Set(1);
            controller.enabled = true;
            controller.Update();

            impostorAvatarPlayerController.ReceivedWithAnyArgs().UpdateImpostorTint(default);
        }

        [Test]
        public void SetNonRenderedAvatarsAsInvisible()
        {
            Vector3 cameraPosition = Vector3.zero;
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            // Create full avatar to reach max 3D avatars
            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (lodDistance * 0.1f);

            // Create impostor avatar
            Player invisibleAvatarPlayer = CreateMockPlayer("impostorPlayer");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            invisibleAvatarPlayerController.player.Returns(invisibleAvatarPlayer);
            otherPlayers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayer);
            controller.lodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = cameraPosition - Vector3.forward * 5;

            controller.UpdateAllLODs(1);

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

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.enabled = true;
            controller.Update();

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

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.enabled = true;
            controller.Update();

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

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.enabled = true;
            controller.Update();

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

            DataStore.i.avatarsLOD.maxAvatars.Set(3);
            DataStore.i.avatarsLOD.maxImpostors.Set(1);
            controller.enabled = true;
            controller.Update();

            controller.lodControllers["player0"].Received().SetSimpleAvatar(); //Takes one of the free avatar spots 
            controller.lodControllers["player1"].Received().SetFullAvatar();
            controller.lodControllers["player2"].Received().SetFullAvatar();
            controller.lodControllers["player3"].Received().SetImpostor();
            controller.lodControllers["player4"].Received().SetInvisible();
        }

        [Test]
        public void UpdateThrottling()
        {
            Vector3 cameraPosition = Vector3.zero;
            CommonScriptableObjects.cameraForward.Set(Vector3.forward);
            CommonScriptableObjects.cameraPosition.Set(cameraPosition);
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.lodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f); //Close player => Full Avatar

            Player simpleAvatarPlayer = CreateMockPlayer("simpleAvatar");
            IAvatarLODController simpleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            simpleAvatarPlayerController.player.Returns(simpleAvatarPlayer);
            controller.lodControllers.Add(simpleAvatarPlayer.id, simpleAvatarPlayerController);
            simpleAvatarPlayer.worldPosition = Vector3.forward * (simpleAvatarDistance * 1.05f); //Near By player => Simple Avatar

            Player farawaySimpleAvatar = CreateMockPlayer("farawaySimpleAvatar");
            IAvatarLODController farawaySimpleAvatarController = Substitute.For<IAvatarLODController>();
            farawaySimpleAvatarController.player.Returns(farawaySimpleAvatar);
            controller.lodControllers.Add(farawaySimpleAvatar.id, farawaySimpleAvatarController);
            farawaySimpleAvatar.worldPosition = Vector3.forward * (lodDistance * 1.05f); //Far Away player => Simple avatar (due to maxAvatars)

            Player farAwayImpostor = CreateMockPlayer("farAwayImpostor");
            IAvatarLODController farAwayImpostorController = Substitute.For<IAvatarLODController>();
            farAwayImpostorController.player.Returns(farAwayImpostor);
            controller.lodControllers.Add(farAwayImpostor.id, farAwayImpostorController);
            farAwayImpostor.worldPosition = Vector3.forward * (lodDistance * 1.10f); //Far Away player => Impostor (due to maxAvatars)

            Player invisibleAvatarPlayer = CreateMockPlayer("invisibleAvatar");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            invisibleAvatarPlayerController.player.Returns(invisibleAvatarPlayer);
            controller.lodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = -Vector3.forward * 10f; //player behind camera => Invisible

            DataStore.i.avatarsLOD.maxAvatars.Set(3);
            controller.enabled = true;
            controller.Update();

            fullAvatarPlayerController.Received().SetThrottling(IAvatarRenderer.AnimationThrottling.Full);
            simpleAvatarPlayerController.Received().SetThrottling(IAvatarRenderer.AnimationThrottling.Near);
            farawaySimpleAvatarController.Received().SetThrottling(IAvatarRenderer.AnimationThrottling.FarAway);
            farAwayImpostorController.DidNotReceiveWithAnyArgs().SetThrottling(Arg.Any<IAvatarRenderer.AnimationThrottling>());
            invisibleAvatarPlayerController.DidNotReceiveWithAnyArgs().SetThrottling(Arg.Any<IAvatarRenderer.AnimationThrottling>());
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