using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Linq;
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
        public IEnumerator BoxShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<BoxShape.Model, BoxShape>(scene,
                CLASS_ID.BOX_SHAPE);
        }

        [UnityTest]
        public IEnumerator SphereShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component =
                TestHelpers.SharedComponentCreate<SphereShape, SphereShape.Model>(scene, CLASS_ID.SPHERE_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<SphereShape.Model, SphereShape>(scene,
                CLASS_ID.SPHERE_SHAPE);
        }

        [UnityTest]
        public IEnumerator SphereShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<SphereShape.Model, SphereShape>(
                scene, CLASS_ID.SPHERE_SHAPE);
        }

        [UnityTest]
        public IEnumerator ConeShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component = TestHelpers.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, CLASS_ID.CONE_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<ConeShape.Model, ConeShape>(scene,
                CLASS_ID.CONE_SHAPE);
        }

        [UnityTest]
        public IEnumerator ConeShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<ConeShape.Model, ConeShape>(scene,
                CLASS_ID.CONE_SHAPE);
        }

        [UnityTest]
        public IEnumerator CylinderShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component =
                TestHelpers.SharedComponentCreate<CylinderShape, CylinderShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<CylinderShape.Model, CylinderShape>(scene,
                CLASS_ID.CYLINDER_SHAPE);
        }

        [UnityTest]
        public IEnumerator CylinderShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            yield return
                TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<CylinderShape.Model, CylinderShape>(scene,
                    CLASS_ID.CYLINDER_SHAPE);
        }

        [UnityTest]
        public IEnumerator PlaneShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component =
                TestHelpers.SharedComponentCreate<PlaneShape, PlaneShape.Model>(scene, CLASS_ID.PLANE_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<PlaneShape.Model, PlaneShape>(scene,
                CLASS_ID.PLANE_SHAPE);
        }

        [UnityTest]
        public IEnumerator PlaneShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<PlaneShape.Model, PlaneShape>(
                scene, CLASS_ID.PLANE_SHAPE);
        }

        [UnityTest]
        public IEnumerator ShapeWithCollisionsUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "transform",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TRANSFORM,
                json = JsonConvert.SerializeObject(new
                {
                    position = Vector3.zero,
                    scale = new Vector3(1, 1, 1),
                    rotation = new
                    {
                        x = 0,
                        y = 0,
                        z = 0,
                        w = 1
                    }
                })
            }));

            // Update shape without collision
            string shapeId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    withCollisions = false
                }));

            yield return null;

            BoxShape shapeComponent = scene.GetSharedComponent(shapeId) as BoxShape;

            Assert.IsFalse(shapeComponent == null);
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>() == null);

            // Update shape with collision
            TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(scene, shapeComponent, new BoxShape.Model
            {
                withCollisions = true
            });
            yield return null;

            MeshCollider meshCollider = scene.entities[entityId].gameObject.GetComponentInChildren<MeshCollider>();

            Assert.IsTrue(meshCollider != null);

            // Update shape without collision
            TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(scene, shapeComponent, new BoxShape.Model
            {
                withCollisions = false
            });
            yield return null;

            Assert.IsFalse(meshCollider.enabled);

            // Update shape with collision
            TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(scene, shapeComponent, new BoxShape.Model
            {
                withCollisions = true
            });
            yield return null;

            Assert.IsTrue(meshCollider.enabled);
        }

        public IEnumerator ShapesVisibilityDefault()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero);
            SphereShape sphereShapeComponent = TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero);
            PlaneShape planeShapeComponent = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero);
            CylinderShape cylinderShapeComponent = TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero);
            ConeShape coneShapeComponent = TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero);

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(true, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, true));

            Assert.AreEqual(true, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, true));

            Assert.AreEqual(true, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, true));

            Assert.AreEqual(true, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, true));

            Assert.AreEqual(true, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator ShapesVisibilityCreateTrue()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent =
                TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, new BoxShape.Model { visible = true });
            SphereShape sphereShapeComponent =
                TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero, new SphereShape.Model { visible = true });
            PlaneShape planeShapeComponent =
                TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero, new PlaneShape.Model { visible = true });
            CylinderShape cylinderShapeComponent =
                TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero,
                    new CylinderShape.Model { visible = true });
            ConeShape coneShapeComponent =
                TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero, new ConeShape.Model { visible = true });

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(true, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, true));

            Assert.AreEqual(true, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, true));

            Assert.AreEqual(true, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, true));

            Assert.AreEqual(true, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, true));

            Assert.AreEqual(true, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator ShapesVisibilityCreateFalse()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, false);
            SphereShape sphereShapeComponent = TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero, false);
            PlaneShape planeShapeComponent = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero, false);
            CylinderShape cylinderShapeComponent =
                TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero, false);
            ConeShape coneShapeComponent = TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero, false);

            #endregion

            #region Act

            //EMPTY

            #endregion

            #region Assert

            Assert.AreEqual(false, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, false));

            Assert.AreEqual(false, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, false));

            Assert.AreEqual(false, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, false));

            Assert.AreEqual(false, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, false));

            Assert.AreEqual(false, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, false));

            #endregion
        }

        [UnityTest]
        public IEnumerator ShapesVisibilityUpdateTrue()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, false);
            SphereShape sphereShapeComponent = TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero, false);
            PlaneShape planeShapeComponent = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero, false);
            CylinderShape cylinderShapeComponent =
                TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero, false);
            ConeShape coneShapeComponent = TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero, false);

            #endregion

            #region Act

            BaseShape.Model visibleModel = new BaseShape.Model { visible = true };
            TestHelpers.SharedComponentUpdate(scene, boxShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, sphereShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, planeShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, cylinderShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, coneShapeComponent, visibleModel);

            #endregion

            #region Assert

            Assert.AreEqual(true, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, true));

            Assert.AreEqual(true, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, true));

            Assert.AreEqual(true, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, true));

            Assert.AreEqual(true, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, true));

            Assert.AreEqual(true, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, true));

            #endregion
        }

        [UnityTest]
        public IEnumerator ShapesVisibilityUpdateFalse()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            SphereShape sphereShapeComponent = TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero, true);
            PlaneShape planeShapeComponent = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero, true);
            CylinderShape cylinderShapeComponent = TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero, true);
            ConeShape coneShapeComponent = TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero, true);

            #endregion

            #region Act

            BaseShape.Model hiddenModel = new BaseShape.Model { visible = false };
            TestHelpers.SharedComponentUpdate(scene, boxShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, sphereShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, planeShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, cylinderShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, coneShapeComponent, hiddenModel);

            #endregion

            #region Assert

            Assert.AreEqual(false, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, false));

            Assert.AreEqual(false, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, false));

            Assert.AreEqual(false, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, false));

            Assert.AreEqual(false, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, false));

            Assert.AreEqual(false, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, false));

            #endregion
        }

        [UnityTest]
        public IEnumerator ShapesVisibilityUpdateMixed()
        {
            #region Arrange

            yield return InitScene();

            BoxShape boxShapeComponent = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            SphereShape sphereShapeComponent = TestHelpers.CreateEntityWithSphereShape(scene, Vector3.zero, false);
            PlaneShape planeShapeComponent = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero, true);
            CylinderShape cylinderShapeComponent =
                TestHelpers.CreateEntityWithCylinderShape(scene, Vector3.zero, false);
            ConeShape coneShapeComponent = TestHelpers.CreateEntityWithConeShape(scene, Vector3.zero, true);

            #endregion

            #region Act

            BaseShape.Model visibleModel = new BaseShape.Model { visible = true };
            BaseShape.Model hiddenModel = new BaseShape.Model { visible = false };
            TestHelpers.SharedComponentUpdate(scene, boxShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, sphereShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, planeShapeComponent, hiddenModel);
            TestHelpers.SharedComponentUpdate(scene, cylinderShapeComponent, visibleModel);
            TestHelpers.SharedComponentUpdate(scene, coneShapeComponent, hiddenModel);

            #endregion

            #region Assert

            Assert.AreEqual(false, boxShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(boxShapeComponent, false));

            Assert.AreEqual(true, sphereShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(sphereShapeComponent, true));

            Assert.AreEqual(false, planeShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(planeShapeComponent, false));

            Assert.AreEqual(true, cylinderShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(cylinderShapeComponent, true));

            Assert.AreEqual(false, coneShapeComponent.model.visible);
            Assert.IsTrue(CheckVisibility(coneShapeComponent, false));

            #endregion
        }

        private bool CheckVisibility(BaseDisposable shapeComponent, bool isVisible)
        {
            var meshGameObjects = shapeComponent.attachedEntities.Select(x => x.meshGameObject);

            foreach (GameObject meshGameObject in meshGameObjects)
            {
                MeshFilter[] meshFilters = meshGameObject.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();
                    if (renderer != null && isVisible != renderer.enabled)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
