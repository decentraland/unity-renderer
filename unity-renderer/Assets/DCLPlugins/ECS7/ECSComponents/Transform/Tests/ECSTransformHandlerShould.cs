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

        [SetUp]
        public void SetUp()
        {
            entityGO = new GameObject();

            entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGO);
            entity.entityId.Returns(42);

            scene = Substitute.For<IParcelScene>();
            scene.WhenForAnyArgs(x => x.SetEntityParent(Arg.Any<long>(), Arg.Any<long>()))
                 .Do(info =>
                 {
                     long parentId = info.ArgAt<long>(1);
                     IDCLEntity parent = Substitute.For<IDCLEntity>();
                     parent.entityId.Returns(parentId);
                     entity.parent.Returns(x => parent);
                 });

            handler = new ECSTransformHandler();
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
            var model = new ECSTransform() { parentId = 999 };
            handler.OnComponentModelUpdated(scene, entity, model);
            scene.Received(1).SetEntityParent(entity.entityId, model.parentId);
            scene.ClearReceivedCalls();

            handler.OnComponentModelUpdated(scene, entity, model);
            scene.DidNotReceive().SetEntityParent(entity.entityId, model.parentId);

            model.parentId = 0;
            handler.OnComponentModelUpdated(scene, entity, model);
            scene.Received(1).SetEntityParent(entity.entityId, model.parentId);
        }
    }
}