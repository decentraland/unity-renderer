using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.GltfContainerLoadingStateSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class GltfContainerLoadingStateSystemShould
    {
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;
        private IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private IECSComponentWriter componentWriter;
        private IParcelScene scene;
        private IDCLEntity entity;
        private Action systemUpdate;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(manager, factory, executors);
            gltfContainerLoadingStateComponent = internalComponents.GltfContainerLoadingStateComponent;
            componentWriter = Substitute.For<IECSComponentWriter>();
            var system = new GltfContainerLoadingStateSystem(componentWriter, gltfContainerLoadingStateComponent);
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

            componentWriter.Received(1)
                           .PutComponent(
                                Arg.Is<int>(val => val == scene.sceneData.sceneNumber),
                                Arg.Is<long>(val => val == entity.entityId),
                                Arg.Is<int>(val => val == ComponentID.GLTF_CONTAINER_LOADING_STATE),
                                Arg.Is<PBGltfContainerLoadingState>(val => val.CurrentState == state),
                                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
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

            componentWriter.Received(1)
                           .RemoveComponent(
                                Arg.Is<int>(val => val == scene.sceneData.sceneNumber),
                                Arg.Is<long>(val => val == entity.entityId),
                                Arg.Is<int>(val => val == ComponentID.GLTF_CONTAINER_LOADING_STATE),
                                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
                            );
        }

        [Test]
        public void WriteOnce()
        {
            gltfContainerLoadingStateComponent.PutFor(scene, entity, new InternalGltfContainerLoadingState()
            {
                LoadingState = LoadingState.Loading
            });

            systemUpdate();
            systemUpdate();
            systemUpdate();

            componentWriter.Received(1)
                           .PutComponent(
                                Arg.Any<int>(),
                                Arg.Any<long>(),
                                Arg.Any<int>(),
                                Arg.Any<PBGltfContainerLoadingState>(),
                                Arg.Any<ECSComponentWriteType>()
                            );
        }
    }
}
