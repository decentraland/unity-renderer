using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;
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
            scene = sceneTestHelper.CreateScene("temptation");
            entity = scene.CreateEntity(42);

            handler = new ECSTransformHandler(0, Substitute.For<IWorldState>(),
                Substitute.For<BaseVariable<Vector3>>(),
                Substitute.For<IECSComponentWriter>());
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
    }
}