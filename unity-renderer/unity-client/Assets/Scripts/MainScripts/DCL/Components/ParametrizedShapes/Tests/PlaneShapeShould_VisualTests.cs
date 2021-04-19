using System.Collections;
using System.Linq;
using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlaneShapeShould_VisualTests : VisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ReceiveShadowsCorrectly_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(ReceiveShadowsCorrectly());
    }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ReceiveShadowsCorrectly()
    {
        yield return InitVisualTestsScene("PlaneShape_ReceiveShadowsCorrectly");

        TestHelpers.CreateEntityWithBoxShape(scene, Vector3.up);

        var planeShape = TestHelpers.CreateEntityWithPlaneShape(scene, Vector3.zero);
        var entity = planeShape.attachedEntities.First();

        TestHelpers.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

        Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
        Vector3 camTarget = Vector3.zero;

        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camPos, camTarget);

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }
}