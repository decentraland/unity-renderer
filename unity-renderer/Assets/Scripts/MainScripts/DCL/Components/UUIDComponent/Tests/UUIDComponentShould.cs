using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UUIDComponentShould : IntegrationTestSuite
    {
        private ParcelScene scene;

        protected override WorldRuntimeContext CreateRuntimeContext()
        {
            return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
            (
                sceneController: new SceneController(),
                state: new WorldState(),
                componentFactory: new RuntimeComponentFactory()
            );
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = Environment.i.world.sceneController.CreateTestScene() as ParcelScene;
        }

        [UnityTest]
        public IEnumerator BeDestroyedCorrectlyWhenReceivingComponentDestroyMessage()
        {
            var shape = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            IDCLEntity entity = shape.attachedEntities.First();

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };

            TestHelpers.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerDown.NAME;

            TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerUp.NAME;

            TestHelpers.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_CLICK ));
            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_UP ));
            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_DOWN ));

            scene.EntityComponentRemove(entity.entityId, OnPointerDown.NAME);
            scene.EntityComponentRemove(entity.entityId, OnPointerUp.NAME);
            scene.EntityComponentRemove(entity.entityId, OnClick.NAME);

            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_CLICK ));
            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_UP ));
            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_DOWN ));
        }
    }
}