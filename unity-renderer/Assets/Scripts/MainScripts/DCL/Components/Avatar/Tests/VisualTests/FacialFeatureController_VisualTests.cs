using System.Collections;
using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = DCL.WaitUntil;

namespace Tests
{
    public class FacialFeatureController_VisualTests : VisualTestsBase
    {
        [UnityTest]
        [VisualTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator MouthWithMask_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(MouthWithMask()); }

        [UnityTest]
        [VisualTest]
        [Category("Explicit")]
        [Explicit("Something is wrong with the avatar renderer material")]
        public IEnumerator MouthWithMask()
        {
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            var model = AvatarShapeTestHelpers.GetTestAvatarModel("Avatar #1", "TestAvatar_MaskMouth.json");
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, model);

            Vector3 camPos = new Vector3(-0.75f, 2.0f, 2.25f);
            Vector3 camTarget = avatar.transform.position + Vector3.up * 2.0f;

            VisualTestUtils.RepositionVisualTestsCamera(camera, camPos, camTarget);

            yield return new WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return VisualTestUtils.TakeSnapshot("AvatarShapeVisualTests_MouthWithMask", camera);
        }
    }
}