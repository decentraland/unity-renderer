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
    public class ParametrizedShapesTests : TestsBase
    {

        [UnityTest]
        public IEnumerator BoxShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);
        }

        [UnityTest]
        public IEnumerator SphereShapeUpdate()
        {
            yield return InitScene();

            string entityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Sphere Instance", meshName);
        }

        [UnityTest]
        public IEnumerator PlaneShapeUpdate()
        {
            yield return InitScene();

            string entityId = "3";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Plane Instance", meshName);
        }


        [UnityTest]
        public IEnumerator PlaneShapeUpdateWithUVs()
        {
            yield return InitScene();

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
                });

            yield return plane.routine;

            Assert.IsTrue(entity != null);
            Assert.IsTrue(plane != null);
            Assert.IsTrue(plane.currentMesh != null);
            CollectionAssert.AreEqual(Utils.FloatArrayToV2List(uvs), plane.currentMesh.uv);
        }

        [UnityTest]
        public IEnumerator CylinderShapeUpdate()
        {
            yield return InitScene();

            string entityId = "5";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
        }

        [UnityTest]
        public IEnumerator ConeShapeUpdate()
        {
            yield return InitScene();

            string entityId = "4";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cone50v0t1b2l2o Instance", meshName);
        }

        [UnityTest]
        public IEnumerator BoxShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new BoxShape.Model
            {
                withCollisions = true
            });

            string componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
              componentJSON
            );

            BoxShape boxShapeComponent = (BoxShape)scene.GetSharedComponent(componentId);

            // 2. Check configured values
            Assert.IsTrue(boxShapeComponent.model.withCollisions);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new BoxShape.Model { });
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            // 4. Check defaulted values
            Assert.IsFalse(boxShapeComponent.model.withCollisions);
        }

        [UnityTest]
        public IEnumerator ConeShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new ConeShape.Model
            {
                radiusBottom = 0.5f,
                segmentsHeight = 0.3f,
                segmentsRadial = 12f,
                openEnded = false,
                arc = 123f,
                withCollisions = true
            });

            string componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE,
              componentJSON
            );

            ConeShape coneShapeComponent = (ConeShape)scene.GetSharedComponent(componentId);

            // 2. Check configured values
            Assert.AreEqual(0.5f, coneShapeComponent.model.radiusBottom);
            Assert.AreEqual(0.3f, coneShapeComponent.model.segmentsHeight);
            Assert.AreEqual(12f, coneShapeComponent.model.segmentsRadial);
            Assert.AreEqual(123f, coneShapeComponent.model.arc);
            Assert.IsTrue(coneShapeComponent.model.withCollisions);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new ConeShape.Model
            {
                openEnded = false
            });

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            // 4. Check defaulted values
            Assert.AreEqual(1f, coneShapeComponent.model.radiusBottom);
            Assert.AreEqual(1f, coneShapeComponent.model.segmentsHeight);
            Assert.AreEqual(36f, coneShapeComponent.model.segmentsRadial);
            Assert.AreEqual(360f, coneShapeComponent.model.arc);
            Assert.IsFalse(coneShapeComponent.model.withCollisions);
        }

        [UnityTest]
        public IEnumerator CylinderShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new CylinderShape.Model
            {
                radiusBottom = 0.5f,
                segmentsHeight = 0.3f,
                segmentsRadial = 12f,
                openEnded = false,
                arc = 123f,
                withCollisions = true
            });

            string componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE,
              componentJSON
            );

            CylinderShape cylinderShapeComponent = (CylinderShape)scene.GetSharedComponent(componentId);

            // 2. Check configured values
            Assert.AreEqual(0.5f, cylinderShapeComponent.model.radiusBottom);
            Assert.AreEqual(0.3f, cylinderShapeComponent.model.segmentsHeight);
            Assert.AreEqual(12f, cylinderShapeComponent.model.segmentsRadial);
            Assert.AreEqual(123f, cylinderShapeComponent.model.arc);
            Assert.IsTrue(cylinderShapeComponent.model.withCollisions);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new CylinderShape.Model
            {
                openEnded = false
            });

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            // 4. Check defaulted values
            Assert.AreEqual(1f, cylinderShapeComponent.model.radiusBottom);
            Assert.AreEqual(1f, cylinderShapeComponent.model.segmentsHeight);
            Assert.AreEqual(36f, cylinderShapeComponent.model.segmentsRadial);
            Assert.AreEqual(360f, cylinderShapeComponent.model.arc);
            Assert.IsFalse(cylinderShapeComponent.model.withCollisions);
        }

        [UnityTest]
        public IEnumerator PlaneShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new PlaneShape.Model
            {
                width = 0.45f,
                height = 0.77f,
                withCollisions = true
            });

            string componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE,
              componentJSON
            );

            PlaneShape planeShapeComponent = (PlaneShape)scene.GetSharedComponent(componentId);

            // 2. Check configured values
            Assert.AreEqual(0.45f, planeShapeComponent.model.width);
            Assert.AreEqual(0.77f, planeShapeComponent.model.height);
            Assert.IsTrue(planeShapeComponent.model.withCollisions);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new PlaneShape.Model { });

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            // 4. Check defaulted values
            Assert.AreEqual(1f, planeShapeComponent.model.width);
            Assert.AreEqual(1f, planeShapeComponent.model.height);
            Assert.IsFalse(planeShapeComponent.model.withCollisions);
        }
    }
}
