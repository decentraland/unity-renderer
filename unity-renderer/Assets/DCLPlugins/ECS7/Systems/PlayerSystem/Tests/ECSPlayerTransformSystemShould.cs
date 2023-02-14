using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.PlayerSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class ECSPlayerTransformSystemShould
    {
        private Transform avatarTransform;
        private IECSComponentWriter componentsWriter;
        private IList<IParcelScene> scenes;

        [SetUp]
        public void SetUp()
        {
            scenes = DataStore.i.ecs7.scenes;
            scenes.Add(Substitute.For<IParcelScene>());

            scenes[0]
               .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = 666, basePosition = new Vector2Int(1, 0)
                });

            componentsWriter = Substitute.For<IECSComponentWriter>();
            avatarTransform = (new GameObject("GO")).transform;
            avatarTransform.position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(Vector3.zero);
            DataStore.i.world.avatarTransform.Set(avatarTransform);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            CommonScriptableObjects.UnloadAll();
            Object.Destroy(avatarTransform.gameObject);
        }

        [Test]
        public void NotSendTransformIfNoChange()
        {
            ECSPlayerTransformSystem system = new ECSPlayerTransformSystem(componentsWriter, DataStore.i.ecs7.scenes,
                DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.PLAYER_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Any<ECSTransform>(),
                                 Arg.Any<int>(),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                 ComponentID.TRANSFORM,
                                 Arg.Any<ECSTransform>(),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));

            componentsWriter.ClearReceivedCalls();

            system.Update();

            componentsWriter.DidNotReceive()
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.PLAYER_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Any<ECSTransform>());

            system.Dispose();
        }

        [Test]
        public void SendTransformIfChanged()
        {
            ECSPlayerTransformSystem system = new ECSPlayerTransformSystem(componentsWriter, DataStore.i.ecs7.scenes,
                DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.PLAYER_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x => x.position == Vector3.zero),
                                 Arg.Any<int>(),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x => x.position == Vector3.zero),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));

            componentsWriter.ClearReceivedCalls();

            avatarTransform.position = new Vector3(0, 0, 0);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.PLAYER_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x =>
                                     x.position == new Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)),
                                 Arg.Any<int>(),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x =>
                                     x.position == new Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)),
                                 Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));

            system.Dispose();
        }
    }
}
