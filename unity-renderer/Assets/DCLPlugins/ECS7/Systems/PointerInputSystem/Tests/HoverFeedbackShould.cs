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
    public class HoverFeedbackShould
    {
        private const int MAX_TOOLTIPS = 2;

        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private IInternalECSComponent<InternalPointerEvents> pointerEventsComponent;
        private InternalECSComponents internalComponents;
        private IECSInteractionHoverCanvas interactionHoverCanvas;

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

            var worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<int>()).Returns(true);

            dataStoreEcs7 = new DataStore_ECS7();
            interactionHoverCanvas = Substitute.For<IECSInteractionHoverCanvas>();
            interactionHoverCanvas.tooltipsCount.Returns(MAX_TOOLTIPS);

            ECSPointerInputSystem system = new ECSPointerInputSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                internalComponents.PointerEventsComponent,
                interactionHoverCanvas,
                worldState,
                dataStoreEcs7,
                new RestrictedActionsContext());

            systemUpdate = system.Update;

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

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
        public void ShowPointerDownHoverTooltip()
        {
            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaAny,
                            "Temptation",
                            1,
                            true
                        )
                    )
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(MAX_TOOLTIPS - 1).SetTooltipActive(Arg.Is<int>(i => i != 0), false);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaAny);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        public void NotShowPointerDownHoverTooltipWhenMoreThanMaxDistance()
        {
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = 1.1f;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaAny,
                            "Temptation",
                            1,
                            true
                        ))
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void ShowPointerUpHoverTooltip()
        {
            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaPointer,
                            "Temptation",
                            1,
                            true
                        )
                    )
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();
            interactionHoverCanvas.ClearReceivedCalls();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(MAX_TOOLTIPS - 1).SetTooltipActive(Arg.Is<int>(i => i != 0), false);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaPointer);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        public void NotShowPointerUpHoverTooltipWhenMoreThanMaxDistance()
        {
            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaPointer,
                            "Temptation",
                            1,
                            true
                        )
                    )
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();
            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.ClearReceivedCalls();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = 1.1f;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void NotShowPointerUpHoverTooltipWhenButtonMismatch()
        {
            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaSecondary,
                            "Temptation",
                            1,
                            true
                        )
                    )
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // systemUpdate is called twice
            interactionHoverCanvas.Received(2 * MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void HandleSeveralHoverTooltip()
        {
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            InternalPointerEvents pointerEvents = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaAny,
                            "Temptation",
                            1,
                            true
                        )
                    ),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaPointer,
                            "Temptation2",
                            1,
                            true
                        )
                    ),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info
                        (
                            InputAction.IaPrimary,
                            "Temptation3",
                            1,
                            true
                        )
                    )
                }
            };

            pointerEventsComponent.PutFor(scene, entity1, pointerEvents);

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation3");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaPrimary);
            interactionHoverCanvas.Received(1).Show();
        }
    }
}
