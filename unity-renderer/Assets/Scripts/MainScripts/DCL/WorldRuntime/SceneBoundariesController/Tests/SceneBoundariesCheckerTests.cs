using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Helpers.NFT.Markets;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;
        private UUIDEventsPlugin uuidEventsPlugin;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            uuidEventsPlugin = new UUIDEventsPlugin();
            coreComponentsPlugin = new CoreComponentsPlugin();
            scene = TestUtils.CreateTestScene() as ParcelScene;
            scene.isPersistent = false;
            Environment.i.world.sceneBoundsChecker.timeBetweenChecks = 0f;
            TestUtils_NFT.RegisterMockedNFTShape(Environment.i.world.componentFactory);
            
        }

        protected override IEnumerator TearDown()
        {
            uuidEventsPlugin.Dispose();
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
        }
        
        protected override ServiceLocator InitializeServiceLocator()
        {
            ServiceLocator result = base.InitializeServiceLocator();
            result.Register<IServiceProviders>(
                () =>
                {
                    var mockedProviders = Substitute.For<IServiceProviders>();
                    mockedProviders.theGraph.Returns(Substitute.For<ITheGraph>());
                    mockedProviders.analytics.Returns(Substitute.For<IAnalytics>());
                    mockedProviders.catalyst.Returns(Substitute.For<ICatalyst>());
                    mockedProviders.openSea.Returns(Substitute.For<INFTMarket>());
                    return mockedProviders;
                });
            return result;
        }

        [UnityTest]
        public IEnumerator EntitiesAreBeingCorrectlyRegistered() { yield return SBC_Asserts.EntitiesAreBeingCorrectlyRegistered(scene); }
        
        [UnityTest]
        public IEnumerator EntityIsEvaluatedOnReparenting() { yield return SBC_Asserts.EntityIsEvaluatedOnReparenting(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeWithCollidersAndNoRenderersIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.GLTFShapeWithCollidersAndNoRenderersIsInvalidatedWhenStartingOutOfBounds(scene); }
        
        [UnityTest]
        public IEnumerator GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries() { yield return SBC_Asserts.GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform() { yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(scene); }
        
        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(scene); }
        
        [UnityTest]
        public IEnumerator PShapeIsEvaluatedAfterCorrectTransformAttachment() { yield return SBC_Asserts.PShapeIsEvaluatedAfterCorrectTransformAttachment(scene); }
        
        [UnityTest]
        public IEnumerator GLTFShapeIsEvaluatedAfterCorrectTransformAttachment() { yield return SBC_Asserts.GLTFShapeIsEvaluatedAfterCorrectTransformAttachment(scene); }
        
        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds() { yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBounds() { yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene); }
        
        [UnityTest]
        public IEnumerator OnPointerEventCollidersAreResetWhenReenteringBounds() { yield return SBC_Asserts.OnPointerEventCollidersAreResetWhenReenteringBounds(scene); }

        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
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
            scene.isPersistent = false;

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator AudioSourceWithMeshIsDisabled()
        {
            scene.isPersistent = false;

            TestUtils.CreateEntityWithGLTFShape(scene, new Vector3(8, 1, 8), TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb", out var entity);
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);

            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
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