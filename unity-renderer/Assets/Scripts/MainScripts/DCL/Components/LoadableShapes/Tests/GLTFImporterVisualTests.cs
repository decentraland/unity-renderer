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
    public IEnumerator ProcessTextureOffsetAndScale_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(ProcessTextureOffsetAndScale()); }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ProcessTextureOffsetAndScale()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/PlaneUVsOffset/planeUVsOffset.glb", out IDCLEntity entity);

        yield return gltfShape.routine;
        yield return new WaitForAllMessagesProcessed();
        yield return new WaitUntil(() => GLTFComponent.downloadingCount == 0);

        Vector3 camPos = new Vector3(0f, 2f, 5f);
        Vector3 camTarget = new Vector3(7.5f, 0f, 10f);

        VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);
        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestUtils.TakeSnapshot("GLTFImporterVisualTests_ProcessTextureOffsetAndScale", camera);
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ProcessTexturesUVs_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(ProcessTexturesUVs()); }

    [UnityTest]
    [VisualTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator ProcessTexturesUVs()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/PlaneUVsMultichannel/PlaneUVsMultichannel.glb", out IDCLEntity entity);

        yield return gltfShape.routine;
        yield return new WaitForAllMessagesProcessed();
        yield return new WaitUntil(() => GLTFComponent.downloadingCount == 0);

        Vector3 camPos = new Vector3(0f, 2f, 5f);
        Vector3 camTarget = new Vector3(7.5f, 0f, 10f);

        VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);
        yield return new WaitForAllMessagesProcessed();

        yield return VisualTestUtils.TakeSnapshot("GLTFImporterVisualTests_ProcessTexturesUVs", camera);
    }
}