using System;
using System.Collections.Generic;
using AvatarSystem;
using DCL;
using DCL.CameraTool;
using DCL.Helpers;
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
        public void SetUp()
        {
            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.Register<IUpdateEventHandler>( () => Substitute.For<IUpdateEventHandler>());
            DCL.Environment.Setup(serviceLocator);

            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);
            controller = Substitute.ForPartsOf<DCL.AvatarsLODController>();
            controller.Initialize();
        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.AreEqual(0, controller.LodControllers.Count);
        }

        [Test]
        public void BeInitializedWithExistantPlayersProperly()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);

            otherPlayers.Add("player0", new Player { name = "player0", id = "player0", avatar = Substitute.For<IAvatar>() });

            Assert.AreEqual(1, controller.LodControllers.Count);
            Assert.AreEqual(lodController, controller.LodControllers["player0"]);
        }

        [Test]
        public void ReactToOtherPlayerAdded()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);

            otherPlayers.Add("player0", CreateMockPlayer("player0"));

            Assert.AreEqual(1, controller.LodControllers.Count);
            Assert.AreEqual(lodController, controller.LodControllers["player0"]);
        }

        [Test]
        public void ReactToOtherPlayerRemoved()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            otherPlayers.Add("player0", CreateMockPlayer("player0"));

            otherPlayers.Remove("player0");

            Assert.AreEqual(0, controller.LodControllers.Count);
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

            controller.RegisterAvatar("player0", CreateMockPlayer("player0"));

            Assert.NotNull(lodController);
            Assert.AreEqual(1, controller.LodControllers.Count);
            Assert.AreEqual(lodController, controller.LodControllers["player0"]);
        }

        [Test]
        public void RegisterPlayerProperly_AlreadyRegistered()
        {
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.Configure().CreateLodController(Arg.Any<Player>()).Returns(lodController);
            Player player0 = CreateMockPlayer("player0");
            controller.LodControllers.Add("player0", lodController);

            controller.RegisterAvatar("player0", player0);

            Assert.AreEqual(1, controller.LodControllers.Count);
            Assert.AreEqual(lodController, controller.LodControllers["player0"]);
        }

        [Test]
        public void UnregisterPlayerProperly()
        {
            Player player = CreateMockPlayer("player0");
            IAvatarLODController lodController = Substitute.For<IAvatarLODController>();
            controller.LodControllers.Add("player0", lodController);

            controller.UnregisterAvatar("player0", player);

            Assert.AreEqual(0, controller.LodControllers.Count);
            lodController.Received().Dispose();
        }

        [Test]
        public void UnregisterPlayerProperly_NotRegistered()
        {
            Player player = CreateMockPlayer("player0");

            controller.UnregisterAvatar("player0", player);

            Assert.AreEqual(0, controller.LodControllers.Count);
        }

        [Test]
        public void UpdateAllLODsCorrectly_Distance()
        {
            controller.cameraTransformValue = MockCameraTransform();
            Vector3 cameraPosition = controller.cameraTransformValue.position;
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.LodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f); //Close player => Full Avatar

            Player simpleAvatarPlayer = CreateMockPlayer("simpleAvatar");
            IAvatarLODController simpleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            simpleAvatarPlayerController.player.Returns(simpleAvatarPlayer);
            controller.LodControllers.Add(simpleAvatarPlayer.id, simpleAvatarPlayerController);
            simpleAvatarPlayer.worldPosition = Vector3.forward * (simpleAvatarDistance * 1.05f); //Near By player => Simple Avatar

            Player impostorAvatarPlayer = CreateMockPlayer("impostorAvatar");
            IAvatarLODController impostorAvatarPlayerController = Substitute.For<IAvatarLODController>();
            impostorAvatarPlayerController.player.Returns(impostorAvatarPlayer);
            controller.LodControllers.Add(impostorAvatarPlayer.id, impostorAvatarPlayerController);
            impostorAvatarPlayer.worldPosition = Vector3.forward * (lodDistance * 1.05f); //Far Away player => LOD

            Player invisibleAvatarPlayer = CreateMockPlayer("invisibleAvatar");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            invisibleAvatarPlayerController.player.Returns(invisibleAvatarPlayer);
            controller.LodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = -Vector3.forward * 10f; //player behind camera => Invisible

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.Update();

            fullAvatarPlayerController.Received().SetLOD0();
            simpleAvatarPlayerController.Received().SetLOD1();
            impostorAvatarPlayerController.Received().SetLOD2();
            invisibleAvatarPlayerController.Received().SetInvisible();
        }

        [Test]
        public void HideCharacterClippingAvatars()
        {
            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.cameraTransformValue = MockCameraTransform();
            Vector3 cameraPosition = controller.cameraTransformValue.position;
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);

            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();

            Player avatar = CreateMockPlayer("avatar");
            IAvatarLODController avatarPlayerController = Substitute.For<IAvatarLODController>();
            avatarPlayerController.player.Returns(avatar);
            controller.LodControllers.Add(avatar.id, avatarPlayerController);

            // Place at normal distance
            avatar.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f);
            controller.Update();
            avatarPlayerController.Received().SetLOD0();

            // Place super close to the main player
            avatar.worldPosition = cameraPosition + Vector3.forward * 0.75f;
            controller.Update();
            avatarPlayerController.Received().SetInvisible();
        }

        [Test]
        public void UpdateImpostorsTintAndInterpolationMovement()
        {
            controller.cameraTransformValue = MockCameraTransform();
            Vector3 cameraPosition = controller.cameraTransformValue.position;
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float simpleAvatarDistance = DataStore.i.avatarsLOD.simpleAvatarDistance.Get();
            float tintMinDistance = 30f; // default internal value

            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.LodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (simpleAvatarDistance * 0.25f);

            Player impostorAvatarPlayer = CreateMockPlayer("impostorAvatar");
            IAvatarLODController impostorAvatarPlayerController = Substitute.For<IAvatarLODController>();
            impostorAvatarPlayerController.player.Returns(impostorAvatarPlayer);
            controller.LodControllers.Add(impostorAvatarPlayer.id, impostorAvatarPlayerController);
            impostorAvatarPlayer.worldPosition = Vector3.forward * tintMinDistance;

            DataStore.i.avatarsLOD.maxAvatars.Set(1);
            controller.Update();

            impostorAvatarPlayerController.ReceivedWithAnyArgs().UpdateImpostorTint(default);
        }

        [Test]
        public void SetNonRenderedAvatarsAsInvisible()
        {
            controller.cameraTransformValue = MockCameraTransform();
            Vector3 cameraPosition = controller.cameraTransformValue.position;
            CommonScriptableObjects.playerUnityPosition.Set(cameraPosition);
            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();

            // Create full avatar to reach max 3D avatars
            Player fullAvatarPlayer = CreateMockPlayer("fullAvatar");
            IAvatarLODController fullAvatarPlayerController = Substitute.For<IAvatarLODController>();
            fullAvatarPlayerController.player.Returns(fullAvatarPlayer);
            controller.LodControllers.Add(fullAvatarPlayer.id, fullAvatarPlayerController);
            fullAvatarPlayer.worldPosition = cameraPosition + Vector3.forward * (lodDistance * 0.1f);

            // Create impostor avatar
            Player invisibleAvatarPlayer = CreateMockPlayer("impostorPlayer");
            IAvatarLODController invisibleAvatarPlayerController = Substitute.For<IAvatarLODController>();
            invisibleAvatarPlayerController.player.Returns(invisibleAvatarPlayer);
            //otherPlayers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayer);
            controller.LodControllers.Add(invisibleAvatarPlayer.id, invisibleAvatarPlayerController);
            invisibleAvatarPlayer.worldPosition = cameraPosition - Vector3.forward * 5;

            controller.UpdateAllLODs(1);

            invisibleAvatarPlayerController.Received().SetInvisible();
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxFullAvatar()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.91f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.92f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.93f));
            controller.LodControllers["player0"] = CreateMockLODController(otherPlayers["player0"]);
            controller.LodControllers["player1"] = CreateMockLODController(otherPlayers["player1"]);
            controller.LodControllers["player2"] = CreateMockLODController(otherPlayers["player2"]);

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.cameraTransformValue = MockCameraTransform();
            controller.Update();

            controller.LodControllers["player0"].Received().SetLOD0();
            controller.LodControllers["player1"].Received().SetLOD0();
            controller.LodControllers["player2"].Received().SetLOD2();
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxSimpleAvatar()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.02f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.03f));
            controller.LodControllers["player0"] = CreateMockLODController(otherPlayers["player0"]);
            controller.LodControllers["player1"] = CreateMockLODController(otherPlayers["player1"]);
            controller.LodControllers["player2"] = CreateMockLODController(otherPlayers["player2"]);

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.cameraTransformValue = MockCameraTransform();
            controller.Update();

            controller.LodControllers["player0"].Received().SetLOD1();
            controller.LodControllers["player1"].Received().SetLOD1();
            controller.LodControllers["player2"].Received().SetLOD2();
        }

        [Test]
        public void UpdateAllLODsCorrectly_FullAvatarPrioritized()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.92f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.93f));
            controller.LodControllers["player0"] = CreateMockLODController(otherPlayers["player0"]);
            controller.LodControllers["player1"] = CreateMockLODController(otherPlayers["player1"]);
            controller.LodControllers["player2"] = CreateMockLODController(otherPlayers["player2"]);

            DataStore.i.avatarsLOD.maxAvatars.Set(2);
            controller.cameraTransformValue = MockCameraTransform();
            controller.Update();

            controller.LodControllers["player0"].Received().SetLOD2();
            controller.LodControllers["player1"].Received().SetLOD0();
            controller.LodControllers["player2"].Received().SetLOD0();
        }

        [Test]
        public void UpdateAllLODsCorrectly_MaxImpostors()
        {
            otherPlayers.Add("player0", CreateMockPlayer("player0", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.01f));
            otherPlayers.Add("player1", CreateMockPlayer("player1", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.92f));
            otherPlayers.Add("player2", CreateMockPlayer("player2", Vector3.forward * DataStore.i.avatarsLOD.simpleAvatarDistance.Get() * 0.93f));
            otherPlayers.Add("player3", CreateMockPlayer("player3", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.02f));
            otherPlayers.Add("player4", CreateMockPlayer("player4", Vector3.forward * DataStore.i.avatarsLOD.LODDistance.Get() * 1.04f));
            controller.LodControllers["player0"] = CreateMockLODController(otherPlayers["player0"]);
            controller.LodControllers["player1"] = CreateMockLODController(otherPlayers["player1"]);
            controller.LodControllers["player2"] = CreateMockLODController(otherPlayers["player2"]);
            controller.LodControllers["player3"] = CreateMockLODController(otherPlayers["player3"]);
            controller.LodControllers["player4"] = CreateMockLODController(otherPlayers["player4"]);

            DataStore.i.avatarsLOD.maxAvatars.Set(3);
            DataStore.i.avatarsLOD.maxImpostors.Set(1);
            controller.cameraTransformValue = MockCameraTransform();
            controller.Update();

            controller.LodControllers["player0"].Received().SetLOD1(); //Takes one of the free avatar spots
            controller.LodControllers["player1"].Received().SetLOD0();
            controller.LodControllers["player2"].Received().SetLOD0();
            controller.LodControllers["player3"].Received().SetLOD2();
            controller.LodControllers["player4"].Received().SetInvisible();
        }

        private static Transform MockCameraTransform()
        {
            var cameraTransform = new GameObject().transform;
            cameraTransform.position = Vector3.zero;
            cameraTransform.forward = Vector3.forward;
            return cameraTransform;
        }

        private Player CreateMockPlayer(string id) => CreateMockPlayer(id, out IAvatar renderer);

        private Player CreateMockPlayer(string id, Vector3 position) => CreateMockPlayer(id, position, out IAvatar renderer);

        private Player CreateMockPlayer(string id, out IAvatar avatar) => CreateMockPlayer(id, Vector3.zero, out avatar);

        private Player CreateMockPlayer(string id, Vector3 worldPosition, out IAvatar avatar)
        {
            avatar = Substitute.For<IAvatar>();
            return new Player { name = id, id = id, avatar = avatar, worldPosition = worldPosition, playerName = Substitute.For<IPlayerName>() };
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
            DCL.Environment.Dispose();
        }
    }
}
