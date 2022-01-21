using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using DCL.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public class DebugShapesCollidersDisplayerShould
    {
        private WorldRuntimeContext worldRuntime;
        private IWorldState worldState;
        private ISceneController sceneController;

        private Dictionary<string, IParcelScene> loadedScenes = new Dictionary<string, IParcelScene>();
        private Dictionary<string, Dictionary<string, IDCLEntity>> entities = new Dictionary<string, Dictionary<string, IDCLEntity>>();

        private BaseDictionary<string, bool> isColliderDisplayEnabledVariable = new BaseDictionary<string, bool>();

        [SetUp]
        public void SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            worldState.loadedScenes.Returns(loadedScenes);

            sceneController = Substitute.For<ISceneController>();

            worldRuntime = WorldRuntimeContextFactory.CreateWithGenericMocks(worldState, sceneController);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var sceneId in entities.Keys)
            {
                foreach (var entity in entities[sceneId].Values)
                {
                    Object.Destroy(entity.gameObject);
                }
            }

            loadedScenes.Clear();
            entities.Clear();
        }

        [Test]
        public void StartWatchingSceneOnInstantiation()
        {
            CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void WaitTargetWatchSceneToBeLoaded()
        {
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.pendingScenesId.Count);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            CreateAndAddScene("temptation");
            Assert.AreEqual(0, controller.pendingScenesId.Count);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void StopWatchingSceneCorrectly()
        {
            CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            isColliderDisplayEnabledVariable.AddOrSet("temptation", false);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void ApplyEntityStyleCorrectlyWhenCollisionEnabled()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");

            AddEntity(scene, entity);
            Assert.AreEqual(1, GetCollidersDisplayingCount());
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            controller.Dispose();
        }

        [Test]
        public void ApplyEntityStyleCorrectlyWhenCollisionDisabled()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");
            entity.meshesInfo.currentShape.HasCollisions().Returns(false);

            AddEntity(scene, entity);
            Assert.AreEqual(0, GetCollidersDisplayingCount());
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            controller.Dispose();
        }

        [Test]
        public void ApplyEntityStyleCorrectlyWhenCollisionToggled()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");

            // withCollision = true
            entity.meshesInfo.currentShape.HasCollisions().Returns(true);
            AddEntity(scene, entity);


            Assert.AreEqual(1, GetCollidersDisplayingCount());
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            // withCollision = false
            entity.meshesInfo.currentShape.HasCollisions().Returns(false);
            entity.OnMeshesInfoUpdated.Invoke(entity);

            Assert.AreEqual(0, GetCollidersDisplayingCount());
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator ApplyEntityStyleCorrectlyWhenMaterialChangeOnRuntime()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");

            AddEntity(scene, entity);
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            // change material for renderers
            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Material newMaterial = new Material(renderer.sharedMaterial) { name = "temptationMaterial" };
                renderer.sharedMaterial = newMaterial;
                Assert.AreNotEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            // wait a frame
            yield return null;

            foreach (var renderer in entity.meshesInfo.renderers)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, renderer.sharedMaterial.name);
            }

            controller.Dispose();
        }

        [Test]
        public void ApplyEntityStyleCorrectlyWhenShapeAdded()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithoutShape("temptationEntity");

            AddEntity(scene, entity);
            Assert.AreEqual(0, GetCollidersDisplayingCount());

            AddShapeToEntityWithColliders(entity, GameObject.CreatePrimitive(PrimitiveType.Cube));
            Assert.AreEqual(1, GetCollidersDisplayingCount());

            controller.Dispose();
        }

        [Test]
        public void RemoveEntityStyleCorrectlyWhenShapeRemoved()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");
            Material originalMaterial = entity.meshesInfo.renderers[0].sharedMaterial;

            AddEntity(scene, entity);

            entity.OnMeshesInfoCleaned.Invoke(entity);

            Assert.AreEqual(0, GetCollidersDisplayingCount());
            Assert.AreEqual(originalMaterial, entity.meshesInfo.renderers[0].sharedMaterial);

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveEntityStyleCorrectlyWhenShapeRemovedAndMaterialWasChanged()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");

            Material originalMaterial = entity.meshesInfo.renderers[0].sharedMaterial;
            Material newMaterial = new Material(originalMaterial);

            AddEntity(scene, entity);

            entity.meshesInfo.renderers[0].sharedMaterial = newMaterial;

            yield return null;

            entity.OnMeshesInfoCleaned.Invoke(entity);

            Assert.AreNotEqual(originalMaterial, entity.meshesInfo.renderers[0].sharedMaterial);
            Assert.AreEqual(newMaterial, entity.meshesInfo.renderers[0].sharedMaterial);

            controller.Dispose();
        }

        [Test]
        public void RemoveEntityStyleCorrectlyWhenEntityRemoved()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var entity = CreateEntityWithShapeAndCollider("temptationEntity");

            AddEntity(scene, entity);
            RemoveEntity(entity);

            Assert.AreEqual(0, GetCollidersDisplayingCount());

            controller.Dispose();
        }

        [Test]
        public void RemoveAllEntitiesStyleCorrectlyWhenFeatureIsDisabled()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var sceneEntities = new[]
            {
                CreateEntityWithShapeAndCollider("temptationEntity1"),
                CreateEntityWithShapeAndCollider("temptationEntity2"),
                CreateEntityWithShapeAndCollider("temptationEntity3"),
                CreateEntityWithShapeAndCollider("temptationEntity4"),
            };

            List<Material> originalMaterials = new List<Material>();
            foreach (var entity in sceneEntities)
            {
                originalMaterials.Add(entity.meshesInfo.renderers[0].sharedMaterial);
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetCollidersDisplayingCount());
            foreach (var entity in sceneEntities)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, entity.meshesInfo.renderers[0].sharedMaterial.name);
            }

            // Disable feature for scene
            isColliderDisplayEnabledVariable.AddOrSet("temptation", false);

            Assert.AreEqual(0, GetCollidersDisplayingCount());
            for (int i = 0; i < sceneEntities.Length; i++)
            {
                Assert.AreEqual(originalMaterials[i], sceneEntities[i].meshesInfo.renderers[0].sharedMaterial);
            }

            controller.Dispose();
        }

        [Test]
        public void RemoveAllEntitiesStyleCorrectlyWhenSceneIsDestroyed()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var sceneEntities = new[]
            {
                CreateEntityWithShapeAndCollider("temptationEntity1"),
                CreateEntityWithShapeAndCollider("temptationEntity2"),
                CreateEntityWithShapeAndCollider("temptationEntity3"),
                CreateEntityWithShapeAndCollider("temptationEntity4"),
            };

            List<Material> originalMaterials = new List<Material>();
            foreach (var entity in sceneEntities)
            {
                originalMaterials.Add(entity.meshesInfo.renderers[0].sharedMaterial);
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetCollidersDisplayingCount());
            foreach (var entity in sceneEntities)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, entity.meshesInfo.renderers[0].sharedMaterial.name);
            }

            // Remove scene
            RemoveScene(scene);

            Assert.AreEqual(0, GetCollidersDisplayingCount());
            for (int i = 0; i < sceneEntities.Length; i++)
            {
                Assert.AreEqual(originalMaterials[i], sceneEntities[i].meshesInfo.renderers[0].sharedMaterial);
            }

            controller.Dispose();
        }

        [Test]
        public void RemoveAllEntitiesStyleCorrectlyOnControllerDisposed()
        {
            var scene = CreateAndAddScene("temptation");
            isColliderDisplayEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesCollidersDisplayer(isColliderDisplayEnabledVariable, worldRuntime);
            var sceneEntities = new[]
            {
                CreateEntityWithShapeAndCollider("temptationEntity1"),
                CreateEntityWithShapeAndCollider("temptationEntity2"),
                CreateEntityWithShapeAndCollider("temptationEntity3"),
                CreateEntityWithShapeAndCollider("temptationEntity4"),
            };

            List<Material> originalMaterials = new List<Material>();
            foreach (var entity in sceneEntities)
            {
                originalMaterials.Add(entity.meshesInfo.renderers[0].sharedMaterial);
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetCollidersDisplayingCount());
            foreach (var entity in sceneEntities)
            {
                Assert.AreEqual(EntityStyle.ENTITY_MATERIAL_NAME, entity.meshesInfo.renderers[0].sharedMaterial.name);
            }

            // Dispose controller
            controller.Dispose();

            Assert.AreEqual(0, GetCollidersDisplayingCount());
            for (int i = 0; i < sceneEntities.Length; i++)
            {
                Assert.AreEqual(originalMaterials[i], sceneEntities[i].meshesInfo.renderers[0].sharedMaterial);
            }
        }

        private IParcelScene CreateAndAddScene(string id)
        {
            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { id = id });

            loadedScenes.Add(id, scene);
            entities[id] = new Dictionary<string, IDCLEntity>();

            scene.entities.Returns(entities[id]);

            sceneController.OnNewSceneAdded += Raise.Event<Action<IParcelScene>>(scene);

            return scene;
        }

        private void RemoveScene(IParcelScene scene)
        {
            var entitiesToRemove = entities[scene.sceneData.id].Values.ToArray();
            for (int i = 0; i < entitiesToRemove.Length; i++)
            {
                RemoveEntity(entitiesToRemove[i]);
            }
        }

        private IDCLEntity CreateEntityWithoutShape(string id)
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(id);

            var gameObject = new GameObject(id);
            entity.gameObject.Returns(gameObject);

            CreateMeshesInfoForEntity(entity);

            return entity;
        }

        private IDCLEntity CreateEntityWithShape(string id)
        {
            IDCLEntity entity = CreateEntityWithoutShape(id);

            AddShapeToEntity(entity, GameObject.CreatePrimitive(PrimitiveType.Cube));

            return entity;
        }

        private IDCLEntity CreateEntityWithShapeAndCollider(string id)
        {
            IDCLEntity entity = CreateEntityWithShape(id);
            AddCollidersToEntity(entity);
            return entity;
        }

        private void CreateMeshesInfoForEntity(IDCLEntity entity)
        {
            var meshesInfo = new MeshesInfo();
            meshesInfo.OnUpdated += () => entity.OnMeshesInfoUpdated.Invoke(entity);
            meshesInfo.OnCleanup += () => entity.OnMeshesInfoCleaned.Invoke(entity);
            entity.meshesInfo.Returns(meshesInfo);
        }

        private void AddShapeToEntity(IDCLEntity entity, GameObject shape)
        {
            entity.meshesInfo.currentShape = Substitute.For<IShape>();
            entity.meshesInfo.meshRootGameObject = shape;
        }

        private void AddShapeToEntityWithColliders(IDCLEntity entity, GameObject shape)
        {
            MeshesInfo meshesInfo = entity.meshesInfo;
            meshesInfo.currentShape = Substitute.For<IShape>();
            meshesInfo.currentShape.HasCollisions().Returns(true);
            meshesInfo.colliders.Add(shape.AddComponent<BoxCollider>());
            meshesInfo.meshRootGameObject = shape;
        }

        private void AddCollidersToEntity(IDCLEntity entity)
        {
            MeshesInfo meshesInfo = entity.meshesInfo;
            meshesInfo.colliders.Add(meshesInfo.meshRootGameObject.AddComponent<BoxCollider>());
            meshesInfo.currentShape.HasCollisions().Returns(true);
        }

        private void AddEntity(IParcelScene scene, IDCLEntity entity)
        {
            entities[scene.sceneData.id].Add(entity.entityId, entity);
            entity.scene.Returns(scene);
            scene.OnEntityAdded += Raise.Event<Action<IDCLEntity>>(entity);
        }

        private void RemoveEntity(IDCLEntity entity)
        {
            if (entities[entity.scene.sceneData.id].Remove(entity.entityId))
            {
                Object.Destroy(entity.gameObject);
                entity.scene.OnEntityRemoved += Raise.Event<Action<IDCLEntity>>(entity);
            }
        }

        private int GetCollidersDisplayingCount(bool includeInactive = false)
        {
            return Object
                   .FindObjectsOfType<GameObject>(includeInactive)
                   .Count(go => go.name.StartsWith(EntityStyle.COLLIDERS_GAMEOBJECT_NAME));
        }
    }
}