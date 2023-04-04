using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Tests
{
    [TestFixture]
    public class TextureAssetResolverShould
    {
        private const string URL = "testUrl";

        [Test]
        public async Task ReturnFirstSuccessfulResult()
        {
            var r1 = Substitute.For<ITextureAssetProvider>();
            r1.GetTextureAsync(URL).Returns(x => UniTask.FromResult<Texture2D>(null));

            var r2 = Substitute.For<ITextureAssetProvider>();
            r2.GetTextureAsync(URL).Returns(x => UniTask.FromResult(Texture2D.blackTexture));

            var resolver = new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.WEB, r2 }
            }, DataStore.i.featureFlags);

            var res = await resolver.GetTextureAsync(AssetSource.ALL, URL, 1000, cancellationToken: CancellationToken.None);

            r1.Received(1).GetTextureAsync(URL);
            r2.Received(1).GetTextureAsync(URL);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(Texture2D.blackTexture, res.GetSuccessResponse().Texture);
        }

        [Test][Category("Flaky")]
        public async Task ThrowLastIfAllSourcesFailed()
        {
            var r1 = Substitute.For<ITextureAssetProvider>();
            r1.GetTextureAsync(URL).Returns(x => throw new Exception("1"));

            var e2 = new Exception("2");
            var r2 = Substitute.For<ITextureAssetProvider>();
            r2.GetTextureAsync(URL).Returns(x => throw e2);

            var resolver = new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.WEB, r2 }
            }, DataStore.i.featureFlags);

            var res = await resolver.GetTextureAsync(AssetSource.ALL, URL, 1000, cancellationToken: CancellationToken.None);
            Assert.IsFalse(res.IsSuccess);

            Assert.AreSame(e2, res.GetFailResponse().Exception);
        }

        [Test]
        public async Task SkipResolverIfNotPermitted()
        {
            var r1 = Substitute.For<ITextureAssetProvider>();
            r1.GetTextureAsync(URL).Returns(x => UniTask.FromResult(Texture2D.whiteTexture));

            var r2 = Substitute.For<ITextureAssetProvider>();
            r2.GetTextureAsync(URL).Returns(x => UniTask.FromResult(Texture2D.blackTexture));

            var resolver = new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.WEB, r2 }
            }, DataStore.i.featureFlags);

            var res = await resolver.GetTextureAsync(AssetSource.WEB, URL, 1000, cancellationToken: CancellationToken.None);

            r1.DidNotReceive().GetTextureAsync(URL);
            r2.Received(1).GetTextureAsync(URL);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(Texture2D.blackTexture, res.GetSuccessResponse().Texture);
        }

        [Test]
        public async Task ThrowIfResolversDoNotThrow()
        {
            var r1 = Substitute.For<ITextureAssetProvider>();
            r1.GetTextureAsync(URL).Returns(x => UniTask.FromResult<Texture2D>(null));

            var r2 = Substitute.For<ITextureAssetProvider>();
            r2.GetTextureAsync(URL).Returns(x => UniTask.FromResult<Texture2D>(null));

            var resolver = new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.WEB, r2 }
            }, DataStore.i.featureFlags);

            var res = await resolver.GetTextureAsync(AssetSource.ALL, URL, 1000, cancellationToken: CancellationToken.None);
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(typeof(AssetNotFoundException), res.GetFailResponse().Exception.GetType());
        }
    }
}
