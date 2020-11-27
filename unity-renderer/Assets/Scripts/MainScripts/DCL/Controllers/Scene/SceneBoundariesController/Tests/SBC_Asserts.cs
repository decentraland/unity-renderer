using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SceneBoundariesCheckerTests
{
    public static class SBC_Asserts
    {
        public static IEnumerator EntitiesAreBeingCorrectlyRegistered(ParcelScene scene)
        {
            var boxShape1 = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));
            var boxShape2 = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));

            var entity1 = boxShape1.attachedEntities.First();
            var entity2 = boxShape2.attachedEntities.First();

            TestHelpers.SetEntityParent(scene, entity1, entity2);

            Assert.AreEqual(2, scene.entities.Count, "scene entities count can't be zero!");
            Assert.AreEqual(2, SceneController.i.boundariesChecker.entitiesToCheckCount, "entities to check can't be zero!");

            yield return null;

            TestHelpers.RemoveSceneEntity(scene, entity2.entityId);

            Environment.i.parcelScenesCleaner.ForceCleanup();

            Assert.AreEqual(0, scene.entities.Count, "entity count should be zero");
            Assert.AreEqual(0, SceneController.i.boundariesChecker.entitiesToCheckCount, "entities to check should be zero!");

            yield break;
        }

        public static IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));
            yield return null;

            Assert.IsTrue(MeshIsInvalid(boxShape.attachedEntities.First().meshesInfo));
        }

        public static IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            yield return null;

            LoadWrapper shapeLoader = NFTShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator PShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(8, 1, 8));
            yield return null;

            var entity = boxShape.attachedEntities.First();

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model {position = new Vector3(18, 1, 18)};
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            yield return null;
            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            yield return null;
            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            yield return null;

            LoadWrapper shapeLoader = NFTShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator HeightIsEvaluated(ParcelScene scene)
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(8, 5, 8));
            var entity = boxShape.attachedEntities.First();
            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene height boundaries
            var transformModel = new DCLTransform.Model {position = new Vector3(8, 30, 8)};
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            yield return null;
            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator ChildShapeIsEvaluated(ParcelScene scene)
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
            var transformModel = new DCLTransform.Model {position = new Vector3(18, 1, 18)};
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            yield return null;
            yield return null;

            Assert.IsTrue(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));
        }

        public static IEnumerator ChildShapeIsEvaluatedOnShapelessParent(ParcelScene scene)
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
            yield return null;

            Assert.IsTrue(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));

            // Move parent object to re-enter the scene boundaries
            TestHelpers.SetEntityTransform(scene, scene.entities[entityId], new Vector3(8, 1, 8), Quaternion.identity, Vector3.one);

            yield return null;
            yield return null;

            Assert.IsFalse(MeshIsInvalid(scene.entities[childEntityId].meshesInfo));
        }

        public static IEnumerator PShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(18, 1, 18));
            yield return null;

            var entity = boxShape.attachedEntities.First();
            yield return null;

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to re-enter the scene boundaries
            var transformModel = new DCLTransform.Model {position = new Vector3(8, 1, 8)};
            TestHelpers.SetEntityTransform(scene, entity, transformModel);

            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator GLTFShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            yield return null;
            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        public static IEnumerator NFTShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };
            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity);

            yield return null;

            LoadWrapper shapeLoader = NFTShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            Assert.IsTrue(MeshIsInvalid(entity.meshesInfo));

            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            yield return null;

            Assert.IsFalse(MeshIsInvalid(entity.meshesInfo));
        }

        public static bool MeshIsInvalid(DecentralandEntity.MeshesInfo meshesInfo)
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