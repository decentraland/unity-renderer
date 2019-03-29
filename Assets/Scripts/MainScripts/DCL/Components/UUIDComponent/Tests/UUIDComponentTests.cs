using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityGLTF;
using Newtonsoft.Json;

namespace Tests
{
    public class UUIDComponentTests : TestsBase
    {
        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            TestHelpers.AddOnClickComponent(scene, entityId, "click-1");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null, "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShape()
        {
            yield return InitScene();

            string entityId = "1";

            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "click-1";
            OnClickComponent component = TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null, "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShapeAsynchronously()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null, "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null, "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesAfterBasicShapeIsAdded()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "click-1";
            TestHelpers.AddOnClickComponent(scene, entityId, clickUuid);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() == null, "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find("OnClickCollider") == null, "the OnClickCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
              JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null, "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh, "OnClickCollider should have the same mesh info as the mesh renderer");
        }
    }
}
