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
    public IEnumerator CastShadowFalseShouldWork_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(CastShadowFalseShouldWork()); }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowFalseShouldWork()
    {
        Vector3 camTarget = new Vector3(5, 1, 5);
        VisualTestUtils.RepositionVisualTestsCamera(camera, camTarget + new Vector3(5, 1, 5), camTarget);

        BasicMaterial basicMaterialComponent = TestUtils.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = false
        }, camTarget, out IDCLEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestUtils.TakeSnapshot("BasicMaterialVisualTests_CastShadowFalseShouldWork", camera);
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowTrueShouldWork_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(CastShadowTrueShouldWork()); }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowTrueShouldWork()
    {
        Vector3 camTarget = new Vector3(5, 1, 5);
        VisualTestUtils.RepositionVisualTestsCamera(camera, camTarget + new Vector3(5, 1, 5), camTarget);

        BasicMaterial basicMaterialComponent = TestUtils.CreateEntityWithBasicMaterial(scene, new BasicMaterial.Model
        {
            alphaTest = 1f,
            castShadows = true
        }, camTarget, out IDCLEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestUtils.TakeSnapshot("BasicMaterialVisualTests_CastShadowTrueShouldWork", camera);
    }
}