using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.GltfContainerLoadingStateSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestUtils;

namespace Tests
{
    public class GltfContainerLoadingStateSystemShould
    {
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;
        private IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private IParcelScene scene;
        private IDCLEntity entity;
        private Action systemUpdate;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(manager, factory, executors);
            gltfContainerLoadingStateComponent = internalComponents.GltfContainerLoadingStateComponent;

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            var componentPool = new WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>>(0, () => new ProtobufWrappedComponent<PBGltfContainerLoadingState>(new PBGltfContainerLoadingState()));

            var system = new GltfContainerLoadingStateSystem(componentsWriter, componentPool, gltfContainerLoadingStateComponent);
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities(manager, executors);
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(6666);

            systemUpdate = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                system.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        [TestCase(LoadingState.Finished)]
        [TestCase(LoadingState.FinishedWithError)]
        [TestCase(LoadingState.Loading)]
        [TestCase(LoadingState.NotFound)]
        [TestCase(LoadingState.Unknown)]
        public void PutComponent(LoadingState state)
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState()
            {
                LoadingState = state
            });

            systemUpdate();

            outgoingMessages.Put_Called<PBGltfContainerLoadingState>(
                entity.entityId,
                ComponentID.GLTF_CONTAINER_LOADING_STATE,
                val => val.CurrentState == state
            );
        }

        [Test]
        public void RemoveComponent()
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState()
            {
                GltfContainerRemoved = true
            });

            systemUpdate();

            outgoingMessages.Remove_Called(
                entity.entityId,
                ComponentID.GLTF_CONTAINER_LOADING_STATE
            );
        }
    }
}
