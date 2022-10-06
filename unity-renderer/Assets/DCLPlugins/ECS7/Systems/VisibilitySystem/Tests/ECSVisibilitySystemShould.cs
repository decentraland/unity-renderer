using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using ECSSystems.VisibilitySystem;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSVisibilitySystemShould
    {
        private ECS7TestScene scene0;
        private ECS7TestScene scene1;
        private InternalECSComponents internalEcsComponents;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemsUpdate;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);

            var visibilityGroup = componentsManager.CreateComponentGroup<InternalRenderers, InternalVisibility>
                ((int)InternalECSComponentsId.RENDERERS, (int)InternalECSComponentsId.VISIBILITY);

            var visibilitySystemUpdate = ECSVisibilitySystem.CreateSystem(visibilityGroup,
                internalEcsComponents.renderersComponent,
                internalEcsComponents.visibilityComponent);

            systemsUpdate = () =>
            {
                visibilitySystemUpdate();
                internalEcsComponents.WriteSystemUpdate();
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            scene0 = testUtils.CreateScene("temptation0");
            scene1 = testUtils.CreateScene("temptation1");
        }

        [TearDown]
        public void TearDown()
        {
            testUtils?.Dispose();
        }

        [Test]
        public void ApplyVisibility()
        {
            var renderersComponent = internalEcsComponents.renderersComponent;
            var visibilityComponent = internalEcsComponents.visibilityComponent;

            ECS7TestEntity entity00 = scene0.CreateEntity(100);
            ECS7TestEntity entity01 = scene0.CreateEntity(101);
            ECS7TestEntity entity10 = scene1.CreateEntity(200);

            Renderer renderer00 = entity00.gameObject.AddComponent<MeshRenderer>();
            Renderer renderer01 = entity01.gameObject.AddComponent<MeshRenderer>();
            Renderer renderer10 = entity10.gameObject.AddComponent<MeshRenderer>();

            // add texturizable component
            renderersComponent.PutFor(scene0, entity00, new InternalRenderers()
            {
                renderers = new List<Renderer>() { renderer00 }
            });
            renderersComponent.PutFor(scene0, entity01, new InternalRenderers()
            {
                renderers = new List<Renderer>() { renderer01 }
            });
            renderersComponent.PutFor(scene1, entity10, new InternalRenderers()
            {
                renderers = new List<Renderer>() { renderer10 }
            });

            // update system
            systemsUpdate();

            // all renderers should be enabled, since we didn't add a visibility component
            Assert.IsTrue(renderer00.enabled);
            Assert.IsTrue(renderer01.enabled);
            Assert.IsTrue(renderer10.enabled);

            // create visibility component for scene0 entities
            visibilityComponent.PutFor(scene0, entity00, new InternalVisibility()
            {
                visible = true
            });
            visibilityComponent.PutFor(scene0, entity01, new InternalVisibility()
            {
                visible = false
            });

            // update system
            systemsUpdate();

            // scene0's entities should have been affected by the visibility
            Assert.IsTrue(renderer00.enabled);
            Assert.IsFalse(renderer01.enabled);

            // components should not be dirty anymore
            Assert.IsFalse(visibilityComponent.GetFor(scene0, entity00).model.dirty);
            Assert.IsFalse(visibilityComponent.GetFor(scene0, entity01).model.dirty);
            Assert.IsFalse(renderersComponent.GetFor(scene0, entity00).model.dirty);
            Assert.IsFalse(renderersComponent.GetFor(scene0, entity01).model.dirty);

            // entity from scene1 should not have changed
            Assert.IsTrue(renderer10.enabled);

            // create visibility for scene1 entity
            visibilityComponent.PutFor(scene1, entity10, new InternalVisibility()
            {
                visible = false
            });

            // update system
            systemsUpdate();

            // scene1's entity should have been affected by the visibility
            Assert.IsFalse(renderer10.enabled);

            // components should not be dirty anymore
            Assert.IsFalse(visibilityComponent.GetFor(scene1, entity10).model.dirty);
            Assert.IsFalse(renderersComponent.GetFor(scene1, entity10).model.dirty);

            // change visibility for scene1 entity
            visibilityComponent.PutFor(scene1, entity10, new InternalVisibility()
            {
                visible = true
            });

            // update system
            systemsUpdate();

            // Check final state
            Assert.IsTrue(renderer00.enabled);
            Assert.IsFalse(renderer01.enabled);
            Assert.IsTrue(renderer10.enabled);
        }
    }
}