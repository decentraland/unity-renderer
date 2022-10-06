using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using ECSSystems.MaterialSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSMaterialSystemShould
    {
        private ECS7TestScene scene0;
        private ECS7TestScene scene1;
        private InternalECSComponents internalEcsComponents;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemsUpdate;
        private Material materialResource;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);

            var texturizableGroup = componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
                ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE);

            var materialSystemUpdate = ECSMaterialSystem.CreateSystem(texturizableGroup,
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.materialComponent);

            systemsUpdate = () =>
            {
                materialSystemUpdate();
                internalEcsComponents.WriteSystemUpdate();
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            scene0 = testUtils.CreateScene("temptation0");
            scene1 = testUtils.CreateScene("temptation1");

            materialResource = Resources.Load("Materials/ShapeMaterial") as Material;
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void ApplyMaterial()
        {
            var texturizableComponent = internalEcsComponents.texturizableComponent;
            var materialComponent = internalEcsComponents.materialComponent;

            ECS7TestEntity entity00 = scene0.CreateEntity(100);
            ECS7TestEntity entity01 = scene0.CreateEntity(101);
            ECS7TestEntity entity10 = scene1.CreateEntity(200);

            Renderer renderer00 = entity00.gameObject.AddComponent<MeshRenderer>();
            Renderer renderer01 = entity01.gameObject.AddComponent<MeshRenderer>();
            Renderer renderer10 = entity10.gameObject.AddComponent<MeshRenderer>();

            Assert.IsNull(renderer00.sharedMaterial);
            Assert.IsNull(renderer01.sharedMaterial);
            Assert.IsNull(renderer10.sharedMaterial);

            // add texturizable component
            texturizableComponent.PutFor(scene0, entity00, new InternalTexturizable()
            {
                renderers = new List<Renderer>() { renderer00 }
            });
            texturizableComponent.PutFor(scene0, entity01, new InternalTexturizable()
            {
                renderers = new List<Renderer>() { renderer01 }
            });
            texturizableComponent.PutFor(scene1, entity10, new InternalTexturizable()
            {
                renderers = new List<Renderer>() { renderer10 }
            });

            // update system
            systemsUpdate();

            // nothing should change since we don't have a material yet
            Assert.IsNull(renderer00.sharedMaterial);
            Assert.IsNull(renderer01.sharedMaterial);
            Assert.IsNull(renderer10.sharedMaterial);

            // create material for scene0 entities
            Material scene0Material = new Material(materialResource);
            materialComponent.PutFor(scene0, entity00, new InternalMaterial()
            {
                material = scene0Material,
                castShadows = false
            });
            materialComponent.PutFor(scene0, entity01, new InternalMaterial()
            {
                material = scene0Material,
                castShadows = false
            });

            // update system
            systemsUpdate();

            // scene0's entities should have material set
            Assert.NotNull(renderer00.sharedMaterial);
            Assert.NotNull(renderer01.sharedMaterial);

            // should share the same material
            Assert.AreEqual(renderer00.sharedMaterial, renderer01.sharedMaterial);

            // and shadow casting set properly
            Assert.IsTrue(renderer00.shadowCastingMode == ShadowCastingMode.Off);
            Assert.IsTrue(renderer01.shadowCastingMode == ShadowCastingMode.Off);

            // components should not be dirty anymore
            Assert.IsFalse(materialComponent.GetFor(scene0, entity00).model.dirty);
            Assert.IsFalse(materialComponent.GetFor(scene0, entity01).model.dirty);
            Assert.IsFalse(texturizableComponent.GetFor(scene0, entity00).model.dirty);
            Assert.IsFalse(texturizableComponent.GetFor(scene0, entity01).model.dirty);

            // entity from scene1 should not have changed
            Assert.IsNull(renderer10.sharedMaterial);

            // create material for scene1 entity
            Material scene1Material = scene0Material;
            materialComponent.PutFor(scene1, entity10, new InternalMaterial()
            {
                material = scene1Material,
                castShadows = true
            });

            // update system
            systemsUpdate();

            // scene1's entity should have material set
            Assert.NotNull(renderer10.sharedMaterial);

            // should share the same material
            Assert.AreEqual(renderer10.sharedMaterial, renderer00.sharedMaterial);

            // and shadow casting set properly
            Assert.IsTrue(renderer10.shadowCastingMode == ShadowCastingMode.On);

            // components should not be dirty anymore
            Assert.IsFalse(materialComponent.GetFor(scene1, entity10).model.dirty);
            Assert.IsFalse(texturizableComponent.GetFor(scene1, entity10).model.dirty);

            // change material for scene1 entity
            scene1Material = new Material(materialResource);
            materialComponent.PutFor(scene1, entity10, new InternalMaterial()
            {
                material = scene1Material,
                castShadows = false
            });

            // update system
            systemsUpdate();

            Assert.AreEqual(scene0Material, renderer00.sharedMaterial);
            Assert.AreEqual(scene0Material, renderer01.sharedMaterial);
            Assert.AreEqual(scene1Material, renderer10.sharedMaterial);

            Object.DestroyImmediate(scene0Material);
            Object.DestroyImmediate(scene1Material);
        }

        [Test]
        public void PropoerlyRemoveSharedMaterialFromEntity()
        {
            var texturizableComponent = internalEcsComponents.texturizableComponent;
            var materialComponent = internalEcsComponents.materialComponent;

            ECS7TestEntity entity00 = scene0.CreateEntity(100);
            ECS7TestEntity entity10 = scene1.CreateEntity(200);

            Renderer renderer00 = entity00.gameObject.AddComponent<MeshRenderer>();
            Renderer renderer10 = entity10.gameObject.AddComponent<MeshRenderer>();

            Assert.IsNull(renderer00.sharedMaterial);
            Assert.IsNull(renderer10.sharedMaterial);

            // add texturizable component
            texturizableComponent.PutFor(scene0, entity00, new InternalTexturizable()
            {
                renderers = new List<Renderer>() { renderer00 }
            });
            texturizableComponent.PutFor(scene1, entity10, new InternalTexturizable()
            {
                renderers = new List<Renderer>() { renderer10 }
            });

            // add same material for both
            Material scene0Material = new Material(materialResource);
            materialComponent.PutFor(scene0, entity00, new InternalMaterial()
            {
                material = scene0Material,
                castShadows = false
            });
            materialComponent.PutFor(scene1, entity10, new InternalMaterial()
            {
                material = scene0Material,
                castShadows = false
            });

            // apply material
            systemsUpdate();

            // remove material for entity00
            materialComponent.RemoveFor(scene0, entity00, new InternalMaterial()
            {
                material = null,
            });

            // apply changes
            systemsUpdate();

            // entity00 should have it material removed from it renderers
            Assert.IsNull(renderer00.sharedMaterial);
        }
    }
}