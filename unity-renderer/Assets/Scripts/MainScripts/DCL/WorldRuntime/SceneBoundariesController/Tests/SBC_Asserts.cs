using System;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Linq;
using DCL;
using DCL.Helpers.NFT;
using NFTShape_Internal;
using NSubstitute;
using UnityEngine;
using UnityGLTF.Loader;
using Environment = DCL.Environment;

namespace SceneBoundariesCheckerTests
{
    public static class SBC_Asserts
    {
        public static IEnumerator EntitiesAreBeingCorrectlyRegistered(ParcelScene scene)
        {
            var boxShape1 = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));
            var boxShape2 = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));

            var entity1 = boxShape1.attachedEntities.First();
            var entity2 = boxShape2.attachedEntities.First();

            TestUtils.SetEntityParent(scene, entity1, entity2);

            Assert.AreEqual(2, scene.entities.Count, "scene entities count can't be zero!");
            Assert.AreEqual(2, Environment.i.world.sceneBoundsChecker.entitiesToCheckCount, "entities to check can't be zero!");

            yield return null;

            TestUtils.RemoveSceneEntity(scene, entity2.entityId);

            Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

            Assert.AreEqual(0, scene.entities.Count, "entity count should be zero");
            Assert.AreEqual(0, Environment.i.world.sceneBoundsChecker.entitiesToCheckCount, "entities to check should be zero!");
        }
        
        public static IEnumerator EntityIsEvaluatedOnReparenting(ParcelScene scene)
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(8, 2, 8));
            var shapeEntity = boxShape.attachedEntities.First();

            yield return null;
            AssertMeshIsValid(shapeEntity.meshesInfo);
         
            var newParentEntity = TestUtils.CreateSceneEntity(scene);
            TestUtils.SetEntityTransform(scene, newParentEntity, new DCLTransform.Model { position = new Vector3(100, 1, 100) });
            
            // Our entities parenting moves the child's local position to Vector3.zero by default...
            TestUtils.SetEntityParent(scene, shapeEntity, newParentEntity);
            
            yield return null;
            AssertMeshIsInvalid(shapeEntity.meshesInfo);
        }

        public static IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(20, 2, 20));
            yield return null;

            AssertMeshIsInvalid(boxShape.attachedEntities.First().meshesInfo);
        }

        public static IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator GLTFShapeWithCollidersAndNoRenderersIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/gltfshape-asset-bundle-colliders-no-renderers.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }
        
        public static IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );
            
            yield return null;
            AssertMeshIsInvalid(entity.meshesInfo);
        }
        
        public static IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            
            yield return null;
            AssertMeshIsInvalid(entity.meshesInfo);
        }
        
        public static IEnumerator PShapeIsEvaluatedAfterCorrectTransformAttachment(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );
            
            yield return null;
            AssertMeshIsInvalid(entity.meshesInfo);
            
            yield return null;
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });
            
            yield return null;
            AssertMeshIsValid(entity.meshesInfo);
        }
        
        public static IEnumerator GLTFShapeIsEvaluatedAfterCorrectTransformAttachment(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            
            yield return null;
            AssertMeshIsInvalid(entity.meshesInfo);
            
            yield return null;
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });
            
            yield return null;
            AssertMeshIsValid(entity.meshesInfo);
        }
        
        public static IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestUtils.SharedComponentAttach(component, entity);

            LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator PShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(8, 1, 8));

            yield return null;
            yield return null;
            var entity = boxShape.attachedEntities.First();

            AssertMeshIsValid(entity.meshesInfo);

            // Move object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(18, 1, 18) };
            TestUtils.SetEntityTransform(scene, entity, transformModel);

            yield return null;
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);

            AssertMeshIsValid(entity.meshesInfo);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            yield return null;
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestUtils.SharedComponentAttach(component, entity);

            LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => shapeLoader.alreadyLoaded);

            AssertMeshIsValid(entity.meshesInfo);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator HeightIsEvaluated(ParcelScene scene)
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(8, 5, 8));
            var entity = boxShape.attachedEntities.First();

            yield return null;
            yield return null;
            AssertMeshIsValid(entity.meshesInfo);

            // Move object to surpass the scene height boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 30, 8) };
            TestUtils.SetEntityTransform(scene, entity, transformModel);

            yield return null;
            yield return null;
            AssertMeshIsInvalid(entity.meshesInfo);
        }

        public static IEnumerator ChildShapeIsEvaluated(ParcelScene scene)
        {
            long entityId = 11;
            TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));

            yield return null;
            yield return null;
            AssertMeshIsValid(scene.entities[entityId].meshesInfo);

            // Attach child
            long childEntityId = 20;
            TestUtils.InstantiateEntityWithShape(scene, childEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
            
            yield return null;
            yield return null;
            AssertMeshIsValid(scene.entities[childEntityId].meshesInfo);

            TestUtils.SetEntityParent(scene, childEntityId, entityId);

            // Move parent object to surpass the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(18, 1, 18) };
            TestUtils.SetEntityTransform(scene, scene.entities[entityId], transformModel);

            yield return null;
            yield return null;
            AssertMeshIsInvalid(scene.entities[childEntityId].meshesInfo);
        }

        public static IEnumerator ChildShapeIsEvaluatedOnShapelessParent(ParcelScene scene)
        {
            // create shapeless parent entity
            long entityId = 11;
            TestUtils.CreateSceneEntity(scene, entityId);
            TestUtils.SetEntityTransform(scene, scene.entities[entityId], new Vector3(18, 1, 18), Quaternion.identity, Vector3.one);
            yield return null;

            AssertMeshIsValid(scene.entities[entityId].meshesInfo);

            // Attach child
            long childEntityId = 20;
            TestUtils.InstantiateEntityWithShape(scene, childEntityId, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(0, 0, 0));
            yield return null;

            TestUtils.SetEntityParent(scene, childEntityId, entityId);
            yield return null;

            AssertMeshIsInvalid(scene.entities[childEntityId].meshesInfo);

            // Move parent object to re-enter the scene boundaries
            TestUtils.SetEntityTransform(scene, scene.entities[entityId], new Vector3(8, 1, 8), Quaternion.identity, Vector3.one);

            yield return null;
            yield return null;

            AssertMeshIsValid(scene.entities[childEntityId].meshesInfo);
        }

        public static IEnumerator PShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(18, 1, 18));
            yield return null;

            var entity = boxShape.attachedEntities.First();
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);

            // Move object to re-enter the scene boundaries
            var transformModel = new DCLTransform.Model { position = new Vector3(8, 1, 8) };
            TestUtils.SetEntityTransform(scene, entity, transformModel);

            yield return null;
            yield return null;

            AssertMeshIsValid(entity.meshesInfo);
        }

        public static IEnumerator GLTFShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            yield return null;

            AssertMeshIsInvalid(entity.meshesInfo);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;
            yield return null;

            AssertMeshIsValid(entity.meshesInfo);
        }

        public static IEnumerator NFTShapeIsResetWhenReenteringBounds(ParcelScene scene)
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestUtils.SharedComponentAttach(component, entity);

            LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => shapeLoader.alreadyLoaded);

            AssertMeshIsInvalid(entity.meshesInfo);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;

            AssertMeshIsValid(entity.meshesInfo);
        }

        public static void AssertMeshIsInvalid(MeshesInfo meshesInfo)
        {
            Assert.IsTrue(meshesInfo.meshRootGameObject != null, "MeshRootGameObject is null. The object is valid when it shouldn't.");

            if (meshesInfo.renderers != null && meshesInfo.renderers.Length > 0)
            {
                if (Environment.i.world.sceneBoundsChecker.GetFeedbackStyle() is SceneBoundsFeedbackStyle_RedBox)
                {
                    bool hasWireframe = false;

                    foreach (Transform t in meshesInfo.innerGameObject.transform)
                    {
                        if (t.name.Contains("Wireframe"))
                            hasWireframe = true;
                    }

                    Assert.That(hasWireframe, Is.True); 
                }
                else
                {
                    for (int i = 0; i < meshesInfo.renderers.Length; i++)
                    {
                        Assert.IsFalse(meshesInfo.renderers[i].enabled, $"Renderer {meshesInfo.renderers[i].gameObject.name} is enabled when it shouldn't!");
                    }

                    foreach (Collider collider in meshesInfo.colliders)
                    {
                        Assert.IsFalse(collider.enabled, $"Collider {collider.gameObject.name} is enabled when it shouldn't!");
                    }
                }
            }

            if (meshesInfo.colliders.Count > 0)
            {
                foreach (Collider collider in meshesInfo.colliders)
                {
                    Assert.IsFalse(collider.enabled);
                }
            }
        }

        public static void AssertMeshIsValid(MeshesInfo meshesInfo)
        {
            if (meshesInfo.meshRootGameObject == null)
                return; // It's valid if there's no mesh

            if (Environment.i.world.sceneBoundsChecker.GetFeedbackStyle() is SceneBoundsFeedbackStyle_RedBox)
            {
                bool hasWireframe = false;

                foreach (Transform t in meshesInfo.innerGameObject.transform)
                {
                    if (t.name.Contains("Wireframe"))
                        hasWireframe = true;
                }
                
                Assert.IsFalse(hasWireframe);
            }
            else
            {
                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    Assert.IsTrue(meshesInfo.renderers[i].enabled, $"Renderer {meshesInfo.renderers[i].gameObject.name} is disabled when it should!");
                }

                foreach (Collider collider in meshesInfo.colliders)
                {
                    Assert.IsTrue(collider.enabled, $"Collider {collider.gameObject.name} is disabled when it should!");
                }
            }
        }
    }
}