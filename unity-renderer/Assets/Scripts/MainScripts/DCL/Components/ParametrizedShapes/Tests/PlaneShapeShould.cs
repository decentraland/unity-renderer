using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlaneShapeShould : IntegrationTestSuite_Legacy
{
    [UnityTest]
    public IEnumerator BeUpdatedCorrectly()
    {
        string entityId = "3";
        TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Plane Instance", meshName);
        yield break;
    }


    [UnityTest]
    public IEnumerator UpdateUVsCorrectly()
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
    public IEnumerator DefaultMissingValuesOnUpdate()
    {
        var component =
            TestHelpers.SharedComponentCreate<PlaneShape, PlaneShape.Model>(scene, CLASS_ID.PLANE_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<PlaneShape.Model, PlaneShape>(scene,
            CLASS_ID.PLANE_SHAPE);
    }

    [UnityTest]
    public IEnumerator BeReplacedCorrectlyWhenAnotherComponentIsAttached()
    {
        yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<PlaneShape.Model, PlaneShape>(
            scene, CLASS_ID.PLANE_SHAPE);
    }
}