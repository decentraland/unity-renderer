using System.Collections.Generic;
using DCL;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class PointerDownUpProcessorShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private InternalECSComponents internalComponents;
        private ECSComponentsManager componentsManager;

        private ECSComponent<PBOnPointerDown> pointerDownComponent;
        private ECSComponent<PBOnPointerUp> pointerUpComponent;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory);
            var componentWriter = Substitute.For<IECSComponentWriter>();

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                componentWriter, internalComponents);

            pointerDownComponent = (ECSComponent<PBOnPointerDown>)componentsManager.GetOrCreateComponent(ComponentID.ON_POINTER_DOWN);
            pointerUpComponent = (ECSComponent<PBOnPointerUp>)componentsManager.GetOrCreateComponent(ComponentID.ON_POINTER_UP);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void GetPointerDownEntity()
        {
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            ECS7TestEntity entity1 = scene.CreateEntity(10111);
            ECS7TestEntity entity2 = scene.CreateEntity(10112);

            PBOnPointerDown pointerDown1 = new PBOnPointerDown()
            {
                Button = ActionButton.Pointer,
                HoverText = "temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            PBOnPointerDown pointerDown2 = new PBOnPointerDown()
            {
                Button = ActionButton.Primary,
                HoverText = "not-temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            GameObject colliderGO1 = new GameObject("collider1");
            GameObject colliderGO2 = new GameObject("collider1");

            Collider collider1 = colliderGO1.AddComponent<BoxCollider>();
            Collider collider2 = colliderGO2.AddComponent<BoxCollider>();

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_DOWN, scene, entity1,
                ProtoSerialization.Serialize(pointerDown1));

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_DOWN, scene, entity2,
                ProtoSerialization.Serialize(pointerDown2));

            internalComponents.onPointerColliderComponent.PutFor(scene, entity1,
                new InternalColliders() { colliders = new List<Collider>() { collider1 } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new List<Collider>() { collider2 } });

            RaycastResultInfo raycastInfo = new RaycastResultInfo()
            {
                hitInfo = new RaycastHitInfo()
                {
                    hit = new HitInfo()
                    {
                        collider = collider1,
                        distance = 10
                    }
                }
            };

            // button for entity1 don't match
            PointerInputResult result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent(1, true, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, PointerInputResult.empty);

            Assert.IsFalse(result.shouldTriggerEvent);

            // button match for entity1
            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerDown1.Button, true, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, result);

            Assert.IsTrue(result.shouldTriggerEvent);
            Assert.AreEqual(entity1.entityId, result.entityId);

            // distance don't match for entity2
            raycastInfo.hitInfo.hit.collider = collider2;
            raycastInfo.hitInfo.hit.distance = 11;

            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerDown2.Button, true, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, result);

            Assert.IsFalse(result.shouldTriggerEvent);

            // distance match for entity2
            raycastInfo.hitInfo.hit.distance = 10;
            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerDown2.Button, true, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, result);

            Assert.IsTrue(result.shouldTriggerEvent);
            Assert.AreEqual(entity2.entityId, result.entityId);

            Object.DestroyImmediate(colliderGO1);
            Object.DestroyImmediate(colliderGO2);
        }

        [Test]
        public void GetPointerUpEntity()
        {
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            ECS7TestEntity entity1 = scene.CreateEntity(10111);
            ECS7TestEntity entity2 = scene.CreateEntity(10112);

            PBOnPointerUp pointerUp1 = new PBOnPointerUp()
            {
                Button = ActionButton.Pointer,
                HoverText = "temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            PBOnPointerUp pointerUp2 = new PBOnPointerUp()
            {
                Button = ActionButton.Primary,
                HoverText = "not-temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            GameObject colliderGO1 = new GameObject("collider1");
            GameObject colliderGO2 = new GameObject("collider1");

            Collider collider1 = colliderGO1.AddComponent<BoxCollider>();
            Collider collider2 = colliderGO2.AddComponent<BoxCollider>();

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_UP, scene, entity1,
                ProtoSerialization.Serialize(pointerUp1));

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_UP, scene, entity2,
                ProtoSerialization.Serialize(pointerUp2));

            internalComponents.onPointerColliderComponent.PutFor(scene, entity1,
                new InternalColliders() { colliders = new List<Collider>() { collider1 } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new List<Collider>() { collider2 } });

            RaycastResultInfo raycastInfo = new RaycastResultInfo()
            {
                hitInfo = new RaycastHitInfo()
                {
                    hit = new HitInfo()
                    {
                        collider = collider1,
                        distance = 10
                    }
                }
            };

            PointerInputResult buttonDownResult = new PointerInputResult(scene.sceneData.id, entity2.entityId, false);

            // button down don't match entity1
            PointerInputResult result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerUp1.Button, false, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, buttonDownResult);

            Assert.IsFalse(result.shouldTriggerEvent);

            // button for entity1 don't match
            buttonDownResult = new PointerInputResult(scene.sceneData.id, entity1.entityId, false);
            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent(1, true, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, buttonDownResult);

            Assert.IsFalse(result.shouldTriggerEvent);

            // button match for entity1
            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerUp1.Button, false, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, buttonDownResult);

            Assert.IsTrue(result.shouldTriggerEvent);
            Assert.AreEqual(entity1.entityId, result.entityId);

            // distance don't match for entity2
            raycastInfo.hitInfo.hit.collider = collider2;
            raycastInfo.hitInfo.hit.distance = 11;
            buttonDownResult = new PointerInputResult(scene.sceneData.id, entity2.entityId, false);

            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerUp2.Button, false, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, buttonDownResult);

            Assert.IsFalse(result.shouldTriggerEvent);

            // distance match for entity2
            raycastInfo.hitInfo.hit.distance = 10;
            result = PointerDownUpProcessor.ProcessPointerDownUp(
                new DataStore_ECS7.PointerEvent((int)pointerUp2.Button, false, raycastInfo),
                internalComponents.onPointerColliderComponent,
                pointerDownComponent, pointerUpComponent, buttonDownResult);

            Assert.IsTrue(result.shouldTriggerEvent);
            Assert.AreEqual(entity2.entityId, result.entityId);

            Object.DestroyImmediate(colliderGO1);
            Object.DestroyImmediate(colliderGO2);
        }
    }
}