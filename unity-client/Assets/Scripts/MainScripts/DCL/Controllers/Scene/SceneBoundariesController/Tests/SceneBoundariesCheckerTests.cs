using DCL;
using DCL.Components;
using Newtonsoft.Json;
using DCL.Models;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using NUnit.Framework;
using System.Collections;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests : TestsBase
    {
        [UnityTest]
        [Explicit]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            yield return Assert_PShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_PShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        IEnumerator Assert_PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));
            yield return null;

            Assert.IsTrue(MeshIsInvalid(boxShape.attachedEntities.First().meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            yield return Assert_GLTFShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, true, false);

            yield return Assert_GLTFShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        IEnumerator Assert_GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return InitScene();

            yield return Assert_NFTShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_NFTShapeIsInvalidatedWhenStartingOutOfBounds();
        }

        IEnumerator Assert_NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            yield return Assert_PShapeIsInvalidatedWhenLeavingBounds();
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_PShapeIsInvalidatedWhenLeavingBounds();
        }

        IEnumerator Assert_PShapeIsInvalidatedWhenLeavingBounds()
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(8, 1, 8));
            yield return null;

            var entity = boxShape.attachedEntities.First();

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(18, 1, 18) };
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            yield return Assert_GLTFShapeIsInvalidatedWhenLeavingBounds();
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            yield return InitScene(debugMode: true, reloadUnityScene: false);

            yield return Assert_GLTFShapeIsInvalidatedWhenLeavingBounds();
        }

        IEnumerator Assert_GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return InitScene();

            yield return Assert_NFTShapeIsInvalidatedWhenLeavingBounds();
        }

        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_NFTShapeIsInvalidatedWhenLeavingBounds();
        }

        IEnumerator Assert_NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator PShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            yield return Assert_PShapeIsResetWhenReenteringBounds();
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_PShapeIsResetWhenReenteringBounds();
        }

        IEnumerator Assert_PShapeIsResetWhenReenteringBounds()
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(18, 1, 18));
            yield return null;

            var entity = boxShape.attachedEntities.First();
            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to re-enter the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 1, 8) };
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            yield return Assert_GLTFShapeIsResetWhenReenteringBounds();
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_GLTFShapeIsResetWhenReenteringBounds();
        }

        IEnumerator Assert_GLTFShapeIsResetWhenReenteringBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper_GLTF gltfShape = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds()
        {
            yield return InitScene();

            yield return Assert_NFTShapeIsResetWhenReenteringBounds();
        }

        [UnityTest]
        public IEnumerator NFTShapeIsResetWhenReenteringBoundsDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_NFTShapeIsResetWhenReenteringBounds();
        }

        IEnumerator Assert_NFTShapeIsResetWhenReenteringBounds()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        [UnityTest]
        [Explicit]
        public IEnumerator ChildShapeIsEvaluated()
        {
            yield return InitScene();

            yield return Assert_ChildShapeIsEvaluated();
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_ChildShapeIsEvaluated();
        }

        IEnumerator Assert_ChildShapeIsEvaluated()
        {
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
        [Explicit]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent()
        {
            yield return InitScene();

            yield return Assert_ChildShapeIsEvaluatedOnShapelessParent();
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParentDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_ChildShapeIsEvaluatedOnShapelessParent();
        }

        IEnumerator Assert_ChildShapeIsEvaluatedOnShapelessParent()
        {
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
        [Explicit]
        public IEnumerator HeightIsEvaluated()
        {
            yield return InitScene();

            yield return Assert_HeightIsEvaluated();
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return Assert_HeightIsEvaluated();
        }

        IEnumerator Assert_HeightIsEvaluated()
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(8, 5, 8));
            var entity = boxShape.attachedEntities.First();
            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene height boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 30, 8) };
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        bool MeshIsInvalid(DecentralandEntity.MeshesInfo meshesInfo)
        {
            if (meshesInfo.meshRootGameObject == null) return false; // It's not invalid if there's no mesh

            if (SceneController.i.isDebugMode)
            {
                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    if (meshesInfo.renderers[i].sharedMaterial.name != "InvalidSubMesh") return false;
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
