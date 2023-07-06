using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.PlayerSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using TestUtils;
using UnityEngine;

namespace Tests
{
    public class ECSPlayerTransformSystemShould
    {
        private Transform avatarTransform;
        private BaseList<IParcelScene> scenes;
        private WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool;
        private ECSComponent<ECSTransform> transformComponent;
        private IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;

        [SetUp]
        public void SetUp()
        {
            scenes = new BaseList<IParcelScene>();
            scenes.Add(Substitute.For<IParcelScene>());

            scenes[0]
               .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = 666, basePosition = new Vector2Int(1, 0)
                });

            var entities = new Dictionary<long, IDCLEntity>();
            scenes[0].entities.Returns(entities);

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            transformPool = new WrappedComponentPool<IWrappedComponent<ECSTransform>>(0, () => new TransformWrappedComponent(new ECSTransform()));
            transformComponent = new ECSComponent<ECSTransform>(null, null);

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
            ECSPlayerTransformSystem system = new ECSPlayerTransformSystem(componentsWriter, transformPool, transformComponent, scenes,
                DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(
                SpecialEntityId.PLAYER_ENTITY,
                ComponentID.TRANSFORM,
                null
            );

            outgoingMessages.Clear_Calls();

            system.Update();

            outgoingMessages.Put_NotCalled(
                SpecialEntityId.PLAYER_ENTITY,
                ComponentID.TRANSFORM
            );

            system.Dispose();
        }

        [Test]
        public void SendTransformIfChanged()
        {
            ECSPlayerTransformSystem system = new ECSPlayerTransformSystem(componentsWriter, transformPool, transformComponent, scenes,
                DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(
                SpecialEntityId.PLAYER_ENTITY,
                ComponentID.TRANSFORM,
                x => x.position == Vector3.zero
            );

            outgoingMessages.Clear_Calls();

            avatarTransform.position = new Vector3(0, 0, 0);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(
                SpecialEntityId.PLAYER_ENTITY,
                ComponentID.TRANSFORM,
                x =>
                    x.position == new Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)
            );

            system.Dispose();
        }
    }
}
