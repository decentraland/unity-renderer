using DCL;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using ECSSystems.AvatarModifierAreaSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class ECSAvatarModifierAreaSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private InternalECSComponents internalComponents;
        private DataStore_Player dataStorePlayer = new DataStore_Player();
        private Action systemUpdate;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            var system = new ECSAvatarModifierAreaSystem(internalComponents.AvatarModifierAreaComponent, dataStorePlayer);
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
            internalComponents.AvatarModifierAreaComponent.RemoveFor(scene, entity);
            testUtils.Dispose();
        }

        [Test]
        public void DetectOnlyAvatarsInArea()
        {
            entity.gameObject.transform.position = new Vector3(8, 1, 8);
            Vector3 entityPosition = entity.gameObject.transform.position;

            // Create some fake avatars
            var fakeAvatar1 = CreateFakeAvatar();
            fakeAvatar1.transform.position = entityPosition + Vector3.forward;
            var fakeAvatar2 = CreateFakeAvatar();
            fakeAvatar2.transform.position = entityPosition + Vector3.back;
            var fakeAvatar3 = CreateFakeAvatar();
            fakeAvatar3.transform.position = new Vector3(100,100, 100);
            Physics.SyncTransforms();

            internalComponents.AvatarModifierAreaComponent.PutFor(scene, entity, new InternalAvatarModifierArea()
            {
                area = new Vector3(8f, 8f, 8f),
                avatarsInArea = new HashSet<GameObject>()
            });

            systemUpdate();

            var internalComponentModel = internalComponents.AvatarModifierAreaComponent.GetFor(scene, entity);

            Assert.AreEqual(2, internalComponentModel.Value.model.avatarsInArea.Count);
            Assert.IsTrue(internalComponentModel.Value.model.avatarsInArea.Contains(fakeAvatar1));
            Assert.IsTrue(internalComponentModel.Value.model.avatarsInArea.Contains(fakeAvatar2));
        }

        [Test]
        public void ApplyModifierOnEnteringAvatars()
        {
            entity.gameObject.transform.position = new Vector3(8, 1, 8);
            Vector3 entityPosition = entity.gameObject.transform.position;
            Vector3 area = new Vector3(5f, 5f, 5f);

            var fakeAvatar = CreateFakeAvatar();
            fakeAvatar.transform.position = new Vector3(100, 100, 100);
            Physics.SyncTransforms();

            bool applyCalled = false;
            bool removeCalled = false;
            void ApplyModifier(GameObject avatarGO)
            {
                Assert.AreEqual(fakeAvatar, avatarGO);
                applyCalled = true;
            }

            void RemoveModifier(GameObject avatarGO)
            {
                Assert.AreEqual(fakeAvatar, avatarGO);
                removeCalled = true;
            }

            internalComponents.AvatarModifierAreaComponent.PutFor(scene, entity, new InternalAvatarModifierArea()
            {
                area = area,
                OnAvatarEnter = ApplyModifier,
                OnAvatarExit = RemoveModifier,
                avatarsInArea = new HashSet<GameObject>()
            });

            systemUpdate();
            Assert.IsFalse(applyCalled);
            Assert.IsFalse(removeCalled);

            // move avatar inside area
            fakeAvatar.transform.position = entityPosition + (area * 0.5f);
            Physics.SyncTransforms();

            systemUpdate();
            Assert.IsTrue(applyCalled);
            Assert.IsFalse(removeCalled);
            applyCalled = false;

            // move avatar outside area
            fakeAvatar.transform.position = entityPosition + area;
            Physics.SyncTransforms();

            systemUpdate();
            Assert.IsTrue(removeCalled);
            Assert.IsFalse(applyCalled);
        }

        private GameObject CreateFakeAvatar()
        {
            GameObject fakeAvatarColliderGO = new GameObject();
            GameObject colliderGO = new GameObject();
            colliderGO.layer = LayerMask.NameToLayer("AvatarTriggerDetection");
            colliderGO.AddComponent<BoxCollider>();
            colliderGO.transform.parent = fakeAvatarColliderGO.transform;
            colliderGO.transform.localPosition = Vector3.zero;
            return fakeAvatarColliderGO;
        }
    }
}
