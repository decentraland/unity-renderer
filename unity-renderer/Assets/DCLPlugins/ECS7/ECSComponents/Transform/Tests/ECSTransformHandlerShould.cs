using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ECSTransformHandlerShould
    {
        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECSTransformHandler handler;
        private IWorldState worldState;
        private IBaseVariable<Vector3> playerTeleportPosition;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene(666);
            entity = scene.CreateEntity(42);

            worldState = Substitute.For<IWorldState>();
            playerTeleportPosition = Substitute.For<IBaseVariable<Vector3>>();

            var sbcInternalComponent = Substitute.For<IInternalECSComponent<InternalSceneBoundsCheck>>();
            handler = new ECSTransformHandler(sbcInternalComponent);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void ApplyPosition()
        {
            var model = new ECSTransform() { position = new Vector3(-100, 56, 80) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.position, entity.gameObject.transform.localPosition);
        }

        [Test]
        public void ApplyRotation()
        {
            var model = new ECSTransform() { rotation = Quaternion.Euler(180, -90, 45) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.rotation.ToString(), entity.gameObject.transform.localRotation.ToString());
        }

        [Test]
        public void ApplyScale()
        {
            var model = new ECSTransform() { scale = new Vector3(0.001f, 0, -10000) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.scale, entity.gameObject.transform.localScale);
        }

        [Test]
        public void ApplyParent()
        {
            var parent = scene.CreateEntity(999);

            var model = new ECSTransform() { parentId = parent.entityId };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(parent.entityId, entity.parentId);
            Assert.AreEqual(parent.entityId, ECSTransformUtils.orphanEntities[entity].parentId);

            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(parent.entityId, entity.parentId);
            Assert.AreEqual(parent.entityId, ECSTransformUtils.orphanEntities[entity].parentId);

            model.parentId = 0;
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(0, entity.parentId);
            Assert.AreEqual(0, ECSTransformUtils.orphanEntities[entity].parentId);
        }

        [Test]
        public void RemoveAsOrphanOnComponentRemove()
        {
            ECSTransformUtils.orphanEntities.Add(entity, new ECSTransformUtils.OrphanEntity(scene, entity, 1));
            handler.OnComponentRemoved(scene, entity);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }

        [Test]
        public void SetChildrenAsOrphanOnParentComponentRemoved()
        {
            Vector3 localPositionParent = new Vector3(1, 0, 0);
            Vector3 localPositionChild = new Vector3(1, 0, 0);

            ECS7TestEntity parent = scene.CreateEntity(44);

            handler.OnComponentModelUpdated(scene, parent, new ECSTransform()
            {
                position = localPositionParent
            });

            handler.OnComponentModelUpdated(scene, entity, new ECSTransform()
            {
                parentId = parent.entityId,
                position = localPositionChild
            });

            var parentingSystemUpdate = ECSTransformParentingSystem.CreateSystem(Substitute.For<IInternalECSComponent<InternalSceneBoundsCheck>>());
            parentingSystemUpdate();

            Assert.AreEqual(1, parent.childrenId.Count);
            Assert.AreEqual(parent.entityId, entity.parentId);
            Assert.AreEqual(parent.gameObject.transform, entity.gameObject.transform.parent);
            Assert.AreEqual(localPositionParent + localPositionChild, entity.gameObject.transform.position);

            handler.OnComponentRemoved(scene, parent);
            Assert.AreEqual(0, parent.childrenId.Count);
            Assert.AreEqual(parent.entityId, entity.parentId);
            Assert.AreEqual(localPositionChild, entity.gameObject.transform.position);
        }

        [Test]
        public void NotMoveCharacter()
        {
            worldState.GetCurrentSceneNumber().Returns(scene.sceneData.sceneNumber);
            var playerEntity = scene.CreateEntity(SpecialEntityId.PLAYER_ENTITY);
            var previousPlayerPosition = playerEntity.gameObject.transform.position;

            Vector3 newPosition = new Vector3(8, 0, 0);
            handler.OnComponentModelUpdated(scene, playerEntity, new ECSTransform() { position = newPosition });

            playerTeleportPosition.DidNotReceive().Set(Arg.Any<Vector3>(), true);
            Assert.AreEqual(previousPlayerPosition, playerEntity.gameObject.transform.position);
        }

        [Test]
        public void CheckForCircularParentingCorreclty()
        {
            var e0 = scene.CreateEntity(10);
            var e1 = scene.CreateEntity(11);
            var e2 = scene.CreateEntity(12);
            var e3 = scene.CreateEntity(13);

            e1.parentId = e0.entityId;
            e2.parentId = e1.entityId;
            e3.parentId = e2.entityId;

            //e3->e2->e1->e0

            Assert.IsFalse(ECSTransformUtils.IsCircularParenting(scene, e0, e0.parentId));
            Assert.IsFalse(ECSTransformUtils.IsCircularParenting(scene, e1, e1.parentId));
            Assert.IsFalse(ECSTransformUtils.IsCircularParenting(scene, e2, e2.parentId));
            Assert.IsFalse(ECSTransformUtils.IsCircularParenting(scene, e3, e3.parentId));

            e3.parentId = e3.entityId;

            //e3->!3e
            Assert.IsTrue(ECSTransformUtils.IsCircularParenting(scene, e3, e3.parentId));

            e3.parentId = e2.entityId;
            e0.parentId = e3.entityId;

            //e3->e2->e1->e0->!e3

            Assert.IsTrue(ECSTransformUtils.IsCircularParenting(scene, e0, e0.parentId));
            Assert.IsTrue(ECSTransformUtils.IsCircularParenting(scene, e1, e1.parentId));
            Assert.IsTrue(ECSTransformUtils.IsCircularParenting(scene, e2, e2.parentId));
            Assert.IsTrue(ECSTransformUtils.IsCircularParenting(scene, e3, e3.parentId));
        }

        [Test]
        public void NotApplyParentingWhenCyclic()
        {
            const long CYCLIC_PARENT_ID = 12;

            var e0 = scene.CreateEntity(10);
            var e1 = scene.CreateEntity(11);

            handler.OnComponentModelUpdated(scene, e1, new ECSTransform() { parentId = e0.entityId });
            Assert.AreEqual(e0.entityId, e1.parentId);
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(e1));

            handler.OnComponentModelUpdated(scene, e0, new ECSTransform() { parentId = CYCLIC_PARENT_ID });
            Assert.AreEqual(CYCLIC_PARENT_ID, e0.parentId);
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(e0));

            var cyclicParent = scene.CreateEntity(CYCLIC_PARENT_ID);
            LogAssert.Expect(LogType.Error, new Regex(""));
            handler.OnComponentModelUpdated(scene, cyclicParent, new ECSTransform() { parentId = e1.entityId });
            Assert.AreNotEqual(e1.entityId, cyclicParent.parentId);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(cyclicParent));
        }
    }
}
