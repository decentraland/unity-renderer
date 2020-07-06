using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests_DebugMode : TestsBase
    {
        protected override bool enableSceneIntegrityChecker => false;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return SetUp_SceneController(debugMode: true);
            yield return SetUp_CharacterController();

            sceneController.boundariesChecker.timeBetweenChecks = 0f;
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);
            yield return SBC_Asserts.ChildShapeIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParentDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return SBC_Asserts.HeightIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }
    }
}