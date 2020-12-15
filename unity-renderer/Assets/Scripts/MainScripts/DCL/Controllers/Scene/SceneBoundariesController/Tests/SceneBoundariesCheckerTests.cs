using DCL.Models;
using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests : TestsBase
    {
        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            Environment.i.sceneBoundsChecker.timeBetweenChecks = 0f;
        }

        [UnityTest]
        public IEnumerator EntitiesAreBeingCorrectlyRegistered()
        {
            yield return SBC_Asserts.EntitiesAreBeingCorrectlyRegistered(scene);
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
        }

        [UnityTest]
        [NUnit.Framework.Explicit("This test started failing on the CI out of the blue. Will be re-enabled after implementing a solution dealing with high delta times")]
        [Category("Explicit")]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluated()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluated()
        {
            yield return SBC_Asserts.HeightIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator AudioSourceIsDisabled()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(-28, 1, 8)});
            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.IsFalse(dclAudioSource.enabled);
        }

        [UnityTest]
        public IEnumerator AudioSourceWithMeshIsDisabled()
        {
            TestHelpers.CreateEntityWithGLTFShape(scene, new Vector3(8, 1, 8), Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb", out var entity);
            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(-28, 1, 8)});
            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.IsFalse(dclAudioSource.enabled);
        }

        [UnityTest]
        public IEnumerator AudioSourcePlaysBackOnReentering()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(-28, 1, 8)});
            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(2, 1, 2)});
            yield return null;

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.IsTrue(dclAudioSource.enabled);
        }
    }
}