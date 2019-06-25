using DCL;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

namespace Tests
{
    public class AvatarShapeTests : VisualTestsBase
    {
        public AvatarShape CreateAvatar(string name)
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            AvatarShape.Model model = new AvatarShape.Model() { useDummyModel = true, name = name };
            AvatarShape shape = TestHelpers.EntityComponentCreate<AvatarShape, AvatarShape.Model>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_SHAPE);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            return shape;
        }


        [UnityTest]
        [VisualTest]
        public IEnumerator AvatarShapeVisualTest_Generate()
        {
            yield return VisualTestHelpers.GenerateBaselineForTest(AvatarShapeVisualTest1());
        }

        [UnityTest]
        public IEnumerator AvatarShapeVisualTest1()
        {
            yield return InitScene();
            yield return VisualTestHelpers.InitVisualTestsScene("AvatarShape_A");

            GLTFSceneImporter.BudgetPerFrameInMilliseconds = float.MaxValue;

            AvatarShape avatar = CreateAvatar("Avatar #1");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return new WaitForSeconds(5.0f);

            yield return VisualTestHelpers.TakeSnapshot(new Vector3(-0.75f, 2.0f, 2.25f), avatar.transform.position + Vector3.up * 2.0f);
        }


        [UnityTest]
        [VisualTest]
        public IEnumerator AvatarShapeVisualTest2_Generate()
        {
            yield return VisualTestHelpers.GenerateBaselineForTest(AvatarShapeVisualTest2());
        }

        [UnityTest]
        public IEnumerator AvatarShapeVisualTest2()
        {
            yield return InitScene();
            yield return VisualTestHelpers.InitVisualTestsScene("AvatarShape_B");

            GLTFSceneImporter.BudgetPerFrameInMilliseconds = float.MaxValue;

            AvatarShape avatar = CreateAvatar("Avatar #2");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            yield return new WaitForSeconds(5.0f);

            yield return VisualTestHelpers.TakeSnapshot(new Vector3(-0.75f, 2.0f, 2.25f), avatar.transform.position + Vector3.up * 2.0f);
        }
    }
}
