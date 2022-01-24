using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
            Environment.i.world.sceneBoundsChecker.timeBetweenChecks = 0f;
        }

        [UnityTest]
        public IEnumerator EntitiesAreBeingCorrectlyRegistered() { yield return SBC_Asserts.EntitiesAreBeingCorrectlyRegistered(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds() { yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBounds() { yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds() { yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds() { yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene); }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluated() { yield return SBC_Asserts.ChildShapeIsEvaluated(scene); }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent() { yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene); }

        [UnityTest]
        public IEnumerator HeightIsEvaluated() { yield return SBC_Asserts.HeightIsEvaluated(scene); }

        [UnityTest]
        public IEnumerator AudioSourceIsMuted()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator AudioSourceWithMeshIsDisabled()
        {
            TestUtils.CreateEntityWithGLTFShape(scene, new Vector3(8, 1, 8), TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb", out var entity);
            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator HighPrioEntitiesAreRegistered_Position()
        {
            var boxShape1 = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f, 0, 0));
            var boxShape2 = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(0, SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f, 0));
            var boxShape3 = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(0, 0, SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f));

            var entity1 = boxShape1.attachedEntities.First();
            var entity2 = boxShape2.attachedEntities.First();
            var entity3 = boxShape3.attachedEntities.First();

            Assert.AreEqual(3, Environment.i.world.sceneBoundsChecker.highPrioEntitiesToCheckCount, "entities to check can't be zero!");

            yield return null;

            TestUtils.RemoveSceneEntity(scene, entity1.entityId);
            TestUtils.RemoveSceneEntity(scene, entity2.entityId);
            TestUtils.RemoveSceneEntity(scene, entity3.entityId);

            Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

            Assert.AreEqual(0, Environment.i.world.sceneBoundsChecker.highPrioEntitiesToCheckCount, "entities to check should be zero!");
        }

        [UnityTest]
        public IEnumerator HighPrioEntitiesAreRegistered_Scale()
        {
            var boxShape1 = TestUtils.CreateEntityWithBoxShape(scene, Vector3.one);
            var boxShape2 = TestUtils.CreateEntityWithBoxShape(scene, Vector3.one);
            var boxShape3 = TestUtils.CreateEntityWithBoxShape(scene, Vector3.one);

            var entity1 = boxShape1.attachedEntities.First();
            TestUtils.SetEntityTransform(scene, entity1, Vector3.one, Quaternion.identity, new Vector3(SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f, 0, 0));
            var entity2 = boxShape2.attachedEntities.First();
            TestUtils.SetEntityTransform(scene, entity2, Vector3.one, Quaternion.identity, new Vector3(0, SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f, 0));
            var entity3 = boxShape3.attachedEntities.First();
            TestUtils.SetEntityTransform(scene, entity3, Vector3.one, Quaternion.identity, new Vector3(0, 0, SceneBoundsChecker.TRIGGER_HIGHPRIO_VALUE * 1.5f));

            Assert.AreEqual(3, Environment.i.world.sceneBoundsChecker.highPrioEntitiesToCheckCount, "entities to check can't be zero!");

            yield return null;

            TestUtils.RemoveSceneEntity(scene, entity1.entityId);
            TestUtils.RemoveSceneEntity(scene, entity2.entityId);
            TestUtils.RemoveSceneEntity(scene, entity3.entityId);

            Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

            Assert.AreEqual(0, Environment.i.world.sceneBoundsChecker.highPrioEntitiesToCheckCount, "entities to check should be zero!");
        }

        [UnityTest]
        public IEnumerator AudioSourcePlaysBackOnReentering()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(2, 1, 2) });
            yield return null;

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.IsTrue(dclAudioSource.enabled);
        }
    }
}