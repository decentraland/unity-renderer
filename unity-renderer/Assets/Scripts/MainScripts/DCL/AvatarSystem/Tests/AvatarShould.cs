using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using GPUSkinning;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Avatar = AvatarSystem.Avatar;
using Object = UnityEngine.Object;

namespace Test.AvatarSystem
{
    public class AvatarShould
    {
        private GameObject container;

        private Avatar avatar;
        private IAvatarCurator curator;
        private ILoader loader;
        private IAnimator animator;
        private IVisibility visibility;
        private ILOD lod;
        private IGPUSkinning gpuSkinning;
        private IGPUSkinningThrottlerService gpuSkinningThrottlerService;
        private IEmoteAnimationEquipper emoteAnimationEquipper;

        [SetUp]
        public void SetUp()
        {
            container = new GameObject();

            curator = Substitute.For<IAvatarCurator>();
            loader = Substitute.For<ILoader>();
            animator = Substitute.For<IAnimator>();
            visibility = Substitute.For<IVisibility>();
            lod = Substitute.For<ILOD>();
            gpuSkinning = Substitute.For<IGPUSkinning>();
            gpuSkinningThrottlerService = Substitute.For<IGPUSkinningThrottlerService>();
            emoteAnimationEquipper = Substitute.For<IEmoteAnimationEquipper>();

            avatar = new Avatar(
                curator,
                loader,
                animator,
                visibility,
                lod,
                gpuSkinning,
                gpuSkinningThrottlerService,
                emoteAnimationEquipper
            );
        }

        [UnityTest]
        public IEnumerator ThrowIfLoadWithCancelledToken() =>
            UniTask.ToCoroutine(async () =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();

                await TestUtils.ThrowsAsync<OperationCanceledException>(avatar.Load(new List<string>(), new List<string>(), new AvatarSettings(), cts.Token));
            });

        [UnityTest]
        public IEnumerator ThrowIfCuratorFails() =>
            UniTask.ToCoroutine(async () =>
            {
                var settings = new AvatarSettings();

                curator.Configure()
                       .Curate(Arg.Any<AvatarSettings>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                       .Returns(x => throw new Exception("Curator failed"));

                var wearableIds = new List<string>();
                var emoteIds = new List<string>();

                await TestUtils.ThrowsAsync<Exception>(avatar.Load(wearableIds, emoteIds, settings));
                visibility.DidNotReceive().RemoveGlobalConstrain(Avatar.LOADING_VISIBILITY_CONSTRAIN);
                curator.Received().Curate(settings, wearableIds, emoteIds, Arg.Any<CancellationToken>());

                loader.DidNotReceiveWithAnyArgs()
                      .Load(default, default, default);
            });

        [UnityTest]
        public IEnumerator ThrowIfLoaderFails() =>
            UniTask.ToCoroutine(async () =>
            {
                var settings = new AvatarSettings();

                var bodyWearables = new BodyWearables(
                    new WearableItem(),
                    new WearableItem(),
                    new WearableItem(),
                    new WearableItem()
                );

                List<WearableItem> wearables = new List<WearableItem>();
                List<WearableItem> emotes = new List<WearableItem>();

                curator.Configure()
                       .Curate(Arg.Any<AvatarSettings>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                       .Returns(_ => new UniTask<(BodyWearables bodyWearables, List<WearableItem> wearables, List<WearableItem> emotes)>((bodyWearables, wearables, emotes)));

                loader.Configure()
                      .Load(Arg.Any<BodyWearables>(),
                           Arg.Any<List<WearableItem>>(),
                           Arg.Any<AvatarSettings>(),
                           Arg.Any<SkinnedMeshRenderer>(),
                           Arg.Any<CancellationToken>())
                      .Returns(x => throw new Exception("Loader failed"));

                await TestUtils.ThrowsAsync<Exception>(avatar.Load(new List<string>(), new List<string>(), settings));

                loader.Received()
                      .Load(bodyWearables, wearables, settings, cancellationToken: Arg.Any<CancellationToken>());
            });

        [UnityTest]
        public IEnumerator FinishSuccessfully() =>
            UniTask.ToCoroutine(async () =>
            {
                var settings = new AvatarSettings { bodyshapeId = "bodyshapeId" };
                SkinnedMeshRenderer combinedRenderer = CreatePrimitiveWithSkinnedMeshRenderer(container.transform).GetComponent<SkinnedMeshRenderer>();
                List<Renderer> facialFeatures = new List<Renderer> { CreatePrimitiveWithSkinnedMeshRenderer(container.transform).GetComponent<SkinnedMeshRenderer>() };
                Renderer gpuSkinnedRenderer = CreatePrimitive(container.transform).GetComponent<Renderer>();
                loader.combinedRenderer.Returns(combinedRenderer);
                loader.facialFeaturesRenderers.Returns(facialFeatures);
                loader.bodyshapeContainer.Returns(combinedRenderer.gameObject);
                gpuSkinning.renderer.Returns(gpuSkinnedRenderer);
                Vector3 extents = loader.combinedRenderer.localBounds.extents * 2f / 100f;

                await avatar.Load(new List<string>(), new List<string>(), settings);

                Assert.AreEqual(extents, avatar.extents);
                animator.Received().Prepare(settings.bodyshapeId, combinedRenderer.gameObject);
                gpuSkinning.Received().Prepare(combinedRenderer);
                gpuSkinningThrottlerService.Received().Register(gpuSkinning);
                visibility.Received().Bind(gpuSkinnedRenderer, facialFeatures);
                visibility.Received().AddGlobalConstrain(Avatar.LOADING_VISIBILITY_CONSTRAIN);
                visibility.Received().RemoveGlobalConstrain(Avatar.LOADING_VISIBILITY_CONSTRAIN);
                lod.Received().Bind(gpuSkinnedRenderer);
                Assert.AreEqual(IAvatar.Status.Loaded, avatar.status);
            });

        private GameObject CreatePrimitive(Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (primitive.TryGetComponent(out Collider collider))
                Utils.SafeDestroy(collider);

            primitive.transform.parent = parent;

            return primitive;
        }

        private GameObject CreatePrimitiveWithSkinnedMeshRenderer(Transform parent)
        {
            GameObject primitive = CreatePrimitive(parent);
            Renderer renderer = primitive.GetComponent<Renderer>();
            SkinnedMeshRenderer skr = primitive.AddComponent<SkinnedMeshRenderer>();
            skr.sharedMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            Utils.SafeDestroy(renderer);

            return primitive;
        }

        [TearDown]
        public void TearDown()
        {
            Utils.SafeDestroy(container);
        }
    }
}
