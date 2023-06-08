using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using NSubstitute;
using NSubstitute.Extensions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UUIDComponentTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private Camera mainCamera;
        private UUIDEventsPlugin uuidEventsPlugin;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateEnvironment());
            result.Add(MainSceneFactory.CreateEventSystem());
            return result;
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            ServiceLocator result = DCL.ServiceLocatorTestFactory.CreateMocked();
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
            result.Register<IWorldState>(() => new WorldState());
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
            result.Register<IWebRequestController>(WebRequestController.Create);
            return result;
        }


        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            Utils.LockCursor();
            scene = TestUtils.CreateTestScene();

            Physics.autoSyncTransforms = true;

            mainCamera = TestUtils.CreateComponentWithGameObject<Camera>("Main Camera");
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = Vector3.zero;
            mainCamera.transform.forward = Vector3.forward;

            EntityIdHelper idHelper = new EntityIdHelper();
            DCL.Environment.i.world.sceneController.Configure().entityIdHelper.Returns(idHelper);

            uuidEventsPlugin = new UUIDEventsPlugin();
            coreComponentsPlugin = new CoreComponentsPlugin();
        }

        protected override IEnumerator TearDown()
        {
            uuidEventsPlugin.Dispose();
            coreComponentsPlugin.Dispose();

            Object.Destroy(mainCamera.gameObject);
            Utils.UnlockCursor();
            yield return base.TearDown();
        }

        void InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape)
        {
            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var componentGO = component.gameObject;

            var meshFilter = component.entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverExitComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverExit.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene,
                entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShape()
        {
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var componentGO = component.gameObject;

            foreach (var meshFilter in componentGO.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithGLTFShape()
        {
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithGLTFShape()
        {
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene,
                scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterInitializesWithGLTFShape()
        {
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = clickUuid
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerHoverExitInitializesWithGLTFShape()
        {
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverExit.NAME,
                uuid = clickUuid
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShapeAsynchronously()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var uuidComponent = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithGLTFShapeAsynchronously()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };
            TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithGLTFShapeAsynchronously()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerUpModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene,
                scene.entities[entityId],
                OnPointerUpModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterInitializesWithGLTFShapeAsynchronously()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = clickUuid
            };
            TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerHoverExitInitializesWithGLTFShapeAsynchronously()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverExit.NAME,
                uuid = clickUuid
            };
            TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesAfterBasicShapeIsAdded()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null, "component is null?");

            yield return component.routine;

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(component.gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            yield return null;

            var meshFilter = component.entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesAfterBasicShapeIsAdded()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };

            var uuidComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(uuidComponent.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            yield return null;

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesAfterBasicShapeIsAdded()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene,
                scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            yield return null;

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterComponentInitializesAfterBasicShapeIsAdded()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            yield return null;

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverExitComponentInitializesAfterBasicShapeIsAdded()
        {
            long entityId = 1;
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverExit.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene,
                scene.entities[entityId],
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            yield return null;

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickEventIsTriggered()
        {
            InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnPointerEvent.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnPointerEvent.Model>(
                scene,
                entity,
                OnClickComponentModel,
                CLASS_ID_COMPONENT.UUID_CALLBACK
            );

            Assert.IsTrue(component != null);

            yield return null;

            string targetEventType = "SceneEvent";

            var onPointerEvent = new WebInterface.OnClickEvent();
            onPointerEvent.uuid = onPointerId;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnClickEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            yield return TestUtils.ExpectMessageToKernel(
                targetEventType,
                sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (eventObj) =>
                {
                    if (eventTriggered)
                        return true;

                    if (eventObj.eventType != sceneEvent.eventType)
                        return false;

                    if (eventObj.payload.uuid != sceneEvent.payload.uuid)
                        return false;

                    eventTriggered = true;
                    return true;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventIsTriggered()
        {
            InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerEvent.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = DCL.Environment.i.world.sceneController.entityIdHelper.GetOriginalId(component.entity.entityId);
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered)
                        return true;

                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerUpEventIsTriggered()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = OnPointerUp.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerUpEvent = new WebInterface.OnPointerUpEvent();
            onPointerUpEvent.uuid = onPointerId;
            onPointerUpEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerUpEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerUpEvent.payload.hit.entityId = DCL.Environment.i.world.sceneController.entityIdHelper.GetOriginalId(component.entity.entityId);
            onPointerUpEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerUpEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_UP, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered)
                        return true;

                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEventIsTriggered()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.up;

            yield return shape.routine;

            const string uuidHoverEnter = "pointerHoverEnterEvent-1";
            var hoverEnterModel = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = uuidHoverEnter
            };

            const string uuidHoverExit = "pointerHoverExitEvent-1";
            var hoverExitModel = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverExit.NAME,
                uuid = uuidHoverExit
            };

            var onPointerHoverEnterComponent =
                TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene, entity,
                    hoverEnterModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var onPointerHoverExitComponent =
                TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene, entity,
                    hoverExitModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(onPointerHoverEnterComponent != null);
            Assert.IsTrue(onPointerHoverExitComponent != null);

            string targetEventType = "SceneEvent";

            var onPointerHoverEnterEvent = new WebInterface.UUIDEvent<WebInterface.EmptyPayload>
                {uuid = uuidHoverEnter};
            var onPointerHoverExitEvent = new WebInterface.UUIDEvent<WebInterface.EmptyPayload> {uuid = uuidHoverExit};

            var sceneEventHoverEnter = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.EmptyPayload>>
            {
                sceneNumber = scene.sceneData.sceneNumber,
                payload = onPointerHoverEnterEvent,
                eventType = "uuidEvent"
            };

            var sceneEventHoverExit = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.EmptyPayload>>
            {
                sceneNumber = scene.sceneData.sceneNumber,
                payload = onPointerHoverExitEvent,
                eventType = "uuidEvent"
            };

            bool hoverEnterEventTriggered = false;
            bool hoverExitEventTriggered = false;

            mainCamera.transform.forward = Vector3.right;

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEventHoverEnter,
                () => { mainCamera.transform.forward = Vector3.right; },
                (pointerEvent) =>
                {
                    if (hoverEnterEventTriggered)
                        return true;

                    if (pointerEvent.eventType == sceneEventHoverEnter.eventType &&
                        pointerEvent.payload.uuid == sceneEventHoverEnter.payload.uuid)
                    {
                        hoverEnterEventTriggered = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(hoverEnterEventTriggered);

            mainCamera.transform.forward = Vector3.up;

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEventHoverExit,
                () => { mainCamera.transform.forward = Vector3.up; },
                (pointerEvent) =>
                {
                    if (hoverExitEventTriggered)
                        return true;

                    if (pointerEvent.eventType == sceneEventHoverExit.eventType &&
                        pointerEvent.payload.uuid == sceneEventHoverExit.payload.uuid)
                    {
                        hoverExitEventTriggered = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(hoverExitEventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerUpEventNotTriggeredOnInvisibles()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = OnPointerUp.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerUpEvent = new WebInterface.OnPointerUpEvent();
            onPointerUpEvent.uuid = onPointerId;
            onPointerUpEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerUpEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerUpEvent.payload.hit.entityId = DCL.Environment.i.world.sceneController.entityIdHelper.GetOriginalId(component.entity.entityId);
            onPointerUpEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerUpEvent;
            sceneEvent.eventType = "uuidEvent";

            bool eventTriggered1 = false;
            uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_UP, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered1)
                        return true;

                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered1 = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered1);

            // turn shape invisible
            TestUtils.UpdateShape(scene, shape.id, JsonConvert.SerializeObject(new {visible = false}));
            uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            var pointerUpReceived = false;

            void MsgFromEngineCallback(string eventType, string eventPayload)
            {
                if (string.IsNullOrEmpty(eventPayload) || eventType != targetEventType)
                    return;

                var pointerEvent =
                    JsonUtility.FromJson<WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>>(eventPayload);
                if (pointerEvent.eventType == sceneEvent.eventType
                    && pointerEvent.payload.uuid == sceneEvent.payload.uuid
                    && pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                {
                    pointerUpReceived = true;
                }
            }

            // Hook up to web interface engine message reporting
            WebInterface.OnMessageFromEngine += MsgFromEngineCallback;
            uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                InputController_Legacy.EVENT.BUTTON_UP, true, true);
            WebInterface.OnMessageFromEngine -= MsgFromEngineCallback;

            Assert.IsFalse(pointerUpReceived);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterEventNotTriggeredOnInvisibles()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity,
                new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.up;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerHoverEnterEvent = new WebInterface.UUIDEvent<WebInterface.EmptyPayload> {uuid = onPointerId};

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.EmptyPayload>>
            {
                sceneNumber = scene.sceneData.sceneNumber,
                payload = onPointerHoverEnterEvent,
                eventType = "uuidEvent"
            };

            bool eventTriggered1 = false;
            mainCamera.transform.forward = Vector3.right;

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () => { mainCamera.transform.forward = Vector3.right; },
                (pointerEvent) =>
                {
                    if (eventTriggered1)
                        return true;

                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid)
                    {
                        eventTriggered1 = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered1);

            // turn shape invisible
            TestUtils.UpdateShape(scene, shape.id, JsonConvert.SerializeObject(new {visible = false}));
            mainCamera.transform.forward = Vector3.up;

            var pointerHoverReceived = false;

            void MsgFromEngineCallback(string eventType, string eventPayload)
            {
                if (string.IsNullOrEmpty(eventPayload) || eventType != targetEventType)
                    return;

                var pointerEvent =
                    JsonUtility.FromJson<WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>>(eventPayload);
                if (pointerEvent.eventType == sceneEvent.eventType
                    && pointerEvent.payload.uuid == sceneEvent.payload.uuid)
                {
                    pointerHoverReceived = true;
                }
            }

            // Hook up to web interface engine message reporting
            WebInterface.OnMessageFromEngine += MsgFromEngineCallback;
            mainCamera.transform.forward = Vector3.right;
            WebInterface.OnMessageFromEngine -= MsgFromEngineCallback;

            Assert.IsFalse(pointerHoverReceived);
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventWhenEntityIsBehindOther()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(3, 3, 5), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;


            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // We simulate that entityId has come from kernel
            DCL.Environment.i.world.sceneController.entityIdHelper.entityIdToLegacyId.Add(component.entity.entityId,component.entity.entityId.ToString());

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = DCL.Environment.i.world.sceneController.entityIdHelper.GetOriginalId(component.entity.entityId);
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";
            EntityIdHelper idHelper = new EntityIdHelper();


            // Check if target entity is hit behind other entity
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            Assert.IsTrue(!targetEntityHit, "Target entity was hit but other entity was blocking it");


            // Move character in front of target entity and rotate camera
            mainCamera.transform.position = new Vector3(3, 3, 6);
            mainCamera.transform.forward = Vector3.back;

            yield return null;

            // Check if target entity is hit in front of the camera without being blocked
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }
                    return true;
                });

            yield return null;
            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterEventWhenEntityIsBehindOther()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for hover
            IDCLEntity hoverTargetEntity;
            BoxShape hoverTargetShape;
            InstantiateEntityWithShape(out hoverTargetEntity, out hoverTargetShape);
            TestUtils.SetEntityTransform(scene, hoverTargetEntity, new Vector3(3, 3, 5), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return hoverTargetShape.routine;

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);
            mainCamera.transform.forward = Vector3.up;

            // Create pointer hover component and add it to target entity
            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                hoverTargetEntity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerHoverEnterEvent = new WebInterface.UUIDEvent<WebInterface.EmptyPayload> {uuid = onPointerId};

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.EmptyPayload>>
            {
                sceneNumber = scene.sceneData.sceneNumber,
                payload = onPointerHoverEnterEvent,
                eventType = "uuidEvent"
            };

            // Check if target entity is hit behind other entity
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () => { mainCamera.transform.forward = Vector3.forward; },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId)
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            Assert.IsTrue(!targetEntityHit, "Target entity was hit but other entity was blocking it");


            // Move character in front of target entity and rotate camera
            mainCamera.transform.position = new Vector3(3, 3, 6);
            mainCamera.transform.forward = Vector3.back;

            yield return null;

            // Check if target entity is hit in front of the camera without being blocked
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () => { mainCamera.transform.forward = Vector3.back; },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId)
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            yield return null;
            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventAndPointerBlockerShape()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity,
                new Vector3(1, 1, 1));

            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(3, 3, 5), Quaternion.identity,
                new Vector3(1, 1, 1));

            yield return clickTargetShape.routine;

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return component.routine;

            Assert.IsTrue(component != null);
            Assert.IsTrue(clickTargetEntity != null);
            Assert.IsTrue(component.entity != null);

            string targetEventType = "SceneEvent";
            // We simulate that entityId has come from kernel
            DCL.Environment.i.world.sceneController.entityIdHelper.entityIdToLegacyId.Add(component.entity.entityId,component.entity.entityId.ToString());

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = DCL.Environment.i.world.sceneController.entityIdHelper.GetOriginalId(component.entity.entityId);
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";
            EntityIdHelper idHelper = new EntityIdHelper();
            // Check the target entity is not hit behind the 'isPointerBlocker' shape
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            Assert.IsFalse(targetEntityHit, "Target entity was hit but other entity was blocking it");

            // Toggle 'isPointerBlocker' property
            yield return TestUtils.SharedComponentUpdate(blockingShape, new BoxShape.Model
            {
                isPointerBlocker = false
            });

            // Check the target entity is hit behind the 'isPointerBlocker' shape now
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            yield return null;

            yield return new WaitForSeconds(5.0f);

            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverEnterEventAndPointerBlockerShape()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity,
                new Vector3(1, 1, 1));

            yield return blockingShape.routine;

            // Create target entity for hover
            InstantiateEntityWithShape(out IDCLEntity hoverTargetEntity, out BoxShape hoverTargetShape);
            TestUtils.SetEntityTransform(scene, hoverTargetEntity, new Vector3(3, 3, 5), Quaternion.identity,
                new Vector3(1, 1, 1));

            yield return hoverTargetShape.routine;

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);
            mainCamera.transform.forward = Vector3.up;

            yield return null;

            // Create pointer hover component and add it to target entity
            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene,
                hoverTargetEntity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return component.routine;

            Assert.IsTrue(component != null);
            Assert.IsTrue(hoverTargetEntity != null);
            Assert.IsTrue(component.entity != null);

            string targetEventType = "SceneEvent";

            var onPointerHoverEnterEvent = new WebInterface.UUIDEvent<WebInterface.EmptyPayload> {uuid = onPointerId};

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.UUIDEvent<WebInterface.EmptyPayload>>
                {sceneNumber = scene.sceneData.sceneNumber, payload = onPointerHoverEnterEvent, eventType = "uuidEvent"};

            // Check the target entity is not hit behind the 'isPointerBlocker' shape
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () => { mainCamera.transform.forward = Vector3.forward; },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId)
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            Assert.IsFalse(targetEntityHit, "Target entity was hit but other entity was blocking it");

            mainCamera.transform.forward = Vector3.up;

            // Toggle 'isPointerBlocker' property
            yield return TestUtils.SharedComponentUpdate(blockingShape, new BoxShape.Model
            {
                isPointerBlocker = false
            });

            // Check the target entity is hit behind the 'isPointerBlocker' shape now
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () => { mainCamera.transform.forward = Vector3.forward; },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId)
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            yield return null;

            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator PointerEventNotTriggeredByParent()
        {
            EntityIdHelper idHelper = new EntityIdHelper();
            DCL.Environment.i.world.sceneController.Configure().entityIdHelper.Returns(idHelper);

            // Create parent entity
            InstantiateEntityWithShape(out IDCLEntity blockingEntity, out BoxShape blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(0, 0, 5), Quaternion.identity,
                new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Enparent target entity as a child of the blocking entity
            TestUtils.SetEntityParent(scene, clickTargetEntity, blockingEntity);

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 2, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            // We simulate that entityId has come from kernel
            DCL.Environment.i.world.sceneController.entityIdHelper.entityIdToLegacyId.Add(component.entity.entityId,component.entity.entityId.ToString());


            string targetEventType = "SceneEvent";

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = component.entity.entityId.ToString();
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneNumber = scene.sceneData.sceneNumber;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";

            // Check if target entity is triggered by hitting the parent entity
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            Assert.IsFalse(targetEntityHit, "Target entity was hit but other entity was blocking it");

            // Move character in front of target entity and rotate camera
            mainCamera.transform.position = new Vector3(3, 3, 12);
            mainCamera.transform.forward = Vector3.back;

            yield return null;

            // Check if target entity is triggered when hit directly
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    uuidEventsPlugin.inputControllerLegacy.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == idHelper.GetOriginalId(clickTargetEntity.entityId))
                    {
                        targetEntityHit = true;
                    }

                    return true;
                });

            yield return null;
            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerEventsPropertiesAreAppliedCorrectly()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity,
                new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return component.routine;

            Assert.IsTrue(component != null);

            mainCamera.transform.position = new Vector3(8, 1, 7);
            yield return null;

            Assert.IsNotNull(uuidEventsPlugin.hoverCanvas);
            Assert.IsTrue(uuidEventsPlugin.hoverCanvas.canvas.enabled);

            // Check default properties
            Assert.IsNotNull(uuidEventsPlugin.hoverCanvas.GetCurrentHoverIcon(),
                "OnPointerEvent.SetFeedbackState never called!");
            Assert.AreEqual("AnyButtonHoverIcon", uuidEventsPlugin.hoverCanvas.GetCurrentHoverIcon().name);
            Assert.AreEqual("Interact", uuidEventsPlugin.hoverCanvas.text.text);

            yield return null;

            onPointerDownModel.button = "PRIMARY";
            onPointerDownModel.hoverText = "Click!";

            // we can't use TestHelpers.EntityComponentUpdate() to update UUIDComponents until we separate every UUIComponent to their own new CLASS_ID_COMPONENT
            component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return null;

            Assert.AreEqual("PrimaryButtonHoverIcon", uuidEventsPlugin.hoverCanvas.GetCurrentHoverIcon().name);
            Assert.AreEqual("Click!", uuidEventsPlugin.hoverCanvas.text.text);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverDistanceIsAppliedCorrectly()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity,
                new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);
            Assert.IsTrue(component != null);

            yield return null;

            // Canvas should be initialized as false
            Assert.IsFalse(uuidEventsPlugin.hoverCanvas.canvas.enabled);
            mainCamera.transform.position = new Vector3(8, 2, 7);

            yield return null;

            // Canvas now should be true because the camera was repositioned
            Assert.IsTrue(uuidEventsPlugin.hoverCanvas.canvas.enabled);

            onPointerDownModel.distance = 1f;
            // we can't use TestHelpers.EntityComponentUpdate() to update UUIDComponents until we separate every UUIComponent to their own new CLASS_ID_COMPONENT
            component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return null;

            // Canvas should be false again because the distance value was set to 1
            Assert.IsFalse(uuidEventsPlugin.hoverCanvas.canvas.enabled);
            Object.Destroy(component);
        }
    }
}
