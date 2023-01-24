using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSTransformParentingSystemShould
    {
        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;
        private ECSTransformHandler handler;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(42);

            handler = new ECSTransformHandler(Substitute.For<IWorldState>(),
                Substitute.For<BaseVariable<Vector3>>(), Substitute.For<InternalECSComponent<InternalSceneBoundsCheck>>());
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void WaitForParentToExists()
        {
            var model = new ECSTransform() { parentId = 43 };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));

            // parent does not exist yet, so it should keep waiting
            ECSTransformParentingSystem.Update();
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));

            // create parent for entity
            var parent = scene.CreateEntity(model.parentId);

            // parent exist so it should apply parenting
            ECSTransformParentingSystem.Update();
            Assert.AreEqual(entity.entityId, parent.childrenId[0]);
            Assert.AreEqual(parent.gameObject.transform, entity.gameObject.transform.parent);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }

        [Test]
        public void SetEntityAsChildOfSceneRootEntity()
        {
            var model = new ECSTransform() { parentId = 23423 };
            handler.OnComponentModelUpdated(scene, entity, model);
            model = new ECSTransform() { parentId = SpecialEntityId.SCENE_ROOT_ENTITY };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));

            ECSTransformParentingSystem.Update();
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
            Assert.AreEqual(scene.GetSceneTransform(), entity.gameObject.transform.parent);
        }

        [Test]
        public void RemoveEntityWithChildren()
        {
            var childEntity = scene.CreateEntity(43);

            handler.OnComponentCreated(scene, entity);
            handler.OnComponentCreated(scene, childEntity);

            handler.OnComponentModelUpdated(scene, entity, new ECSTransform());
            handler.OnComponentModelUpdated(scene, childEntity, new ECSTransform() { parentId = entity.entityId });

            ECSTransformParentingSystem.Update();

            handler.OnComponentModelUpdated(scene, childEntity, new ECSTransform() { parentId = 0 });
            handler.OnComponentRemoved(scene, entity);
            scene.RemoveEntity(entity.entityId, true);
            ECSTransformParentingSystem.Update();
        }
    }
}
