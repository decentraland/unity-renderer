using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class GLTFImporterVisualTests : VisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ProcessTextureOffsetAndScale_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(ProcessTextureOffsetAndScale());
    }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ProcessTextureOffsetAndScale()
    {
        yield return InitVisualTestsScene("GLTFImporterVisualTests_ProcessTextureOffsetAndScale");

        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, Utils.GetTestsAssetsPath() + "/GLB/PlaneUVsOffset/planeUVsOffset.glb", out IDCLEntity entity);

        yield return gltfShape.routine;
        yield return new WaitForAllMessagesProcessed();
        yield return new WaitUntil(() => GLTFComponent.downloadingCount == 0);

        Vector3 camPos = new Vector3(0f, 2f, 5f);
        Vector3 camTarget = new Vector3(7.5f, 0f, 10f);

        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camPos, camTarget);
        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ProcessTexturesUVs_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(ProcessTexturesUVs());
    }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ProcessTexturesUVs()
    {
        yield return InitVisualTestsScene("GLTFImporterVisualTests_ProcessTexturesUVs");

        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, Utils.GetTestsAssetsPath() + "/GLB/PlaneUVsMultichannel/PlaneUVsMultichannel.glb", out IDCLEntity entity);

        yield return gltfShape.routine;
        yield return new WaitForAllMessagesProcessed();
        yield return new WaitUntil(() => GLTFComponent.downloadingCount == 0);

        Vector3 camPos = new Vector3(0f, 2f, 5f);
        Vector3 camTarget = new Vector3(7.5f, 0f, 10f);

        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, camPos, camTarget);
        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestHelpers.TakeSnapshot();
    }
}