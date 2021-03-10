using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PBRMaterialVisualTests : VisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator AlphaTextureShouldWork_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(AlphaTextureShouldWork());
    }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator AlphaTextureShouldWork()
    {
        yield return InitVisualTestsScene("PBRMaterialVisualTests_AlphaTextureShouldWork");
        DCLTexture texture = TestHelpers.CreateDCLTexture(scene, Utils.GetTestsAssetsPath() + "/Images/alphaTexture.png");
        yield return texture.routine;
        Vector3 camTarget = new Vector3(5, 2, 5);
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camTarget - new Vector3(2, -1, 2), camTarget);

        PBRMaterial matPBR = TestHelpers.CreateEntityWithPBRMaterial(scene, new PBRMaterial.Model
        {
            albedoTexture = texture.id,
            transparencyMode = 2,
            albedoColor = Color.blue
        }, camTarget, out DecentralandEntity entity);
        yield return matPBR.routine;


        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }
}