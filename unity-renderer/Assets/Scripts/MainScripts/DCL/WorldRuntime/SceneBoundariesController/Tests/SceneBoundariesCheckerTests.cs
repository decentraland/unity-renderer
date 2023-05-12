using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Helpers.NFT.Markets;
using NSubstitute;
using System.Threading.Tasks;
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

        [Test]
        public async Task EntitiesAreBeingCorrectlyRegistered() { await SBC_Asserts.EntitiesAreBeingCorrectlyRegistered(scene); }

        [Test]
        public async Task EntityIsEvaluatedOnReparenting() { await SBC_Asserts.EntityIsEvaluatedOnReparenting(scene); }

        [Test]
        public async Task PShapeIsInvalidatedWhenStartingOutOfBounds() { await SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [Test]
        public async Task GLTFShapeIsInvalidatedWhenStartingOutOfBounds() { await SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [Test]
        public async Task GLTFShapeWithCollidersAndNoRenderersIsInvalidatedWhenStartingOutOfBounds() { await SBC_Asserts.GLTFShapeWithCollidersAndNoRenderersIsInvalidatedWhenStartingOutOfBounds(scene); }

        [Test]
        public async Task GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries() { await SBC_Asserts.GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries(scene); }

        [Test]
        public async Task PShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform() { await SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(scene); }

        [Test]
        public async Task GLTFShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform() { await SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBoundsWithoutTransform(scene); }

        [Test]
        public async Task PShapeIsEvaluatedAfterCorrectTransformAttachment() { await SBC_Asserts.PShapeIsEvaluatedAfterCorrectTransformAttachment(scene); }

        [Test]
        public async Task GLTFShapeIsEvaluatedAfterCorrectTransformAttachment() { await SBC_Asserts.GLTFShapeIsEvaluatedAfterCorrectTransformAttachment(scene); }

        [Test]
        public async Task PShapeIsInvalidatedWhenLeavingBounds() { await SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene); }

        [Test]
        public async Task GLTFShapeIsInvalidatedWhenLeavingBounds() { await SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene); }

        [Test]
        public async Task PShapeIsResetWhenReenteringBounds() { await SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene); }

        [Test]
        public async Task OnPointerEventCollidersAreResetWhenReenteringBounds() { await SBC_Asserts.OnPointerEventCollidersAreResetWhenReenteringBounds(scene); }

        [Test]
        public async Task NFTShapeIsInvalidatedWhenStartingOutOfBounds() { await SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [Test]
        public async Task NFTShapeIsInvalidatedWhenLeavingBounds() { await SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene); }

        [Test]
        [Explicit]
        [Category("Explicit")]
        public async Task NFTShapeIsResetWhenReenteringBounds() { await SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene); }

        [Test]
        public async Task GLTFShapeIsResetWhenReenteringBounds() { await SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene); }

        [Test]
        public async Task ChildShapeIsEvaluated() { await SBC_Asserts.ChildShapeIsEvaluated(scene); }

        [Test]
        public async Task ChildShapeIsEvaluatedOnShapelessParent() { await SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene); }

        [Test]
        public async Task HeightIsEvaluated() { await SBC_Asserts.HeightIsEvaluated(scene); }

        [Test]
        public async Task AudioSourceIsMuted()
        {
            var entity = TestUtils.CreateSceneEntity(scene);
            scene.isPersistent = false;

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            await TestUtils.CreateAudioSourceWithClipForEntity(entity).ToUniTask();

            await UniTask.Yield();
            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
        }

        [Test]
        public async Task AudioSourceWithMeshIsDisabled()
        {
            scene.isPersistent = false;

            TestUtils.CreateEntityWithGLTFShape(scene, new Vector3(8, 1, 8), TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb", out var entity);
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            await new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            await TestUtils.CreateAudioSourceWithClipForEntity(entity);

            await UniTask.Yield();
            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.AreEqual(0, dclAudioSource.volume);
        }

        [Test]
        public async Task AudioSourcePlaysBackOnReentering()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(-28, 1, 8) });
            await UniTask.WaitForFixedUpdate();

            await TestUtils.CreateAudioSourceWithClipForEntity(entity);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(2, 1, 2) });

            await UniTask.Yield();
            AudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<AudioSource>();
            Assert.IsTrue(dclAudioSource.enabled);
        }
    }
}
