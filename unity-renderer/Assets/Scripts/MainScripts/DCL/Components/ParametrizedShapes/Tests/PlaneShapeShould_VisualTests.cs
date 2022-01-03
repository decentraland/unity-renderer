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
    public IEnumerator ReceiveShadowsCorrectly_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(ReceiveShadowsCorrectly()); }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ReceiveShadowsCorrectly()
    {
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.up);

        var planeShape = TestUtils.CreateEntityWithPlaneShape(scene, Vector3.zero);
        var entity = planeShape.attachedEntities.First();

        TestUtils.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

        Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
        Vector3 camTarget = Vector3.zero;

        VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestUtils.TakeSnapshot("PlaneShape_ReceiveShadowsCorrectly", camera);
    }
}