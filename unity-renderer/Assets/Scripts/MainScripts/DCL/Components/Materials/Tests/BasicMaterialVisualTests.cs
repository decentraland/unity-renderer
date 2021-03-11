using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BasicMaterialVisualTests : VisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowFalseShouldWork_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(CastShadowFalseShouldWork());
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowFalseShouldWork()
    {
        yield return InitVisualTestsScene("BasicMaterialVisualTests_CastShadowFalseShouldWork");

        Vector3 camTarget = new Vector3(5, 1, 5);
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camTarget + new Vector3(5, 1, 5), camTarget);

        BasicMaterial basicMaterialComponent = TestHelpers.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = false
        }, camTarget, out DecentralandEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowTrueShouldWork_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(CastShadowTrueShouldWork());
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowTrueShouldWork()
    {
        yield return InitVisualTestsScene("BasicMaterialVisualTests_CastShadowTrueShouldWork");

        Vector3 camTarget = new Vector3(5, 1, 5);
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camTarget + new Vector3(5, 1, 5), camTarget);

        BasicMaterial basicMaterialComponent = TestHelpers.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = true
        }, camTarget, out DecentralandEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }
}