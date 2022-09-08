using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

public class ParametrizedShapesTests : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        coreComponentsPlugin = new CoreComponentsPlugin();
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator BoxShapeUpdate()
    {
        long entityId = 1;
        TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Box Instance", meshName);
        yield break;
    }

    [UnityTest]
    public IEnumerator SphereShapeUpdate()
    {
        long entityId = 2;
        TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.SPHERE_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Sphere Instance", meshName);
        yield break;
    }

    [UnityTest]
    public IEnumerator CylinderShapeUpdate()
    {
        long entityId = 5;
        TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CYLINDER_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Cylinder Instance", meshName);
        yield break;
    }

    [UnityTest]
    public IEnumerator ConeShapeUpdate()
    {
        long entityId = 5;
        TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.CONE_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;

        Assert.AreEqual("DCL Cone50v0t1b2l2o Instance", meshName);
        yield break;
    }

    [UnityTest]
    public IEnumerator BoxShapeComponentMissingValuesGetDefaultedOnUpdate()
    {
        long entityId = 5;
        TestUtils.CreateSceneEntity(scene, entityId);

        // 1. Create component with non-default configs
        string componentJSON = JsonUtility.ToJson(new BoxShape.Model
        {
            withCollisions = true
        });

        string componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
            componentJSON
        );

        BoxShape boxShapeComponent = (BoxShape) scene.componentsManagerLegacy.GetSceneSharedComponent(componentId);

        // 2. Check configured values
        Assert.IsTrue(boxShapeComponent.GetModel().withCollisions);

        // 3. Update component with missing values
        scene.componentsManagerLegacy.SceneSharedComponentUpdate(componentId, JsonUtility.ToJson(new BoxShape.Model { }));

        // 4. Check defaulted values
        Assert.IsTrue(boxShapeComponent.GetModel().withCollisions);
        yield break;
    }

    [UnityTest]
    public IEnumerator BoxShapeAttachedGetsReplacedOnNewAttachment()
    {
        yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<BoxShape.Model, BoxShape>(scene,
            CLASS_ID.BOX_SHAPE);
    }

    [UnityTest]
    public IEnumerator SphereShapeComponentMissingValuesGetDefaultedOnUpdate()
    {
        var component =
            TestUtils.SharedComponentCreate<SphereShape, SphereShape.Model>(scene, CLASS_ID.SPHERE_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestUtils.TestSharedComponentDefaultsOnUpdate<SphereShape.Model, SphereShape>(scene,
            CLASS_ID.SPHERE_SHAPE);
    }

    [UnityTest]
    public IEnumerator SphereShapeAttachedGetsReplacedOnNewAttachment()
    {
        yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<SphereShape.Model, SphereShape>(
            scene, CLASS_ID.SPHERE_SHAPE);
    }

    [UnityTest]
    public IEnumerator ConeShapeComponentMissingValuesGetDefaultedOnUpdate()
    {
        var component = TestUtils.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, CLASS_ID.CONE_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestUtils.TestSharedComponentDefaultsOnUpdate<ConeShape.Model, ConeShape>(scene,
            CLASS_ID.CONE_SHAPE);
    }

    [UnityTest]
    public IEnumerator ConeShapeAttachedGetsReplacedOnNewAttachment()
    {
        yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<ConeShape.Model, ConeShape>(scene,
            CLASS_ID.CONE_SHAPE);
    }

    [UnityTest]
    public IEnumerator CylinderShapeComponentMissingValuesGetDefaultedOnUpdate()
    {
        var component =
            TestUtils.SharedComponentCreate<CylinderShape, CylinderShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestUtils.TestSharedComponentDefaultsOnUpdate<CylinderShape.Model, CylinderShape>(scene,
            CLASS_ID.CYLINDER_SHAPE);
    }

    [UnityTest]
    public IEnumerator CylinderShapeAttachedGetsReplacedOnNewAttachment()
    {
        yield return
            TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<CylinderShape.Model, CylinderShape>(scene,
                CLASS_ID.CYLINDER_SHAPE);
    }

    [UnityTest]
    public IEnumerator CollisionProperty()
    {
        long entityId = 5;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];

        TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

        yield return null;

        // BoxShape
        BaseShape.Model shapeModel = new BoxShape.Model();
        BaseShape shapeComponent = TestUtils.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // SphereShape
        shapeModel = new SphereShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<SphereShape, BaseShape.Model>(scene, CLASS_ID.SPHERE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // ConeShape
        shapeModel = new ConeShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<ConeShape, BaseShape.Model>(scene, CLASS_ID.CONE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // CylinderShape
        shapeModel = new CylinderShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<CylinderShape, BaseShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // PlaneShape
        shapeModel = new PlaneShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<PlaneShape, BaseShape.Model>(scene, CLASS_ID.PLANE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;
    }

    [UnityTest]
    public IEnumerator VisiblePropertyWithRenderers()
    {
        long entityId = 5;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];

        TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

        yield return null;

        // BoxShape
        BaseShape.Model shapeModel = new BoxShape.Model();
        BaseShape shapeComponent = TestUtils.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestRenderersWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // SphereShape
        shapeModel = new SphereShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<SphereShape, BaseShape.Model>(scene, CLASS_ID.SPHERE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestRenderersWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // ConeShape
        shapeModel = new ConeShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<ConeShape, BaseShape.Model>(scene, CLASS_ID.CONE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestRenderersWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // CylinderShape
        shapeModel = new CylinderShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<CylinderShape, BaseShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestRenderersWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // PlaneShape
        shapeModel = new PlaneShape.Model();
        shapeComponent = TestUtils.SharedComponentCreate<PlaneShape, BaseShape.Model>(scene, CLASS_ID.PLANE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestRenderersWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;
    }


    [UnityTest]
    public IEnumerator VisiblePropertyWithOnPointerEvents()
    {
        // Add UUID plugin to enable OnPointerEvent components for this integration test.
        UUIDEventsPlugin eventsPlugin = new UUIDEventsPlugin();

        long entityId = 2134;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];

        TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});

        yield return null;

        // BoxShape
        BaseShape.Model shapeModel = new BoxShape.Model();
        BaseShape shapeComponent =
            TestUtils.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // SphereShape
        shapeModel = new SphereShape.Model();
        shapeComponent =
            TestUtils.SharedComponentCreate<SphereShape, BaseShape.Model>(scene, CLASS_ID.SPHERE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // ConeShape
        shapeModel = new ConeShape.Model();
        shapeComponent =
            TestUtils.SharedComponentCreate<ConeShape, BaseShape.Model>(scene, CLASS_ID.CONE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // CylinderShape
        shapeModel = new CylinderShape.Model();
        shapeComponent =
            TestUtils.SharedComponentCreate<CylinderShape, BaseShape.Model>(scene, CLASS_ID.CYLINDER_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        // PlaneShape
        shapeModel = new PlaneShape.Model();
        shapeComponent =
            TestUtils.SharedComponentCreate<PlaneShape, BaseShape.Model>(scene, CLASS_ID.PLANE_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);

        TestUtils.DetachSharedComponent(scene, entityId, shapeComponent.id);
        shapeComponent.Dispose();
        yield return null;

        eventsPlugin.Dispose();
    }

    [UnityTest]
    [TestCase(5, true, ExpectedResult = null)]
    [TestCase(5, false, ExpectedResult = null)]
    //TODO: When refactoring these tests to split them by shape, replicate this on them
    public IEnumerator UpdateWithCollisionInMultipleEntities(int entitiesCount, bool withCollision)
    {
        Environment.i.world.sceneBoundsChecker.Stop();

        // Arrange: set inverse of withCollision to trigger is dirty later
        BaseShape shapeComponent = TestUtils.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BaseShape.Model { withCollisions = !withCollision });
        yield return shapeComponent.routine;
        List<IDCLEntity> entities = new List<IDCLEntity>();
        for (int i = 0; i < entitiesCount; i++)
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene, i);
            TestUtils.SharedComponentAttach(shapeComponent, entity);
            entities.Add(entity);
        }

        // Act: Update withCollision
        shapeComponent.UpdateFromModel(new BoxShape.Model { withCollisions = withCollision });
        yield return shapeComponent.routine;

        // Assert:
        foreach (IDCLEntity entity in entities)
        {
            foreach (Collider collider in entity.meshesInfo.colliders)
            {
                Assert.AreEqual(withCollision, collider.enabled);
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
        BaseShape shapeComponent = TestUtils.SharedComponentCreate<BoxShape, BaseShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BaseShape.Model { visible = !visible });
        yield return shapeComponent.routine;
        List<IDCLEntity> entities = new List<IDCLEntity>();
        for (int i = 0; i < entitiesCount; i++)
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene, i);
            TestUtils.SharedComponentAttach(shapeComponent, entity);
            entities.Add(entity);
        }

        // Act: Update visible
        shapeComponent.UpdateFromModel(new BoxShape.Model { visible = visible, withCollisions = true, isPointerBlocker = true });
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