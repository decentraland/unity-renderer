using System.Collections;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoxShapeShould : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator BeUpdatedCorrectly()
    {
        long entityId = 3;
        TestUtils.InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero);

        var meshName = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Box Instance", meshName);
        yield break;
    }

    [UnityTest]
    public IEnumerator UpdateUVsCorrectly()
    {
        float[] uvs = new float[]
        {
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
            0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1
        };

        IDCLEntity entity;

        BoxShape box = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
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
            TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE);
        yield return component.routine;

        Assert.IsFalse(component == null);

        yield return TestUtils.TestSharedComponentDefaultsOnUpdate<BoxShape.Model, BoxShape>(scene,
            CLASS_ID.BOX_SHAPE);
    }

    [UnityTest]
    public IEnumerator BeReplacedCorrectlyWhenAnotherComponentIsAttached()
    {
        yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<BoxShape.Model, BoxShape>(
            scene, CLASS_ID.BOX_SHAPE);
    }
}