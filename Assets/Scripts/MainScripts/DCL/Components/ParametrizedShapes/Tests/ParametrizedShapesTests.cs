using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ParametrizedShapesTests
    {

        [UnityTest]
        public IEnumerator BoxShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);
        }

        [UnityTest]
        public IEnumerator SphereShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Sphere Instance", meshName);
        }

        [UnityTest]
        public IEnumerator PlaneShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "3";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Plane Instance", meshName);
        }


        [UnityTest]
        public IEnumerator PlaneShapeUpdateWithUVs()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            float[] uvs = new float[] { 0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1, 0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1 };

            DecentralandEntity entity;

            PlaneShape plane = TestHelpers.InstantiateEntityWithShape<PlaneShape, PlaneShape.Model>(
                scene,
                DCL.Models.CLASS_ID.PLANE_SHAPE,
                Vector3.zero,
                out entity,
                new PlaneShape.Model()
                {
                    height = 1,
                    width = 1,
                    uvs = uvs
                } );


            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(entity != null);
            Assert.IsTrue(plane != null);
            Assert.IsTrue(plane.currentMesh != null);
            CollectionAssert.AreEqual(Utils.FloatArrayToV2List(uvs), plane.currentMesh.uv);
        }

        [UnityTest]
        public IEnumerator CylinderShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "5";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
        }

        [UnityTest]
        public IEnumerator ConeShapeUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            string entityId = "4";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cone50v0t1b2l2o Instance", meshName);
        }
    }
}
