using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
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
    public class GlobalInputShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent;
        private IInternalECSComponent<InternalPointerEvents> pointerEventsComponent;
        private InternalECSComponents internalComponents;

        private Collider colliderEntity1;
        private Collider colliderEntity2;

        private ECS7TestScene scene;
        private ECS7TestScene scene2;
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

            var worldState = Substitute.For<IWorldState>();
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
            scene2 = testUtils.CreateScene(667);
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            dataStoreEcs7.scenes.Add(scene);
            dataStoreEcs7.scenes.Add(scene2);

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

            // entity's scene does not receive input as global since it's entity input because entity with
            // pointer events was hit
            Assert.IsNull(inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            // second scene receive the pointer down event without hit info
            var result = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt = result.model.events[0];

            Assert.IsTrue(evt.type == PointerEventType.PetDown);
            Assert.IsTrue(evt.button == InputAction.IaPrimary);
            Assert.IsNull(evt.hit);
        }

        [Test]
        public void DetectPointerDown_WithoutPointerEvents()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // both scene receive input as global since entity without pointer events was hit
            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt1 = result.model.events[0];
            var result2 = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt2 = result2.model.events[0];

            Assert.IsTrue(evt1.type == PointerEventType.PetDown);
            Assert.IsTrue(evt1.button == InputAction.IaPrimary);
            Assert.IsNull(evt1.hit);
            Assert.IsTrue(evt2.type == PointerEventType.PetDown);
            Assert.IsTrue(evt2.button == InputAction.IaPrimary);
            Assert.IsNull(evt2.hit);
        }

        [Test]
        public void DetectPointerDown_WithPointerEvents_OutsideRange()
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

            // both scene receive input as global since entity with pointer events but outside of `distance` range was hit
            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt1 = result.model.events[0];
            var result2 = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt2 = result2.model.events[0];

            Assert.IsTrue(evt1.type == PointerEventType.PetDown);
            Assert.IsTrue(evt1.button == InputAction.IaPrimary);
            Assert.IsNull(evt1.hit);
            Assert.IsTrue(evt2.type == PointerEventType.PetDown);
            Assert.IsTrue(evt2.button == InputAction.IaPrimary);
            Assert.IsNull(evt2.hit);
        }

        [Test]
        public void DetectPointerUp_WithPointerEvents()
        {
            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false)),
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

            // entity's scene does not receive input as global since it's entity input because entity with
            // pointer events was hit
            Assert.IsNull(inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            // second scene receive the pointer down event without hit info
            var result = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt = result.model.events[0];

            Assert.IsTrue(evt.type == PointerEventType.PetDown);
            Assert.IsTrue(evt.button == InputAction.IaPrimary);
            Assert.IsNull(evt.hit);
        }

        [Test]
        public void DetectPointerUp_WithoutPointerEvents()
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

            // both scene receive input as global since entity without pointer events was hit
            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt1 = result.model.events[1]; // result.model.events[0] == pointer down
            var result2 = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt2 = result2.model.events[1]; // result.model.events[0] == pointer down

            Assert.IsTrue(evt1.type == PointerEventType.PetUp);
            Assert.IsTrue(evt1.button == InputAction.IaPrimary);
            Assert.IsNull(evt1.hit);
            Assert.IsTrue(evt2.type == PointerEventType.PetUp);
            Assert.IsTrue(evt2.button == InputAction.IaPrimary);
            Assert.IsNull(evt2.hit);
        }

        [Test]
        public void DetectPointerUp_WithPointerEvents_OutsideRange()
        {
            const float DISTANCE = 10;

            pointerEventsComponent.PutFor(scene, entity1, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPrimary, string.Empty, float.MaxValue, false)),
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

            // both scene receive input as global since entity with pointer events but outside of `distance` range was hit
            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt1 = result.model.events[0]; // result.model.events[0] since input down was a valid entity input
            var result2 = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt2 = result2.model.events[1]; // result.model.events[0] == pointer down

            Assert.IsTrue(evt1.type == PointerEventType.PetUp);
            Assert.IsTrue(evt1.button == InputAction.IaPrimary);
            Assert.IsNull(evt1.hit);
            Assert.IsTrue(evt2.type == PointerEventType.PetUp);
            Assert.IsTrue(evt2.button == InputAction.IaPrimary);
            Assert.IsNull(evt2.hit);
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

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt1 = result.model.events[0]; // result.model.events[0] since input up was a valid entity input
            var result2 = inputEventResultsComponent.GetFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY);
            var evt2 = result2.model.events[1]; // result.model.events[0] == pointer down

            Assert.IsTrue(evt1.type == PointerEventType.PetDown);
            Assert.IsTrue(evt1.button == InputAction.IaPrimary);
            Assert.IsNull(evt1.hit);
            Assert.IsTrue(evt2.type == PointerEventType.PetUp);
            Assert.IsTrue(evt2.button == InputAction.IaPrimary);
            Assert.IsNull(evt2.hit);
            Assert.AreEqual(1, result.model.events.Count);
            Assert.AreEqual(2, result2.model.events.Count);
        }
    }
}
