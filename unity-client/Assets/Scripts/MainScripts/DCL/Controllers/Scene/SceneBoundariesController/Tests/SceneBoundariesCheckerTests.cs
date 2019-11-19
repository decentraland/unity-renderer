using DCL;
using DCL.Components;
using Newtonsoft.Json;
using DCL.Models;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Tests
{
    public class SceneBoundariesCheckerTests : TestsBase
    {
        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
            yield return null;

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, true);

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);
            yield return null;

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, scene.entities[entityId]);

            var shapeLoader = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(18, 1, 18) };
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, scene.entities[entityId]);

            var shapeLoader = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(18, 1, 18));
            yield return null;

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to re-enter the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 1, 8) };
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, scene.entities[entityId]);

            var shapeLoader = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluated()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Attach child
            string childEntityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, childEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));

            TestHelpers.SetEntityParent(scene, childEntityId, entityId);

            // Move parent object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(18, 1, 18) };
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            Assert.IsTrue(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent()
        {
            yield return InitScene();

            // create shapeless parent entity
            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new Vector3(18, 1, 18), Quaternion.identity, Vector3.one);
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo), "Entity mesh shouldn't be invalid as it has no mesh");

            // Attach child
            string childEntityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, childEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(0, 0, 0));
            yield return null;

            TestHelpers.SetEntityParent(scene, childEntityId, entityId);

            Assert.IsTrue(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));

            // Move parent object to re-enter the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new Vector3(8, 1, 8), Quaternion.identity, Vector3.one);

            Assert.IsFalse(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluated()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 5, 8));
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[entityId].meshesInfo));

            // Move object to surpass the scene height boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 30, 8) };
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            Assert.IsTrue(MeshIsInvalid(scene.entities[entityId].meshesInfo));
        }

        [UnityTest]
        public IEnumerator SubmeshesCheckAllowsIrregularShapes()
        {
            // Set up an irregular-shaped ("L" shape) scene
            yield return InitScene(false, spawnCharController: true, spawnTestScene: false, true, debugMode: true);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.basePosition = new Vector2Int(0, 0);
            sceneData.parcels = new Vector2Int[] { sceneData.basePosition, new Vector2Int(1, 0), new Vector2Int(1, -1) };

            scene = sceneController.CreateTestScene(sceneData);
            yield return new WaitForSeconds(0.01f);

            // Load an irregular-shaped ("L" shape) mesh
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/IrregularShape/irregular.glb"
                }));
            yield return null;

            LoadWrapper_GLTF gltfShape = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            // position and rotate mesh
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(16, 0, 0), rotation = Quaternion.Euler(0, -90, 0) });
            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        bool MeshIsInvalid(DecentralandEntity.MeshesInfo meshesInfo)
        {
            if (meshesInfo.meshRootGameObject == null) return false; // It's not invalid if there's no mesh

            if (SceneController.i.isDebugMode)
            {
                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    if (meshesInfo.renderers[i].sharedMaterial.name != "InvalidMesh") return false;
                }
            }
            else
            {
                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    if (meshesInfo.renderers[i].enabled) return false;
                }

                for (int i = 0; i < meshesInfo.colliders.Count; i++)
                {
                    if (meshesInfo.colliders[i].enabled) return false;
                }
            }

            return true;
        }
    }
}