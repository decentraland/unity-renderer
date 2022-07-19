using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using ECSSystems.PlayerSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSPlayerTransformSystemShould
    {
        private Transform avatarTransform;

        [SetUp]
        public void SetUp()
        {
            var scenes = new[] { Substitute.For<IParcelScene>() };
            scenes[0]
                .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "temptation", basePosition = new Vector2Int(1, 0)
                });

            ECSSystemsReferencesContainer.loadedScenes = scenes;
            ECSSystemsReferencesContainer.componentsWriter = Substitute.For<IECSComponentWriter>();
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
            var update = ECSPlayerTransformSystem.CreateSystem();

            update.Invoke();
            ECSSystemsReferencesContainer.componentsWriter.Received(1)
                                         .PutComponent(
                                             ECSSystemsReferencesContainer.loadedScenes[0].sceneData.id,
                                             SpecialEntityId.PLAYER_ENTITY,
                                             ComponentID.TRANSFORM,
                                             Arg.Any<ECSTransform>());

            ECSSystemsReferencesContainer.componentsWriter.ClearReceivedCalls();

            update.Invoke();
            ECSSystemsReferencesContainer.componentsWriter.DidNotReceive()
                                         .PutComponent(
                                             ECSSystemsReferencesContainer.loadedScenes[0].sceneData.id,
                                             SpecialEntityId.PLAYER_ENTITY,
                                             ComponentID.TRANSFORM,
                                             Arg.Any<ECSTransform>());
        }

        [Test]
        public void SendTransformIfChanged()
        {
            var update = ECSPlayerTransformSystem.CreateSystem();

            update.Invoke();
            ECSSystemsReferencesContainer.componentsWriter.Received(1)
                                         .PutComponent(
                                             ECSSystemsReferencesContainer.loadedScenes[0].sceneData.id,
                                             SpecialEntityId.PLAYER_ENTITY,
                                             ComponentID.TRANSFORM,
                                             Arg.Is<ECSTransform>(x => x.position == UnityEngine.Vector3.zero));

            ECSSystemsReferencesContainer.componentsWriter.ClearReceivedCalls();

            avatarTransform.position = new UnityEngine.Vector3(0, 0, 0);

            update.Invoke();
            ECSSystemsReferencesContainer.componentsWriter.Received(1)
                                         .PutComponent(
                                             ECSSystemsReferencesContainer.loadedScenes[0].sceneData.id,
                                             SpecialEntityId.PLAYER_ENTITY,
                                             ComponentID.TRANSFORM,
                                             Arg.Is<ECSTransform>(x =>
                                                 x.position == new UnityEngine.Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)));
        }
    }
}