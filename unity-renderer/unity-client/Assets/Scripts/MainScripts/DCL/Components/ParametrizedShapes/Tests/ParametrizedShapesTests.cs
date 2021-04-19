using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using UnityEngine.TestTools;

public class ParametrizedShapesTests : IntegrationTestSuite_Legacy
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
        Assert.IsTrue(boxShapeComponent.GetModel().withCollisions);

        // 3. Update component with missing values
        scene.SharedComponentUpdate(componentId, JsonUtility.ToJson(new BoxShape.Model { }));

        // 4. Check defaulted values
        Assert.IsTrue(boxShapeComponent.GetModel().withCollisions);
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

    [UnityTest]
    [TestCase(5, true, ExpectedResult = null)]
    [TestCase(5, false, ExpectedResult = null)]
    //TODO: When refactoring these tests to split them by shape, replicate this on them
    public IEnumerator UpdateWithCollisionInMultipleEntities(int entitiesCount, bool withCollision)
    {
        Environment.i.world.sceneBoundsChecker.Stop();

        // Arrange: set inverse of withCollision to trigger is dirty later
        BaseShape shapeComponent = TestHelpers.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BaseShape.Model {withCollisions = !withCollision});
        yield return shapeComponent.routine;
        List<IDCLEntity> entities = new List<IDCLEntity>();
        for (int i = 0; i < entitiesCount; i++)
        {
            IDCLEntity entity = TestHelpers.CreateSceneEntity(scene, $"entity{i}");
            TestHelpers.SharedComponentAttach(shapeComponent, entity);
            entities.Add(entity);
        }

        // Act: Update withCollision
        shapeComponent.UpdateFromModel(new BoxShape.Model {withCollisions = withCollision});
        yield return shapeComponent.routine;

        // Assert:
        foreach (IDCLEntity entity in entities)
        {
            for (int i = 0; i < entity.meshesInfo.colliders.Count; i++)
            {
                Assert.AreEqual(withCollision, entity.meshesInfo.colliders[i].enabled);
            }
        }
    }

    [UnityTest]
    [TestCase(5, true, ExpectedResult = null)]
    [TestCase(5, false, ExpectedResult = null)]
    //TODO: When refactoring these tests to split them by shape, replicate this on them
    public IEnumerator UpdateVisibilityInMultipleEntities(int entitiesCount, bool visible)
    {
        Environment.i.world.sceneBoundsChecker.Stop();

        // Arrange: set inverse of visible to trigger is dirty later
        BaseShape shapeComponent = TestHelpers.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BaseShape.Model {visible = !visible});
        yield return shapeComponent.routine;
        List<IDCLEntity> entities = new List<IDCLEntity>();
        for (int i = 0; i < entitiesCount; i++)
        {
            IDCLEntity entity = TestHelpers.CreateSceneEntity(scene, $"entity{i}");
            TestHelpers.SharedComponentAttach(shapeComponent, entity);
            entities.Add(entity);
        }

        // Act: Update visible
        shapeComponent.UpdateFromModel(new BoxShape.Model {visible = visible, withCollisions = true, isPointerBlocker = true});
        yield return shapeComponent.routine;

        // Assert:
        foreach (IDCLEntity entity in entities)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                Assert.AreEqual(visible, entity.meshesInfo.renderers[i].enabled);
            }
        }
    }
}