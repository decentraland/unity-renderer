using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    // Visual tests are disabled until we fix the resolution issue
    public class AvatarShapeVisualTests : VisualTestsBase
    {
        [UnityTest]
        [VisualTest]
        [NUnit.Framework.Explicit]
        [Category("Visual Tests")]
        public IEnumerator AvatarShapeVisualTest_Generate()
        {
            yield return VisualTestHelpers.GenerateBaselineForTest(AvatarShapeVisualTest1());
        }

        [UnityTest]
        [VisualTest]
        [Category("Visual Tests")]
        [Category("Explicit")]
        [NUnit.Framework.Explicit]
        public IEnumerator AvatarShapeVisualTest1()
        {
            yield return InitScene();
            yield return VisualTestHelpers.InitVisualTestsScene("AvatarShape_A");

            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #1", "TestAvatar.json");

            Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
            Vector3 camTarget = avatar.transform.position + Vector3.up * 2.0f;

            VisualTestHelpers.RepositionVisualTestsCamera(camPos, camTarget);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return new WaitForSeconds(5.0f);

            yield return VisualTestHelpers.TakeSnapshot();
        }


        [UnityTest]
        [VisualTest]
        [NUnit.Framework.Explicit]
        [Category("Visual Tests")]
        [Category("Explicit")]
        public IEnumerator AvatarShapeVisualTest2_Generate()
        {
            yield return VisualTestHelpers.GenerateBaselineForTest(AvatarShapeVisualTest2());
        }

        [UnityTest]
        [VisualTest]
        [Category("Visual Tests")]
        [Category("Explicit")]
        [NUnit.Framework.Explicit]
        public IEnumerator AvatarShapeVisualTest2()
        {
            yield return InitScene();
            yield return VisualTestHelpers.InitVisualTestsScene("AvatarShape_B");

            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #2", "TestAvatar2.json");

            Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
            Vector3 camTarget = avatar.transform.position + Vector3.up * 2.0f;

            VisualTestHelpers.RepositionVisualTestsCamera(camPos, camTarget);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return VisualTestHelpers.TakeSnapshot();
        }
    }
}
