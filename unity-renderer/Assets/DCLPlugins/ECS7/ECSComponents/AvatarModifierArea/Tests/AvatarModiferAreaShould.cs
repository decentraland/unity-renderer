using System.Collections.Generic;
using DCL.CRDT;
using DCL.ECSRuntime;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ECSComponents.Test
{
    public class AvatarModifierAreaShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private AvatarModifierAreaComponentHandler componentHandler;
        private InternalECSComponents internalComponents;
        private AvatarModifierFactory factory = new AvatarModifierFactory();

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            componentHandler = new AvatarModifierAreaComponentHandler(internalComponents.AvatarModifierAreaComponent, factory);
        }

        [TearDown]
        protected void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            internalComponents.AvatarModifierAreaComponent.RemoveFor(scene, entity);
            testUtils.Dispose();
        }

        [Test]
        public void CreateInternalComponentCorrectly()
        {
            var internalComponent = internalComponents.AvatarModifierAreaComponent.GetFor(scene, entity);
            Assert.IsNull(internalComponent);
            string excludedId1 = "AAA123456";
            string excludedId2 = "BBB456789";

            var model = new PBAvatarModifierArea()
            {
                Area =  new Decentraland.Common.Vector3() { X = 4f, Y = 2.5f, Z = 4f },
                ExcludeIds = { excludedId1, excludedId2 },
                Modifiers = { AvatarModifierType.AmtHideAvatars }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalComponent = internalComponents.AvatarModifierAreaComponent.GetFor(scene, entity);
            Assert.IsNotNull(internalComponent);
            Assert.AreEqual(new Vector3(4f, 2.5f, 4f), internalComponent.Value.model.area);
            Assert.IsNotNull(internalComponent.Value.model.OnAvatarEnter);
            Assert.IsNotNull(internalComponent.Value.model.OnAvatarExit);
            Assert.IsTrue(internalComponent.Value.model.excludedIds.Contains(excludedId1.ToLower()));
            Assert.IsTrue(internalComponent.Value.model.excludedIds.Contains(excludedId2.ToLower()));
        }

        [Test]
        [TestCase(AvatarModifierType.AmtHideAvatars)]
        [TestCase(AvatarModifierType.AmtDisablePassports)]
        public void SetupModifierEventsCorrectly(AvatarModifierType modifierType)
        {
            var model = new PBAvatarModifierArea()
            {
                Area =  new Decentraland.Common.Vector3() { X = 4f, Y = 2.5f, Z = 4f },
                Modifiers = { modifierType }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            var internalComponent = internalComponents.AvatarModifierAreaComponent.GetFor(scene, entity);
            Assert.IsTrue(factory.GetOrCreateAvatarModifier(modifierType).ApplyModifier == internalComponent.Value.model.OnAvatarEnter);
            Assert.IsTrue(factory.GetOrCreateAvatarModifier(modifierType).RemoveModifier == internalComponent.Value.model.OnAvatarExit);
        }
    }
}
