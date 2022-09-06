using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class PointerHoverProcessorShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private IECSComponentWriter componentWriter;
        private ComponentGroups componentGroups;
        private ECSComponentsManager componentsManager;

        private Collider colliderEntity;

        private ECS7TestScene scene;
        private ECS7TestEntity entity;

        private PBOnPointerDown pointerDown;
        private PBOnPointerUp pointerUp;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory);
            componentWriter = Substitute.For<IECSComponentWriter>();

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                componentWriter, internalComponents);

            componentGroups = new ComponentGroups(componentsManager);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);

            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(10111);

            pointerDown = new PBOnPointerDown()
            {
                Button = ActionButton.Pointer,
                HoverText = "temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            pointerUp = new PBOnPointerUp()
            {
                Button = ActionButton.Primary,
                HoverText = "not-temptation",
                MaxDistance = 10,
                ShowFeedback = true
            };

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_DOWN, scene, entity,
                ProtoSerialization.Serialize(pointerDown));

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_UP, scene, entity,
                ProtoSerialization.Serialize(pointerUp));

            var colliderGO1 = new GameObject("collider1");

            colliderEntity = colliderGO1.AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(scene, entity,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity } });

        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            Object.DestroyImmediate(colliderEntity.gameObject);
        }

        [Test]
        public void HoverOnPointerDownComponent()
        {
            var result = PointerHoverProcessor.ProcessPointerHover(false,
                colliderEntity,
                pointerDown.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsTrue(result.hasValue);
            Assert.AreEqual(pointerDown.ShowFeedback, result.hasFeedback);
            Assert.AreEqual(pointerDown.HoverText, result.text);
            Assert.AreEqual(pointerDown.Button, result.buttonId);
            Assert.AreEqual(scene.sceneData.id, result.sceneId);
            Assert.AreEqual(entity.entityId, result.entityId);
        }

        [Test]
        public void NotHoverOnPointerDownComponentExceedDistance()
        {
            var result = PointerHoverProcessor.ProcessPointerHover(false,
                colliderEntity,
                pointerDown.MaxDistance + 1,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
        }

        [Test]
        public void NotHoverOnPointerDownComponentNoFeedback()
        {
            pointerDown.ShowFeedback = false;

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_DOWN, scene, entity,
                ProtoSerialization.Serialize(pointerDown));

            var result = PointerHoverProcessor.ProcessPointerHover(false,
                colliderEntity,
                pointerDown.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
        }

        [Test]
        public void NotHoverOnPointerDownComponentWhenWrongCollider()
        {
            var collider = new GameObject("collider2").AddComponent<BoxCollider>();

            var result = PointerHoverProcessor.ProcessPointerHover(false,
                collider,
                pointerDown.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
            Object.DestroyImmediate(collider.gameObject);
        }

        [Test]
        public void HoverOnPointerUpComponent()
        {
            var result = PointerHoverProcessor.ProcessPointerHover(true,
                colliderEntity,
                pointerUp.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsTrue(result.hasValue);
            Assert.AreEqual(pointerUp.ShowFeedback, result.hasFeedback);
            Assert.AreEqual(pointerUp.HoverText, result.text);
            Assert.AreEqual(pointerUp.Button, result.buttonId);
            Assert.AreEqual(scene.sceneData.id, result.sceneId);
            Assert.AreEqual(entity.entityId, result.entityId);
        }

        [Test]
        public void NotHoverOnPointerUpComponentExceedDistance()
        {
            var result = PointerHoverProcessor.ProcessPointerHover(true,
                colliderEntity,
                pointerUp.MaxDistance + 1,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
        }

        [Test]
        public void NotHoverOnPointerUpComponentNoFeedback()
        {
            pointerUp.ShowFeedback = false;

            componentsManager.DeserializeComponent(ComponentID.ON_POINTER_UP, scene, entity,
                ProtoSerialization.Serialize(pointerUp));

            var result = PointerHoverProcessor.ProcessPointerHover(true,
                colliderEntity,
                pointerUp.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
        }

        [Test]
        public void NotHoverOnPointerUpComponentWhenWrongCollider()
        {
            var collider = new GameObject("collider2").AddComponent<BoxCollider>();

            var result = PointerHoverProcessor.ProcessPointerHover(true,
                collider,
                pointerUp.MaxDistance,
                componentGroups.pointerDownGroup,
                componentGroups.pointerUpGroup);

            Assert.IsFalse(result.hasValue);
            Object.DestroyImmediate(collider.gameObject);
        }
    }
}