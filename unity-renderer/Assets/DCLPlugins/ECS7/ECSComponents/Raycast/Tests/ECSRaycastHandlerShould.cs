using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Raycast;
using ECSSystems.ECSRaycastSystem;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class ECSRaycastHandlerShould
    {
        private ECSRaycastSystem system;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entityRaycaster;
        private ECS7TestEntity testEntity_PhysicsCollider;
        private ECS7TestEntity testEntity_PhysicsAndCustomCollider;
        private ECS7TestEntity testEntity_CustomCollider1;
        private ECS7TestEntity testEntity_OnPointerCollider;
        private ECS7TestEntity testEntity_CustomCollider2;

        private InternalECSComponents internalComponents;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            system = new ECSRaycastSystem(
                internalComponents.raycastComponent,
                internalComponents.physicColliderComponent,
                internalComponents.onPointerColliderComponent,
                internalComponents.customLayerColliderComponent,
                internalComponents.EngineInfo,
                new Dictionary<int,ComponentWriter>(),
                new WrappedComponentPool<IWrappedComponent<PBRaycastResult>>(0, ()=> new ProtobufWrappedComponent<PBRaycastResult>(new PBRaycastResult())));

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entityRaycaster = scene.CreateEntity(512);

            // This is normally initialized by SceneStateHandler when a scene is loaded
            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo(0, 0));
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void CreateInternalComponentCorrectly()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);

            Assert.IsNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));
        }

        [Test]
        public void CreateInternalComponentOnTimestampPropChange()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst,
                Timestamp = 5
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            system.Update();

            Assert.IsNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            raycast = raycast.Clone();
            raycast.Timestamp = 6;
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));
        }

        [Test]
        public void CreateInternalComponentOnContinuousPropChange()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst,
                Continuous = false
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            system.Update();

            Assert.IsNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            raycast = raycast.Clone();
            raycast.Continuous = true;
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));
        }

        [Test]
        public void NotCreateInternalComponentOnOtherPropsChange()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            system.Update();

            Assert.IsNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            raycast = raycast.Clone();
            raycast.MaxDistance = 3;
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.IsNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));
        }
    }
}

