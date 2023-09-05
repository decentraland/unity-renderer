using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Helpers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class GltfContainerCollidersShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private GltfContainerHandler handler;

        private IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private IInternalECSComponent<InternalColliders> physicColliderComponent;
        private IInternalECSComponent<InternalColliders> customLayerColliderComponent;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            InternalECSComponents internalEcsComponents = new InternalECSComponents(componentsManager, componentFactory, executors);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(23423);

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("lantern", "Lantern/Lantern.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");

            pointerColliderComponent = internalEcsComponents.onPointerColliderComponent;
            physicColliderComponent = internalEcsComponents.physicColliderComponent;
            customLayerColliderComponent = internalEcsComponents.customLayerColliderComponent;

            handler = new GltfContainerHandler(
                pointerColliderComponent,
                physicColliderComponent,
                customLayerColliderComponent,
                internalEcsComponents.renderersComponent,
                internalEcsComponents.GltfContainerLoadingStateComponent,
                new DataStore_ECS7(),
                new DataStore_FeatureFlag());

            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            PoolManager.i.Dispose();
        }

        [Test]
        public void DefaultsCorrectly()
        {
            var model = new PBGltfContainer();
            Assert.AreEqual(0, model.GetVisibleMeshesCollisionMask());
            Assert.AreEqual((uint)(ColliderLayer.ClPhysics | ColliderLayer.ClPointer), model.GetInvisibleMeshesCollisionMask());
        }

        [UnityTest]
        [TestCase((uint)ColliderLayer.ClPhysics, false, ExpectedResult = null)]
        [TestCase((uint)ColliderLayer.ClPointer, false, ExpectedResult = null)]
        [TestCase((uint)ColliderLayer.ClCustom1, false, ExpectedResult = null)]
        [TestCase((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics), false, ExpectedResult = null)]
        [TestCase((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1), false, ExpectedResult = null)]
        [TestCase((uint)ColliderLayer.ClPhysics, true, ExpectedResult = null)]
        [TestCase((uint)ColliderLayer.ClPointer, true, ExpectedResult = null)]
        [TestCase((uint)ColliderLayer.ClCustom1, true, ExpectedResult = null)]
        [TestCase((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics), true, ExpectedResult = null)]
        [TestCase((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1), true, ExpectedResult = null)]
        public IEnumerator CreateWithColliders(uint mask, bool visibleColliders)
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = visibleColliders ? 0 : mask,
                VisibleMeshesCollisionMask = visibleColliders ? mask : 0
            });

            yield return handler.gltfLoader.Promise;

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true)
                                   .Where(c => c.enabled)
                                   .ToArray();

            int? unityGameObjectLayer = LayerMaskUtils.SdkLayerMaskToUnityLayer(mask);
            bool hasCustomLayer = LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(mask);

            Assert.IsTrue(unityGameObjectLayer.HasValue);
            Assert.IsTrue(colliders.Length > 0);

            foreach (var collider in colliders)
            {
                bool containsColliderName = HasColliderName(collider);
                Assert.IsTrue((!visibleColliders && containsColliderName) || (visibleColliders && !containsColliderName));
                Assert.AreEqual(unityGameObjectLayer.Value, collider.gameObject.layer);

                if ((mask & (int)ColliderLayer.ClPhysics) != 0) { Assert.IsTrue(physicColliderComponent.GetFor(scene, entity).Value.model.colliders.ContainsKey(collider)); }

                if ((mask & (int)ColliderLayer.ClPointer) != 0) { Assert.IsTrue(pointerColliderComponent.GetFor(scene, entity).Value.model.colliders.ContainsKey(collider)); }

                if (hasCustomLayer) { Assert.IsTrue(customLayerColliderComponent.GetFor(scene, entity).Value.model.colliders.ContainsKey(collider)); }
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator DisableColliders()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1),
                VisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1)
            });

            yield return handler.gltfLoader.Promise;

            Assert.IsTrue(physicColliderComponent.GetFor(scene, entity).Value.model.colliders.Count > 0);
            Assert.IsTrue(pointerColliderComponent.GetFor(scene, entity).Value.model.colliders.Count > 0);
            Assert.IsTrue(customLayerColliderComponent.GetFor(scene, entity).Value.model.colliders.Count > 0);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = 0,
                VisibleMeshesCollisionMask = 0
            });

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true)
                                   .Where(c => c.enabled)
                                   .ToArray();

            Assert.AreEqual(0, colliders.Length);
            Assert.IsNull(physicColliderComponent.GetFor(scene, entity));
            Assert.IsNull(pointerColliderComponent.GetFor(scene, entity));
            Assert.IsNull(customLayerColliderComponent.GetFor(scene, entity));
        }

        [UnityTest]
        public IEnumerator UpdateCollidersMask()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = 0,
                VisibleMeshesCollisionMask = 0
            });

            yield return handler.gltfLoader.Promise;

            yield return CreateWithColliders((uint)ColliderLayer.ClPhysics, false);
            yield return CreateWithColliders((uint)ColliderLayer.ClPointer, false);
            yield return CreateWithColliders((uint)ColliderLayer.ClCustom1, false);
            yield return CreateWithColliders((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics), false);
            yield return CreateWithColliders((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1), false);
            yield return CreateWithColliders((uint)ColliderLayer.ClPhysics, true);
            yield return CreateWithColliders((uint)ColliderLayer.ClPointer, true);
            yield return CreateWithColliders((uint)ColliderLayer.ClCustom1, true);
            yield return CreateWithColliders((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics), true);
            yield return CreateWithColliders((uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1), true);
        }

        [UnityTest]
        public IEnumerator UndoCollidersModifications()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1),
                VisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1)
            });

            yield return handler.gltfLoader.Promise;

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true);

            var invisibleColliders = colliders.Where(HasColliderName).ToArray();
            var visibleColliders = colliders.Where(c => !HasColliderName(c)).ToArray();

            handler.collidersHandler.CleanUp();
            yield return null;

            foreach (var collider in invisibleColliders)
            {
                Assert.IsTrue(collider);
                Assert.IsFalse(collider.enabled);
            }

            foreach (var collider in visibleColliders)
            {
                // Any non "_collider" should have being removed
                Assert.IsFalse(collider);
            }
        }

        [UnityTest]
        public IEnumerator RemoveColliders()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1),
                VisibleMeshesCollisionMask = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1)
            });

            yield return handler.gltfLoader.Promise;

            handler.OnComponentRemoved(scene, entity);

            Assert.IsNull(physicColliderComponent.GetFor(scene, entity));
            Assert.IsNull(pointerColliderComponent.GetFor(scene, entity));
            Assert.IsNull(customLayerColliderComponent.GetFor(scene, entity));
        }

        [UnityTest]
        public IEnumerator UpdateCollidersOnGltfChanged()
        {
            const uint visibleColliders = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1);
            const uint invisibleColliders = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "palmtree",
                InvisibleMeshesCollisionMask = visibleColliders,
                VisibleMeshesCollisionMask = invisibleColliders
            });

            yield return handler.gltfLoader.Promise;

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer()
            {
                Src = "lantern",
                InvisibleMeshesCollisionMask = visibleColliders,
                VisibleMeshesCollisionMask = invisibleColliders
            });

            yield return handler.gltfLoader.Promise;

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true)
                                   .ToArray();

            Assert.IsTrue(colliders.Length > 0);

            foreach (var collider in colliders)
            {
                physicColliderComponent.GetFor(scene, entity).Value.model.colliders.Remove(collider);
                pointerColliderComponent.GetFor(scene, entity).Value.model.colliders.Remove(collider);
                customLayerColliderComponent.GetFor(scene, entity).Value.model.colliders.Remove(collider);
            }

            Assert.IsTrue(physicColliderComponent.GetFor(scene, entity).Value.model.colliders.Count == 0);
            Assert.IsTrue(pointerColliderComponent.GetFor(scene, entity).Value.model.colliders.Count == 0);
            Assert.IsTrue(customLayerColliderComponent.GetFor(scene, entity).Value.model.colliders.Count == 0);
        }

        [UnityTest]
        public IEnumerator CreateCollidersOnSkinnedMeshRenderers()
        {
            const uint visibleColliders = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1);
            const uint invisibleColliders = (uint)(ColliderLayer.ClPointer | ColliderLayer.ClPhysics | ColliderLayer.ClCustom1);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer
            {
                Src = "sharknado", // this specific model is 100% skinned mesh renderer
                InvisibleMeshesCollisionMask = visibleColliders,
                VisibleMeshesCollisionMask = invisibleColliders
            });

            yield return handler.gltfLoader.Promise;

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true).ToArray();

            Assert.IsTrue(colliders.Length > 0, "Ammount of colliders");
        }

        [UnityTest]
        public IEnumerator NotCreateVisibleCollidersAsDefault()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer
            {
                Src = "sharknado", // this specific model is 100% skinned mesh renderer
            });

            yield return handler.gltfLoader.Promise;

            var colliders = handler.gameObject.GetComponentsInChildren<Collider>(true).ToArray();

            Assert.IsTrue(colliders.Length == 0, "should not have create any collider");
        }

        private static bool HasColliderName(Collider collider)
        {
            const StringComparison IGNORE_CASE = StringComparison.OrdinalIgnoreCase;

            return collider.name.Contains("_collider", IGNORE_CASE)
                   || collider.transform.parent.name.Contains("_collider", IGNORE_CASE);
        }
    }
}
