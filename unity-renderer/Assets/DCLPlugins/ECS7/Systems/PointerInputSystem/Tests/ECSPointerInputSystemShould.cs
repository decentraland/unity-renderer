using System;
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
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSPointerInputSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;
        private IECSComponentWriter componentWriter;

        private DataStore_Cursor dataStoreCursor;
        private DataStore_ECS7 dataStoreEcs7;

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
            componentWriter = Substitute.For<IECSComponentWriter>();

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                componentWriter, internalComponents);

            var pointerDownComponent = (ECSComponent<PBOnPointerDown>)componentsManager.GetOrCreateComponent(ComponentID.ON_POINTER_DOWN);
            var pointerUpComponent = (ECSComponent<PBOnPointerUp>)componentsManager.GetOrCreateComponent(ComponentID.ON_POINTER_UP);

            var componentGroups = new ComponentGroups(componentsManager);

            dataStoreCursor = new DataStore_Cursor();
            dataStoreEcs7 = new DataStore_ECS7();

            systemUpdate = ECSPointerInputSystem.CreateSystem(componentWriter,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup,
                internalComponents.onPointerColliderComponent,
                pointerDownComponent,
                pointerUpComponent,
                dataStoreEcs7,
                dataStoreCursor);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);

            scene = testUtils.CreateScene("temptation");
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            PBOnPointerDown pointerDown1 = new PBOnPointerDown()
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

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_DOWN, scene, entity1,
                ProtoSerialization.Serialize(pointerDown1));

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_UP, scene, entity2,
                ProtoSerialization.Serialize(pointerUp2));

            var colliderGO1 = new GameObject("collider1");
            var colliderGO2 = new GameObject("collider1");

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
        public void PutButtonDownResult()
        {
            RaycastResultInfo raycastInfo = new RaycastResultInfo()
            {
                hitInfo = new RaycastHitInfo()
                {
                    hit = new HitInfo()
                    {
                        collider = colliderEntity1,
                        distance = 10
                    }
                }
            };

            dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Pointer, true, raycastInfo);
            systemUpdate();

            componentWriter.Received(1)
                           .PutComponent(scene.sceneData.id,
                               entity1.entityId,
                               ComponentID.ON_POINTER_DOWN_RESULT,
                               Arg.Any<PBOnPointerDownResult>(),
                               ECSComponentWriteType.SEND_TO_SCENE);

            dataStoreEcs7.lastPointerInputEvent = null;
            systemUpdate();
        }

        [Test]
        public void PutButtonUpResult()
        {
            RaycastResultInfo raycastInfo = new RaycastResultInfo()
            {
                hitInfo = new RaycastHitInfo()
                {
                    hit = new HitInfo()
                    {
                        collider = colliderEntity2,
                        distance = 10
                    }
                }
            };

            dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Primary, true, raycastInfo);
            systemUpdate();

            componentWriter.DidNotReceive()
                           .PutComponent(Arg.Any<string>(),
                               Arg.Any<long>(),
                               Arg.Any<int>(),
                               Arg.Any<PBOnPointerDownResult>(),
                               Arg.Any<ECSComponentWriteType>());

            dataStoreEcs7.lastPointerInputEvent = new DataStore_ECS7.PointerEvent((int)ActionButton.Primary, false, raycastInfo);
            systemUpdate();

            componentWriter.Received(1)
                           .PutComponent(scene.sceneData.id,
                               entity2.entityId,
                               ComponentID.ON_POINTER_UP_RESULT,
                               Arg.Any<PBOnPointerUpResult>(),
                               ECSComponentWriteType.SEND_TO_SCENE);

            dataStoreEcs7.lastPointerInputEvent = null;
            systemUpdate();
        }
    }
}