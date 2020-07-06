using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ParametrizedShapesTests : TestsBase
    {
        [UnityTest]
        public IEnumerator BoxShapeUpdate()
        {
            string entityId = "1";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);
            yield break;
        }

        [UnityTest]
        public IEnumerator SphereShapeUpdate()
        {
            string entityId = "2";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Sphere Instance", meshName);
            yield break;
        }

        [UnityTest]
        public IEnumerator PlaneShapeUpdate()
        {
            string entityId = "3";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Plane Instance", meshName);
            yield break;
        }


        [UnityTest]
        public IEnumerator PlaneShapeUpdateWithUVs()
        {
            float[] uvs = new float[] {0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1, 0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1};

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
            string entityId = "5";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
            yield break;
        }

        [UnityTest]
        public IEnumerator ConeShapeUpdate()
        {
            string entityId = "4";
            TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE, Vector3.zero);

            var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Cone50v0t1b2l2o Instance", meshName);
            yield break;
        }

        [UnityTest]
        public IEnumerator BoxShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
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

            BoxShape boxShapeComponent = (BoxShape) scene.GetSharedComponent(componentId);

            // 2. Check configured values
            Assert.IsTrue(boxShapeComponent.model.withCollisions);

            // 3. Update component with missing values
            scene.SharedComponentUpdate(componentId, JsonUtility.ToJson(new BoxShape.Model { }));

            // 4. Check defaulted values
            Assert.IsTrue(boxShapeComponent.model.withCollisions);
            yield break;
        }

        [UnityTest]
        public IEnumerator BoxShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<BoxShape.Model, BoxShape>(scene,
                CLASS_ID.BOX_SHAPE);
        }

        [UnityTest]
        public IEnumerator SphereShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
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
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<SphereShape.Model, SphereShape>(
                scene, CLASS_ID.SPHERE_SHAPE);
        }

        [UnityTest]
        public IEnumerator ConeShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            var component = TestHelpers.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, CLASS_ID.CONE_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<ConeShape.Model, ConeShape>(scene,
                CLASS_ID.CONE_SHAPE);
        }

        [UnityTest]
        public IEnumerator ConeShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<ConeShape.Model, ConeShape>(scene,
                CLASS_ID.CONE_SHAPE);
        }

        [UnityTest]
        public IEnumerator CylinderShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
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
            yield return
                TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<CylinderShape.Model, CylinderShape>(scene,
                    CLASS_ID.CYLINDER_SHAPE);
        }


        [UnityTest]
        public IEnumerator PlaneShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
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
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<PlaneShape.Model, PlaneShape>(
                scene, CLASS_ID.PLANE_SHAPE);
        }

        [UnityTest]
        public IEnumerator CollisionProperty()
        {
            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            yield return null;

            // BoxShape
            BaseShape.Model shapeModel = new BoxShape.Model();
            BaseShape shapeComponent = TestHelpers.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // SphereShape
            shapeModel = new SphereShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<SphereShape, BaseShape.Model>(scene, CLASS_ID.SPHERE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // ConeShape
            shapeModel = new ConeShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<ConeShape, BaseShape.Model>(scene, CLASS_ID.CONE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // CylinderShape
            shapeModel = new CylinderShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<CylinderShape, BaseShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // PlaneShape
            shapeModel = new PlaneShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<PlaneShape, BaseShape.Model>(scene, CLASS_ID.PLANE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator VisibleProperty()
        {
            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

            yield return null;

            // BoxShape
            BaseShape.Model shapeModel = new BoxShape.Model();
            BaseShape shapeComponent = TestHelpers.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // SphereShape
            shapeModel = new SphereShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<SphereShape, BaseShape.Model>(scene, CLASS_ID.SPHERE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // ConeShape
            shapeModel = new ConeShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<ConeShape, BaseShape.Model>(scene, CLASS_ID.CONE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // CylinderShape
            shapeModel = new CylinderShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<CylinderShape, BaseShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;

            // PlaneShape
            shapeModel = new PlaneShape.Model();
            shapeComponent = TestHelpers.SharedComponentCreate<PlaneShape, BaseShape.Model>(scene, CLASS_ID.PLANE_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);

            TestHelpers.DetachSharedComponent(scene, entityId, shapeComponent.id);
            shapeComponent.Dispose();
            yield return null;
        }
    }
}