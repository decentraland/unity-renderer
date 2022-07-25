using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSTransformParentingSystemShould
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
            handler = new ECSTransformHandler();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(entityGO);
        }
        
        [Test]
        public void WaitForParentToExists()
        {
            scene.GetEntityById(Arg.Any<long>()).Returns(x => null);
            
            var model = new ECSTransform() { parentId = 43 };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));

            // parent does not exist yet, so it should keep waiting
            ECSTransformParentingSystem.Update();
            Assert.IsTrue(ECSTransformUtils.orphanEntities.ContainsKey(entity));
            
            // create parent for entity
            var parent = Substitute.For<IDCLEntity>();
            parent.entityId.Returns(model.parentId);
            scene.GetEntityById(Arg.Any<long>()).Returns(x => parent);
            
            // parent exist so it should apply parenting
            ECSTransformParentingSystem.Update();
            entity.Received(1).SetParent(parent);
            Assert.IsFalse(ECSTransformUtils.orphanEntities.ContainsKey(entity));
        }
    }
}