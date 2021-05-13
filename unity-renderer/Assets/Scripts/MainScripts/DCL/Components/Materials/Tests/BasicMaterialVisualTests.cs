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
    public IEnumerator CastShadowFalseShouldWork_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(CastShadowFalseShouldWork()); }

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
        }, camTarget, out IDCLEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CastShadowTrueShouldWork_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(CastShadowTrueShouldWork()); }

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
        }, camTarget, out IDCLEntity entity);
        yield return basicMaterialComponent.routine;

        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    public IEnumerator StandardConfigurations_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(StandardConfigurations()); }

    [UnityTest]
    [VisualTest]
    public IEnumerator StandardConfigurations()
    {
        yield return InitVisualTestsScene("BasicMaterialVisualTests_StandardConfigurations");

        Vector3 camTarget = new Vector3(8, 3, 8);
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, new Vector3(9, 4, 17), camTarget);

        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero,
            new LoadableShape.Model
            {
                src = TestAssetsUtils.GetPath() + "/GLB/MaterialsScene.glb"
            }, out IDCLEntity entity);
        TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(0, 0, 8), rotation = Quaternion.Euler(90, 180, 0) });

        LoadWrapper loader = GLTFShape.GetLoaderForEntity(entity);
        yield return new WaitUntil(() => loader.alreadyLoaded);

        yield return VisualTestHelpers.TakeSnapshot();
    }
}