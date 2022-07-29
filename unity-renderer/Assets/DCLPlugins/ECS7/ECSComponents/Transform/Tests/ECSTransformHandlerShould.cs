using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSTransformHandlerShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private GameObject entityGO;
        private ECSTransformHandler handler;
        private IWorldState worldState;
        private BaseVariable<Vector3> playerWorldPosition;

        [SetUp]
        public void SetUp()
        {
            entityGO = new GameObject();

            entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGO);
            entity.entityId.Returns(42);

            scene = Substitute.For<IParcelScene>();
            worldState = Substitute.For<IWorldState>();
            playerWorldPosition = Substitute.For<BaseVariable<Vector3>>();
            handler = new ECSTransformHandler(worldState, playerWorldPosition);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(entityGO);
        }

        [Test]
        public void ApplyPosition()
        {
            var model = new ECSTransform() { position = new Vector3(-100, 56, 80) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.position, entityGO.transform.localPosition);
        }

        [Test]
        public void ApplyRotation()
        {
            var model = new ECSTransform() { rotation = Quaternion.Euler(180, -90, 45) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.rotation.ToString(), entityGO.transform.localRotation.ToString());
        }

        [Test]
        public void ApplyScale()
        {
            var model = new ECSTransform() { scale = new Vector3(0.001f, 0, -10000) };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(model.scale, entityGO.transform.localScale);
        }

        [Test]
        public void ApplyParent()
        {
            var parent = Substitute.For<IDCLEntity>();
            parent.entityId.Returns(999);

            scene.GetEntityById(Arg.Any<long>()).Returns(parent);

            var model = new ECSTransform() { parentId = parent.entityId };
            handler.OnComponentModelUpdated(scene, entity, model);
            entity.Received(1).SetParent(parent);
            entity.ClearReceivedCalls();

            handler.OnComponentModelUpdated(scene, entity, model);
            entity.DidNotReceive().SetParent(parent);

            model.parentId = 0;
            handler.OnComponentModelUpdated(scene, entity, model);
            entity.Received(1).SetParent(null);
        }

        [Test]
        public void AddAsOrphanIfParentDontExist()
        {
            scene.GetEntityById(Arg.Any<long>()).Returns(x => null);
            var model = new ECSTransform() { parentId = 43 };
            handler.OnComponentModelUpdated(scene, entity, model);
            entity.DidNotReceive().SetParent(Arg.Any<IDCLEntity>());
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }

        [Test]
        public void RemoveAsOrphanOnComponentRemove()
        {
            ECSTransformUtils.orphanEntities.Add(entity, new ECSTransformUtils.OrphanEntity(scene, entity, 1));
            handler.OnComponentRemoved(scene, entity);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }

        [Test]
        public void RemoveAsOrphanOnParentingChanged()
        {
            scene.GetEntityById(Arg.Any<long>()).Returns(x => null);

            var model = new ECSTransform() { parentId = 43 };
            handler.OnComponentModelUpdated(scene, entity, model);

            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));

            var parent = Substitute.For<IDCLEntity>();
            parent.entityId.Returns(999);

            scene.GetEntityById(Arg.Any<long>()).Returns(parent);

            model.parentId = parent.entityId;
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }

        [Test]
        public void MoveCharacterWhenSameSceneAndValidPosition()
        {
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { id = "temptation", basePosition = Vector2Int.zero });
            scene.IsInsideSceneBoundaries(Arg.Any<Vector2Int>()).Returns(true);
            worldState.currentSceneId.Returns("temptation");
            entity.entityId.Returns(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(8, 0, 0);
            handler.OnComponentModelUpdated(scene, entity, new ECSTransform() { position = position });
            playerWorldPosition.Received(1).Set(Arg.Do<Vector3>(x => Assert.AreEqual(position, x)));
        }

        [Test]
        public void NotMoveCharacterWhenSameSceneButInvalidPosition()
        {
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { id = "temptation", basePosition = Vector2Int.zero });
            scene.IsInsideSceneBoundaries(Arg.Any<Vector2Int>()).Returns(false);
            worldState.currentSceneId.Returns("temptation");
            entity.entityId.Returns(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(1000, 0, 0);
            handler.OnComponentModelUpdated(scene, entity, new ECSTransform() { position = position });
            playerWorldPosition.DidNotReceive().Set(Arg.Any<Vector3>());
        }

        [Test]
        public void NotMoveCharacterWhenPlayerIsNotInScene()
        {
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { id = "temptation", basePosition = Vector2Int.zero });
            scene.IsInsideSceneBoundaries(Arg.Any<Vector2Int>()).Returns(true);
            worldState.currentSceneId.Returns("NOTtemptation");
            entity.entityId.Returns(SpecialEntityId.PLAYER_ENTITY);

            Vector3 position = new Vector3(1000, 0, 0);
            handler.OnComponentModelUpdated(scene, entity, new ECSTransform() { position = position });
            playerWorldPosition.DidNotReceive().Set(Arg.Any<Vector3>());
        }
    }
}