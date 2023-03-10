using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCLPlugins.ECSComponents.Raycast;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ECSRaycastShould
    {
        private IECSComponentWriter componentWriter;

        private ECS7TestUtilsScenesAndEntities testUtils;

        private ECS7TestScene scene;
        private ECS7TestEntity entityRaycaster;
        private ECS7TestEntity entityCollider1;
        private ECS7TestEntity entityCollider2;

        private InternalECSComponents internalComponents;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            componentWriter = Substitute.For<IECSComponentWriter>();

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);

            entityRaycaster = scene.CreateEntity(512);

            entityCollider1 = scene.CreateEntity(513);
            entityCollider1.gameObject.transform.position =
                PositionUtils.WorldToUnityPosition(new Vector3(8, 1, 8));
            var colliderEntity1 =  entityCollider1.gameObject.AddComponent<BoxCollider>();
            internalComponents.physicColliderComponent.PutFor(scene, entityCollider1,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity1 } });


            entityCollider2 = scene.CreateEntity(514);
            entityCollider2.gameObject.transform.position =
                PositionUtils.WorldToUnityPosition(new Vector3(8, 1, 12));
            var colliderEntity2 = entityCollider2.gameObject.AddComponent<BoxCollider>();
            internalComponents.physicColliderComponent.PutFor(scene, entityCollider2,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity2 } });


        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void HitNothing()
        {
            PBRaycast raycast = new PBRaycast()
            {
                Origin = new Decentraland.Common.Vector3() { X = 12.0f, Y = 0.5f, Z = 0.0f },
                Direction = new Decentraland.Common.Vector3() { X = .0f, Y = .0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHander = new RaycastComponentHandler(componentWriter, internalComponents.physicColliderComponent);
            raycastHander.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 0),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );
        }

        [Test]
        public void HitOneEntity()
        {
            PBRaycast raycast = new PBRaycast()
            {
                Origin = new Decentraland.Common.Vector3() { X = 8.0f, Y = 0.5f, Z = 0.0f },
                Direction = new Decentraland.Common.Vector3() { X = .0f, Y = .0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHander = new RaycastComponentHandler(componentWriter, internalComponents.physicColliderComponent);

            raycastHander.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );
        }

        [Test]
        public void HitTwoEntities()
        {
            PBRaycast raycast = new PBRaycast()
            {
                Origin = new Decentraland.Common.Vector3() { X = 8.0f, Y = 0.5f, Z = 0.0f },
                Direction = new Decentraland.Common.Vector3() { X = .0f, Y = .0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll
            };

            RaycastComponentHandler raycastHander = new RaycastComponentHandler(componentWriter, internalComponents.physicColliderComponent);

            raycastHander.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 2),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );

        }

        [UnityTest]
        public IEnumerator ReturnResultsWithDifferentTimestamps()
        {
            RaycastComponentHandler.ResetRaycastResponseTimestamp();

            PBRaycast raycast = new PBRaycast()
            {
                Origin = new Decentraland.Common.Vector3() { X = 8.0f, Y = 0.5f, Z = 0.0f },
                Direction = new Decentraland.Common.Vector3() { X = .0f, Y = .0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHander = new RaycastComponentHandler(componentWriter, internalComponents.physicColliderComponent);

            raycastHander.OnComponentModelUpdated(scene, entityRaycaster, raycast);
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && e.Timestamp == 0),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );

            yield return null;

            raycastHander.OnComponentModelUpdated(scene, entityRaycaster, raycast);
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && e.Timestamp == 1),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );

            ECS7TestEntity entityRaycaster2 = scene.CreateEntity(666);
            raycastHander.OnComponentModelUpdated(scene, entityRaycaster2, raycast);
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster2.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && e.Timestamp == 2),
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );
        }
    }
}
