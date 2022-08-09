using DCL;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSTransformHandlerShould
    {
        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECSTransformHandler handler;
        private IWorldState worldState;
        private BaseVariable<Vector3> playerTeleportPosition;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;

        [SetUp]
        public void SetUp()
        {
            sceneTestHelper = new ECS7TestUtilsScenesAndEntities();
            scene = sceneTestHelper.CreateScene("temptation1");
            entity = scene.CreateEntity(42);

            worldState = Substitute.For<IWorldState>();
            playerTeleportPosition = Substitute.For<BaseVariable<Vector3>>();
            handler = new ECSTransformHandler(worldState, playerTeleportPosition);
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

            ECSTransformParentingSystem.Update();

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
        public void MoveCharacterWhenSameSceneAndValidPosition()
        {
            string sceneId = scene.sceneData.id;
            worldState.currentSceneId.Returns(sceneId);
            var playerEntity = scene.CreateEntity(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(8, 0, 0);
            handler.OnComponentModelUpdated(scene, playerEntity, new ECSTransform() { position = position });
            playerTeleportPosition.Received(1).Set(Arg.Do<Vector3>(x => Assert.AreEqual(position, x)));
        }

        [Test]
        public void NotMoveCharacterWhenSameSceneButInvalidPosition()
        {
            string sceneId = scene.sceneData.id;
            worldState.currentSceneId.Returns(sceneId);
            var playerEntity = scene.CreateEntity(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(1000, 0, 0);
            handler.OnComponentModelUpdated(scene, playerEntity, new ECSTransform() { position = position });
            playerTeleportPosition.DidNotReceive().Set(Arg.Any<Vector3>());
        }

        [Test]
        public void NotMoveCharacterWhenPlayerIsNotInScene()
        {
            worldState.currentSceneId.Returns("NOTtemptation");
            var playerEntity = scene.CreateEntity(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(1000, 0, 0);
            handler.OnComponentModelUpdated(scene, playerEntity, new ECSTransform() { position = position });
            playerTeleportPosition.DidNotReceive().Set(Arg.Any<Vector3>());
        }
    }
}