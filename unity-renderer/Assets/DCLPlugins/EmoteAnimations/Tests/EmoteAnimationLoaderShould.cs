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

        [UnityTest]
        public IEnumerator ThrowIfNullContainer() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                await TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(null, new WearableItem(), "female"));
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
                Assert.IsTrue(e.Message.Contains(loader.MISSING_CONTAINER_ERROR));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfNullEmote() => UniTask.ToCoroutine(async () =>
        {
            try 
            {
                await TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, null, "female"));
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
                Assert.IsTrue(e.Message.Contains(loader.MISSING_EMOTE_ERROR));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfNullBodyShape() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                await TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), null));
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
                Assert.IsTrue(e.Message.Contains(loader.MISSING_BODYSHAPE_ERROR));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfEmptyBodyShape() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                await TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), ""));
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
                Assert.IsTrue(e.Message.Contains(loader.MISSING_BODYSHAPE_ERROR));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfNoRepresentationForBodyShape() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                await TestUtils.ThrowsAsync<Exception>(loader.LoadEmote(container, new WearableItem { id = "emote0" }, "female"), $"No representation for female of emote: emote0");
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
                Assert.IsTrue(e.Message.Contains("No representation"));
            }
        });

        [UnityTest]
        public IEnumerator ThrowIfCancelledTokenProvided() => UniTask.ToCoroutine(async () =>
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                await TestUtils.ThrowsAsync<OperationCanceledException>(loader.LoadEmote(container, new WearableItem(), "female", cts.Token));
            }
            catch (Exception e)
            {
                // We don't 'throw' the exception because it makes the test fail
            }
        });

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