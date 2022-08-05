using System.Collections.Generic;
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
                    id = "temptation", basePosition = new Vector2Int(1, 0)
                });

            componentsWriter = Substitute.For<IECSComponentWriter>();
            avatarTransform = (new GameObject("GO")).transform;
            avatarTransform.position = new UnityEngine.Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(UnityEngine.Vector3.zero);
            DataStore.i.world.avatarTransform.Set(avatarTransform);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            Object.Destroy(avatarTransform.gameObject);
        }

        [Test]
        public void NotSendTransformIfNoChange()
        {
            var update = ECSPlayerTransformSystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.PLAYER_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Any<ECSTransform>(),
                                Arg.Any<long>(),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                ComponentID.TRANSFORM,
                                Arg.Any<ECSTransform>(),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));

            componentsWriter.ClearReceivedCalls();

            update.Invoke();
            componentsWriter.DidNotReceive()
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.PLAYER_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Any<ECSTransform>());
        }

        [Test]
        public void SendTransformIfChanged()
        {
            var update = ECSPlayerTransformSystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.PLAYER_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x => x.position == UnityEngine.Vector3.zero),
                                Arg.Any<long>(),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x => x.position == UnityEngine.Vector3.zero),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));

            componentsWriter.ClearReceivedCalls();

            avatarTransform.position = new UnityEngine.Vector3(0, 0, 0);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.PLAYER_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x =>
                                    x.position == new UnityEngine.Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)),
                                Arg.Any<long>(),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.SEND_TO_SCENE));

            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x =>
                                    x.position == new UnityEngine.Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)),
                                Arg.Is<ECSComponentWriteType>(x => x == ECSComponentWriteType.EXECUTE_LOCALLY));
        }
    }
}