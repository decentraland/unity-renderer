using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    // Visual tests are disabled until we fix the resolution issue
    public class AvatarShapeVisualTests : VisualTestsBase
    {
        private IWearablesCatalogService wearablesCatalogService;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        }

        protected override IEnumerator TearDown()
        {
            wearablesCatalogService.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        [VisualTest]
        [NUnit.Framework.Explicit]
        [Category("Explicit")]
        public IEnumerator AvatarShapeVisualTest_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(AvatarShapeVisualTest1()); }

        [UnityTest]
        [VisualTest]
        [Category("Explicit")]
        [NUnit.Framework.Explicit]
        public IEnumerator AvatarShapeVisualTest1()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Avatar #1", "TestAvatar.json");

            Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
            Vector3 camTarget = avatar.transform.position + Vector3.up * 2.0f;

            VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return new WaitForSeconds(5.0f);

            yield return VisualTestUtils.TakeSnapshot("AvatarShape_A", camera);
        }

        [UnityTest]
        [VisualTest]
        [NUnit.Framework.Explicit]
        [Category("Explicit")]
        public IEnumerator AvatarShapeVisualTest2_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(AvatarShapeVisualTest2()); }

        [UnityTest]
        [VisualTest]
        [Category("Explicit")]
        [NUnit.Framework.Explicit]
        public IEnumerator AvatarShapeVisualTest2()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Avatar #2", "TestAvatar2.json");

            Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
            Vector3 camTarget = avatar.transform.position + Vector3.up * 2.0f;

            VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return VisualTestUtils.TakeSnapshot("AvatarShape_B", camera);
        }
    }
}
