using DCL.Configuration;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using DCLPlugins.ECSComponents.Raycast;
using ECSSystems.ECSRaycastSystem;
using Google.Protobuf.Collections;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace Tests
{
    public class ECSRaycastSystemShould
    {
        private ECSRaycastSystem system;
        private IECSComponentWriter componentWriter;
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
            componentWriter = Substitute.For<IECSComponentWriter>();
            system = new ECSRaycastSystem(
                internalComponents.raycastComponent,
                internalComponents.physicColliderComponent,
                internalComponents.onPointerColliderComponent,
                internalComponents.customLayerColliderComponent,
                componentWriter);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entityRaycaster = scene.CreateEntity(512);

            // Test collider entities in line
            testEntity_PhysicsCollider = CreateColliderEntity(513, new Vector3(8, 1, 2), new[] { ColliderLayer.ClPhysics });
            testEntity_PhysicsCollider.gameObject.layer = PhysicsLayers.characterOnlyLayer;
            testEntity_PhysicsAndCustomCollider = CreateColliderEntity(514, new Vector3(8, 1, 5), new[] { ColliderLayer.ClPhysics, ColliderLayer.ClCustom3 });
            testEntity_PhysicsAndCustomCollider.gameObject.layer = PhysicsLayers.characterOnlyLayer;
            testEntity_CustomCollider1 = CreateColliderEntity(515, new Vector3(8, 1, 8), new[] { ColliderLayer.ClCustom7 });
            testEntity_CustomCollider1.gameObject.layer = PhysicsLayers.sdkCustomLayer;
            testEntity_OnPointerCollider = CreateColliderEntity(516, new Vector3(8, 1, 11), new[] { ColliderLayer.ClPointer });
            testEntity_OnPointerCollider.gameObject.layer = PhysicsLayers.onPointerEventLayer;
            testEntity_CustomCollider2 = CreateColliderEntity(517, new Vector3(8, 1, 14), new[] { ColliderLayer.ClCustom5, ColliderLayer.ClCustom8 });
            testEntity_CustomCollider2.gameObject.layer = PhysicsLayers.sdkCustomLayer;
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void HitNothing()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(12f, 0.5f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 0)
            );
        }

        [Test]
        public void HitOnlyClosestEntity()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst,
                CollisionMask = (int)ColliderLayer.ClPhysics
                                | (int)ColliderLayer.ClPointer
                                | (int)ColliderLayer.ClCustom3
                                | (int)ColliderLayer.ClCustom7
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && HitsContainEntity(e.Hits, testEntity_PhysicsCollider.entityId))
            );
        }

        [Test]
        public void HitOnlyPhysicsLayerEntitiesByDefault()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);

            // By default (no layermask specified) the raycast hits only 'ColliderLayer.ClPhysics'
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClPhysics
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 2
                                             && HitsContainEntity(e.Hits, testEntity_PhysicsCollider.entityId)
                                             && HitsContainEntity(e.Hits, testEntity_PhysicsAndCustomCollider.entityId))
            );
        }

        [Test]
        public void FilterByPhysicsLayer()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClPhysics
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 2
                                             && HitsContainEntity(e.Hits, testEntity_PhysicsCollider.entityId)
                                             && HitsContainEntity(e.Hits, testEntity_PhysicsAndCustomCollider.entityId))
            );
        }

        [Test]
        public void FilterByOnPointerLayer()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);

            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClPointer
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && HitsContainEntity(e.Hits, testEntity_OnPointerCollider.entityId))
            );
        }

        [Test]
        public void FilterByCustomLayer()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);

            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClCustom7
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1
                                             && HitsContainEntity(e.Hits, testEntity_CustomCollider1.entityId))
            );
        }

        [Test]
        public void ReturnNoHitsBasedOnLayers()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClCustom2
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 0)
            );
        }

        [Test]
        public void DistinguishBetweenCustomLayerEntitiesHit()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);

            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClCustom3 | (int)ColliderLayer.ClCustom8
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 2
                                             && HitsContainEntity(e.Hits, testEntity_CustomCollider2.entityId)
                                             && HitsContainEntity(e.Hits, testEntity_PhysicsAndCustomCollider.entityId))
            );
        }

        [Test]
        public void DistinguishBetweenPhysicsAndOnPointerEntitiesHit()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);

            PBRaycast raycast = new PBRaycast()
            {
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtQueryAll,
                CollisionMask = (int)ColliderLayer.ClPointer
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1
                                             && HitsContainEntity(e.Hits, testEntity_OnPointerCollider.entityId))
            );
        }

        [Test]
        public void ReturnResultWithSameRaycastTimestamp()
        {
            entityRaycaster.gameObject.transform.position = new Vector3(8f, 1f, 0.1f);
            uint raycastTimestamp = 999;
            PBRaycast raycast = new PBRaycast()
            {
                Timestamp = raycastTimestamp,
                GlobalDirection = new Decentraland.Common.Vector3() { X = 0f, Y = 0f, Z = 1.0f },
                MaxDistance = 16.0f,
                QueryType = RaycastQueryType.RqtHitFirst
            };

            RaycastComponentHandler raycastHandler = new RaycastComponentHandler(internalComponents.raycastComponent);
            raycastHandler.OnComponentCreated(scene, entityRaycaster);
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1 && e.Timestamp == raycastTimestamp)
            );
        }

        /*[Test]
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
            raycastHandler.OnComponentModelUpdated(scene, entityRaycaster, raycast);

            Assert.NotNull(internalComponents.raycastComponent.GetFor(scene, entityRaycaster));

            system.Update();
            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                entityRaycaster.entityId,
                ComponentID.RAYCAST_RESULT,
                Arg.Is<PBRaycastResult>(e => e.Hits.Count == 1)
            );
        }*/

        private ECS7TestEntity CreateColliderEntity(int entityId, Vector3 position, ColliderLayer[] layers)
        {
            var entityCollider = scene.CreateEntity(entityId);
            entityCollider.gameObject.transform.position = PositionUtils.WorldToUnityPosition(position);
            var collider =  entityCollider.gameObject.AddComponent<BoxCollider>();
            IInternalECSComponent<InternalColliders> internalCollidersComponent = internalComponents.physicColliderComponent;

            int mergedLayers = 0;
            for (var i = 0; i < layers.Length; i++)
            {
                mergedLayers |= (int)layers[i];
            }

            for (var i = 0; i < layers.Length; i++)
            {
                switch (layers[i])
                {
                    case ColliderLayer.ClPhysics:
                        internalCollidersComponent = internalComponents.physicColliderComponent;
                        break;
                    case ColliderLayer.ClPointer:
                        internalCollidersComponent = internalComponents.onPointerColliderComponent;
                        break;
                    case ColliderLayer.ClCustom1:
                    case ColliderLayer.ClCustom2:
                    case ColliderLayer.ClCustom3:
                    case ColliderLayer.ClCustom4:
                    case ColliderLayer.ClCustom5:
                    case ColliderLayer.ClCustom6:
                    case ColliderLayer.ClCustom7:
                    case ColliderLayer.ClCustom8:
                        internalCollidersComponent = internalComponents.customLayerColliderComponent;
                        break;
                }

                internalCollidersComponent.PutFor(scene, entityCollider,
                    new InternalColliders() { colliders = new KeyValueSet<Collider, int>() {{ collider, mergedLayers }}});
            }

            return entityCollider;
        }

        private bool HitsContainEntity(RepeatedField<RaycastHit> hits, long targetEntityId) =>
            hits.Count(x => x.EntityId == targetEntityId) == 1;
    }
}
