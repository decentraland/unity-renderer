using System;
using System.Collections;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCL.Emotes
{
    public class EmoteAnimationLoaderShould
    {
        private EmoteAnimationLoader loader;
        private IWearableRetriever retriever;
        private GameObject container;

        [SetUp]
        public void SetUp()
        {
            container = new GameObject("_Container");
            retriever = Substitute.For<IWearableRetriever>();
            loader = new EmoteAnimationLoader(retriever);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(container); }

        [Test]
        public void ThrowIfNullContainer() { TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(null, new WearableItem(), "female")); }

        [Test]
        public void ThrowIfNullEmote() { TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, null, "female")); }

        [Test]
        public void ThrowIfNullBodyShape() { TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), null)); }

        [Test]
        public void ThrowIfEmptyBodyShape() { TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), "")); }

        [Test]
        public void ThrowIfNoRepresentationForBodyShape() { TestUtils.ThrowsAsync<Exception>(loader.LoadEmote(container, new WearableItem { id = "emote0" }, "female"), $"No representation for female of emote: emote0"); }

        [Test]
        public void ThrowIfCancelledTokenProvided()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), "female", cts.Token));
        }

        [UnityTest]
        public IEnumerator ProvideTheRetrieverAnimation() => UniTask.ToCoroutine(async () =>
        {
            Animation animation = container.AddComponent<Animation>();
            AnimationClip clip = Resources.Load<AnimationClip>("tik");
            animation.clip = clip;
            Rendereable rendereable = new Rendereable { container = container, };
            retriever.Retrieve(
                         Arg.Any<GameObject>(),
                         Arg.Any<ContentProvider>(),
                         Arg.Any<string>(),
                         Arg.Any<string>(),
                         Arg.Any<CancellationToken>())
                     .Returns(new UniTask<Rendereable>(rendereable));
            retriever.rendereable.Returns(rendereable);

            await loader.LoadEmote(container, new WearableItem
                {
                    id = "emote",
                    data = new WearableItem.Data
                    {
                        representations = new []
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new [] { "female" },
                                contents = new [] { new WearableItem.MappingPair { key = "key", hash = "hash" } }
                            }
                        }
                    }
                },
                "female");

            Assert.AreEqual(clip, loader.animation);
        });

    }
}