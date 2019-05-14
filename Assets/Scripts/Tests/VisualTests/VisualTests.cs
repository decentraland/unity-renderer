using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class VisualTests : VisualTestsBase
    {
        [UnityTest]
        [Explicit]
        public IEnumerator VisualTestStub_Generate()
        {
            yield return VisualTestHelpers.GenerateBaselineForTest(VisualTestStub());
        }

        [UnityTest]
        [VisualTest]
        public IEnumerator VisualTestStub()
        {
            yield return InitScene();
            yield return VisualTestHelpers.InitVisualTestsScene("BaseTest");

            string src = TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb";
            GLTFShape trevorGLTFShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, src);
            yield return trevorGLTFShape.routine;

            yield return VisualTestHelpers.TakeSnapshot(new Vector3(2f, 2f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, 10f));
            yield return VisualTestHelpers.TakeSnapshot(new Vector3(-2f, 2f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, -2f));

        }
    }
}
