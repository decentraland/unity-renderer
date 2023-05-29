using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

namespace Tests
{
    // Test for input against an Entity with pointer event component
    public class EntityInputShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private IWorldState worldState;
        private IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent;
        private IInternalECSComponent<InternalPointerEvents> pointerEventsComponent;
        private InternalECSComponents internalComponents;

        private Collider colliderEntity1;
        private Collider colliderEntity2;

        private ECS7TestScene scene;
        private ECS7TestEntity entity1;
        private ECS7TestEntity entity2;

        [SetUp]
        public void SetUp()
        {
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            pointerEventsComponent = internalComponents.PointerEventsComponent;

            worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<int>()).Returns(true);

            dataStoreEcs7 = new DataStore_ECS7();

            ECSPointerInputSystem system = new ECSPointerInputSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                internalComponents.PointerEventsComponent,
                Substitute.For<IECSInteractionHoverCanvas>(),
                worldState,
                dataStoreEcs7,
                new RestrictedActionsContext());

            systemUpdate = system.Update;

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            inputEventResultsComponent = internalComponents.inputEventResultsComponent;

            scene = testUtils.CreateScene(666);
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            dataStoreEcs7.scenes.Add(scene);

            var colliderGO1 = new GameObject("collider1");
            var colliderGO2 = new GameObject("collider2");

            colliderEntity1 = colliderGO1.AddComponent<BoxCollider>();
            colliderEntity2 = colliderGO2.AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(scene, entity1,
                new InternalColliders() { colliders = new KeyValueSet<Collider, uint>() { { colliderEntity1, 0 } } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new KeyValueSet<Collider, uint>() { { colliderEntity2, 0 } } });
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            Object.DestroyImmediate(colliderEntity1.gameObject);
            Object.DestroyImmediate(colliderEntity2.gameObject);
        }

        [Test]
        public void DetectPointerDown_WithPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetDown);
        }

        [Test]
        public void DoNotDetectPointerDown_WithoutPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DoNotDetectPointerDown_WithPointerEvents_OutsideRange()
        {
            const float DISTANCE = 10;

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = DISTANCE;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, DISTANCE - 1, false))
                }
            });

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DetectPointerUp_WithPointerEvents()
        {
            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            // pointer down
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // pointer up
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetUp);
        }

        [Test]
        public void DoNotDetectPointerUp_WithoutPointerEvents()
        {
            // pointer down
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // pointer up
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DoNotDetectPointerUp_WithPointerEvents_OutsideRange()
        {
            const float DISTANCE = 10;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, DISTANCE - 1, false))
                }
            });

            // pointer down
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // pointer up
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = DISTANCE;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DetectPointerUp_WithPointerEvents_OnOtherEntityThanPointerDown()
        {
            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            // pointer down
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2; // entity2 collider
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // pointer up
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetUp);
        }

        [Test]
        public void DetectPointerHoverEnter_WithPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverEnter,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetHoverEnter);
        }

        [Test]
        public void DoNotDetectPointerHoverEnter_WithoutPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DoNotDetectPointerHoverEnter_WithPointerEvents_OutsideRange()
        {
            const float DISTANCE = 10;

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = DISTANCE;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverEnter,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, DISTANCE - 1, false))
                }
            });

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DetectPointerHoverLeave_WithPointerEvents()
        {
            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetHoverLeave);
        }

        [Test]
        public void DoNotDetectPointerHoverLeave_WithoutPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        public void DoNotDetectPointerHoverLeave_WithPointerEvents_OutsideRange()
        {
            const float DISTANCE = 10;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, DISTANCE - 1, false))
                }
            });

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hit.distance = DISTANCE;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            Assert.IsNull(inputEventResultsComponent.GetFor(scene, entity1.entityId));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(66, 66)]
        public void ReturnResultHitPositionInSceneSpace(int sceneBaseCoordX, int sceneBaseCoordY)
        {
            // 1. Set scene, entity and its collider
            Vector2Int sceneBaseCoords = new Vector2Int(sceneBaseCoordX, sceneBaseCoordY);
            var newTestScene = testUtils.CreateScene(2222, sceneBaseCoords, new List<Vector2Int>() { sceneBaseCoords });
            var testEntity = newTestScene.CreateEntity(12345);
            Collider testEntityCollider = new GameObject("testEntityCollider").AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(newTestScene, testEntity,
                new InternalColliders() { colliders = new KeyValueSet<Collider, uint>() { { testEntityCollider, 0 } } });

            pointerEventsComponent.PutFor(newTestScene, testEntity, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false))
                }
            });

            // 2. position collider entity inside scene space
            ECSTransformHandler transformHandler = new ECSTransformHandler(Substitute.For<IInternalECSComponent<InternalSceneBoundsCheck>>());

            var entityLocalPosition = new Vector3(8, 1, 8);
            var transformModel = new ECSTransform() { position = entityLocalPosition };
            transformHandler.OnComponentModelUpdated(newTestScene, testEntity, transformModel);

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = testEntityCollider;

            // 3. update pointer ray hit values with object unity position
            var entityGlobalPosition = WorldStateUtils.ConvertSceneToUnityPosition(entityLocalPosition, newTestScene);
            dataStoreEcs7.lastPointerRayHit.hit.point = entityGlobalPosition;
            dataStoreEcs7.lastPointerRayHit.ray.origin = entityGlobalPosition + Vector3.back * 3;

            // 4. Update to enqueue new events
            systemUpdate();

            var result = inputEventResultsComponent.GetFor(newTestScene, testEntity.entityId);
            var evt = result.model.events[0];

            Assert.AreEqual(testEntity.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetDown);

            // 5. Check enqueued event has correct position and origin
            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z
            }, evt.hit.Position);

            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z - 3
            }, evt.hit.GlobalOrigin);

            // 6. Clean up
            Object.DestroyImmediate(testEntityCollider.gameObject);
        }

        [Test]
        public void DetectTwoInputDown()
        {
            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaAction3, string.Empty, float.MaxValue, false)),
                }
            });

            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;
            dataStoreEcs7.inputActionState[(int)InputAction.IaAction3] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);

            var evt1 = result.model.events[0];
            Assert.AreEqual(InputAction.IaPrimary, evt1.button);
            Assert.AreEqual(entity1.entityId, evt1.hit.EntityId);
            Assert.IsTrue(evt1.type == PointerEventType.PetDown);

            var evt2 = result.model.events[1];
            Assert.AreEqual(InputAction.IaAction3, evt2.button);
            Assert.AreEqual(entity1.entityId, evt2.hit.EntityId);
            Assert.IsTrue(evt2.type == PointerEventType.PetDown);
        }
    }
}
