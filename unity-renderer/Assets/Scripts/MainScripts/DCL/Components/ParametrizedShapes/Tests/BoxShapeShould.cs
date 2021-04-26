using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoxShapeShould : IntegrationTestSuite_Legacy
{
    [UnityTest]
    public IEnumerator BeUpdatedCorrectly()
    {
        string entityId = "3";
        TestHelpers.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Box Instance", meshName);
        yield break;
    }


    [UnityTest]
    public IEnumerator UpdateUVsCorrectly()
    {
        float[] uvs = new float[] {
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1
        };

        IDCLEntity entity;

        BoxShape box = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
            scene,
            DCL.Models.CLASS_ID.BOX_SHAPE,
            Vector3.zero,
            out entity,
            new BoxShape.Model()
            {
                uvs = uvs
            });

        yield return box.routine;

        Assert.IsTrue(entity != null);
        Assert.IsTrue(box != null);
        Assert.IsTrue(box.currentMesh != null);
        CollectionAssert.AreEqual(Utils.FloatArrayToV2List(uvs), box.currentMesh.uv);
    }

    [UnityTest]
    public IEnumerator DefaultMissingValuesOnUpdate()
    {
        var component =
            TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<BoxShape.Model, BoxShape>(scene,
            CLASS_ID.BOX_SHAPE);
    }

    [UnityTest]
    public IEnumerator BeReplacedCorrectlyWhenAnotherComponentIsAttached()
    {
        yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<BoxShape.Model, BoxShape>(
            scene, CLASS_ID.BOX_SHAPE);
    }
}