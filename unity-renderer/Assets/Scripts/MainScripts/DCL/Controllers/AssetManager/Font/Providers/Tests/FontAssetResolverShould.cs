using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Font;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DCL.Tests
{
    [TestFixture]
    public class FontAssetResolverShould
    {
        private const string URL = "testUrl";
        private readonly TMP_FontAsset testFontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();

        [Test]
        public async Task ReturnFirstSuccessfulResult()
        {
            IFontAssetProvider r1 = Substitute.For<IFontAssetProvider>();
            r1.GetFontAsync(URL).Returns(x => UniTask.FromResult<TMP_FontAsset>(null));

            IFontAssetProvider r2 = Substitute.For<IFontAssetProvider>();
            r2.GetFontAsync(URL).Returns(x => UniTask.FromResult(testFontAsset));

            var resolver = new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.ADDRESSABLE, r2 },
            }, DataStore.i.featureFlags);

            FontResponse res = await resolver.GetFontAsync(AssetSource.ALL, URL, CancellationToken.None);

            r1.Received(1).GetFontAsync(URL);
            r2.Received(1).GetFontAsync(URL);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(testFontAsset, res.GetSuccessResponse().Font);
        }

        [Test]
        public async Task ThrowLastIfAllSourcesFailed()
        {
            IFontAssetProvider r1 = Substitute.For<IFontAssetProvider>();
            r1.GetFontAsync(URL).Returns(x => throw new Exception("1"));

            var e2 = new Exception("2");
            IFontAssetProvider r2 = Substitute.For<IFontAssetProvider>();
            r2.GetFontAsync(URL).Returns(x => throw e2);

            var resolver = new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.ADDRESSABLE, r2 },
            }, DataStore.i.featureFlags);

            FontResponse res = await resolver.GetFontAsync(AssetSource.ALL, URL, CancellationToken.None);
            Assert.IsFalse(res.IsSuccess);

            Assert.AreSame(e2, res.GetFailResponse().Exception);
        }

        [Test]
        public async Task SkipResolverIfNotPermitted()
        {
            IFontAssetProvider r1 = Substitute.For<IFontAssetProvider>();
            r1.GetFontAsync(URL).Returns(x => UniTask.FromResult(testFontAsset));

            IFontAssetProvider r2 = Substitute.For<IFontAssetProvider>();
            r2.GetFontAsync(URL).Returns(x => UniTask.FromResult(testFontAsset));

            var resolver = new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.ADDRESSABLE, r2 },
            }, DataStore.i.featureFlags);

            FontResponse res = await resolver.GetFontAsync(AssetSource.ADDRESSABLE, URL, CancellationToken.None);

            r1.DidNotReceive().GetFontAsync(URL);
            r2.Received(1).GetFontAsync(URL);

            Assert.IsTrue(res.IsSuccess);
            Assert.AreEqual(testFontAsset, res.GetSuccessResponse().Font);
        }

        [Test]
        public async Task ThrowIfResolversDoNotThrow()
        {
            IFontAssetProvider r1 = Substitute.For<IFontAssetProvider>();
            r1.GetFontAsync(URL).Returns(x => UniTask.FromResult<TMP_FontAsset>(null));

            IFontAssetProvider r2 = Substitute.For<IFontAssetProvider>();
            r2.GetFontAsync(URL).Returns(x => UniTask.FromResult<TMP_FontAsset>(null));

            var resolver = new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                { AssetSource.EMBEDDED, r1 },
                { AssetSource.ADDRESSABLE, r2 },
            }, DataStore.i.featureFlags);

            FontResponse res = await resolver.GetFontAsync(AssetSource.ALL, URL, CancellationToken.None);
            Assert.IsFalse(res.IsSuccess);
            Assert.AreEqual(typeof(AssetNotFoundException), res.GetFailResponse().Exception.GetType());
        }
    }
}
