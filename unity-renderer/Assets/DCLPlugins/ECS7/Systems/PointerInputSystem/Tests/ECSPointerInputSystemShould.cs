using System;
using System.Collections.Generic;
using DCL;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSPointerInputSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private IWorldState worldState;
        private IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent;

        private Collider colliderEntity1;
        private Collider colliderEntity2;

        private ECS7TestScene scene;
        private ECS7TestEntity entity1;
        private ECS7TestEntity entity2;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory);

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);

            worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<string>()).Returns(true);

            dataStoreEcs7 = new DataStore_ECS7();

            systemUpdate = ECSPointerInputSystem.CreateSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                (ECSComponent<PBPointerEvents>)componentsManager.GetOrCreateComponent(ComponentID.POINTER_EVENTS),
                Substitute.For<IECSInteractionHoverCanvas>(),
                worldState,
                dataStoreEcs7);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            inputEventResultsComponent = internalComponents.inputEventResultsComponent;

            scene = testUtils.CreateScene("temptation");
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            var colliderGO1 = new GameObject("collider1");
            var colliderGO2 = new GameObject("collider2");

            colliderEntity1 = colliderGO1.AddComponent<BoxCollider>();
            colliderEntity2 = colliderGO2.AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(scene, entity1,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity1 } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity2 } });
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            Object.DestroyImmediate(colliderEntity1.gameObject);
            Object.DestroyImmediate(colliderEntity2.gameObject);
        }

        [Test]
        public void DetectPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.Down);
        }

        [Test]
        public void DetectPointerUpOnSameEntityAsPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.Up);
        }

        [Test]
        public void DetectPointerUpOnOtherEntityAsPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.IsFalse(enqueuedEvent.hit.HasEntityId);
        }

        [Test]
        public void DetectPointerUpWhenInputReleaseWithoutHit()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.IsFalse(enqueuedEvent.hit.HasEntityId);
        }

        [Test]
        public void DetectHoverEnter()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.HoverEnter);
        }

        [Test]
        public void DetectHoverExitWhenNewHover()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExit = result.model.events.Dequeue();
            var enqueuedEventNewHoverEnter = result.model.events.Dequeue();

            Assert.AreEqual(entity2.entityId, enqueuedEventNewHoverEnter.hit.EntityId);
            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExit.hit.EntityId);
            Assert.IsTrue(enqueuedEventNewHoverEnter.type == PointerEventType.HoverEnter);
            Assert.IsTrue(enqueuedEventHoverExit.type == PointerEventType.HoverLeave);
        }

        [Test]
        public void DetectHoverExit()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = null;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExit = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExit.hit.EntityId);
            Assert.IsTrue(enqueuedEventHoverExit.type == PointerEventType.HoverLeave);
        }

        // [Test]
        // public void PutButtonDownResult()
        // {
        //     RaycastResultInfo raycastInfo = new RaycastResultInfo()
        //     {
        //         hitInfo = new RaycastHitInfo()
        //         {
        //             hit = new HitInfo()
        //             {
        //                 collider = colliderEntity1,
        //                 distance = 10
        //             }
        //         }
        //     };
        //
        //     dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Pointer, true, raycastInfo);
        //     systemUpdate();
        //
        //     // componentWriter.Received(1)
        //     //                .PutComponent(scene.sceneData.id,
        //     //                    entity1.entityId,
        //     //                    ComponentID.ON_POINTER_DOWN_RESULT,
        //     //                    Arg.Any<PBOnPointerDownResult>(),
        //     //                    ECSComponentWriteType.SEND_TO_SCENE);
        //
        //     dataStoreEcs7.lastPointerInputEvent = null;
        //     systemUpdate();
        // }
        //
        // [Test]
        // public void PutButtonUpResult()
        // {
        //     RaycastResultInfo raycastInfo = new RaycastResultInfo()
        //     {
        //         hitInfo = new RaycastHitInfo()
        //         {
        //             hit = new HitInfo()
        //             {
        //                 collider = colliderEntity2,
        //                 distance = 10
        //             }
        //         }
        //     };
        //
        //     dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Primary, true, raycastInfo);
        //     systemUpdate();
        //
        //     // componentWriter.DidNotReceive()
        //     //                .PutComponent(Arg.Any<string>(),
        //     //                    Arg.Any<long>(),
        //     //                    Arg.Any<int>(),
        //     //                    Arg.Any<PBOnPointerDownResult>(),
        //     //                    Arg.Any<ECSComponentWriteType>());
        //
        //     dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Primary, false, raycastInfo);
        //     systemUpdate();
        //
        //     // componentWriter.Received(1)
        //     //                .PutComponent(scene.sceneData.id,
        //     //                    entity2.entityId,
        //     //                    ComponentID.ON_POINTER_UP_RESULT,
        //     //                    Arg.Any<PBOnPointerUpResult>(),
        //     //                    ECSComponentWriteType.SEND_TO_SCENE);
        //
        //     dataStoreEcs7.lastPointerInputEvent = null;
        //     systemUpdate();
        // }
    }
}