using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DCLServices.Lambdas.Tests
{
    [TestFixture]
    public class LambdaResponsePagePointerShould
    {
        [Serializable]
        public class Foo : PaginatedResponse
        {
            public int field1;
            public string field2;
        }

        private const string END_POINT = "TestEndpoint";
        private const int PAGE_SIZE = 5;

        private LambdaResponsePagePointer<Foo> pointer;
        private ILambdaServiceConsumer<Foo> serviceConsumer;
        private CancellationTokenSource source;

        [SetUp]
        public void Setup()
        {
            serviceConsumer = Substitute.For<ILambdaServiceConsumer<Foo>>();
            source = new CancellationTokenSource();
            pointer = new LambdaResponsePagePointer<Foo>(END_POINT, PAGE_SIZE, source.Token, serviceConsumer);
        }

        [TearDown]
        public void Teardown()
        {
            pointer.Dispose();
            source.Dispose();
        }

        private void MockSuccess()
        {
            serviceConsumer.CreateRequest(END_POINT, PAGE_SIZE, Arg.Any<int>(), Arg.Any<Dictionary<string,string>>(), Arg.Any<CancellationToken>())
                           .Returns(info =>
                            {
                                var ct = info.Arg<CancellationToken>();
                                ct.ThrowIfCancellationRequested();

                                var r = new Foo { field1 = 1, field2 = "str", pageNum = info.ArgAt<int>(2), pageSize = PAGE_SIZE };
                                return UniTask.FromResult((r, true));
                            });
        }

        private void MockFailure()
        {
            serviceConsumer.CreateRequest(END_POINT, PAGE_SIZE, Arg.Any<int>(), Arg.Any<Dictionary<string,string>>(), source.Token)
                           .Returns(UniTask.FromResult<(Foo response, bool success)>((null, false)));
        }

        [Test]
        public async Task InvokeServiceIfNotCached()
        {
            MockSuccess();
            await pointer.GetPageAsync(2, CancellationToken.None);
            serviceConsumer.Received().CreateRequest(END_POINT, PAGE_SIZE, 2, Arg.Any<Dictionary<string,string>>(), source.Token);
        }

        [Test]
        public async Task CacheResultOnSuccess()
        {
            MockSuccess();
            var r = await pointer.GetPageAsync(2, CancellationToken.None);
            Assert.IsTrue(pointer.CachedPages.TryGetValue(2, out var pageResult));
            Assert.AreEqual(r.response, pageResult);
            Assert.IsTrue(r.success);
            Assert.AreEqual(1, pageResult.page.field1);
            Assert.AreEqual("str", pageResult.page.field2);
            Assert.AreEqual(2, pageResult.page.pageNum);
            Assert.AreEqual(PAGE_SIZE, pageResult.page.pageSize);
        }

        [Test]
        public async Task NotCacheResultOnFail()
        {
            MockFailure();
            await pointer.GetPageAsync(3, CancellationToken.None);
            Assert.IsFalse(pointer.CachedPages.ContainsKey(3));
        }

        [Test]
        public async Task RetrieveFromCache()
        {
            MockSuccess();
            await pointer.GetPageAsync(4, CancellationToken.None);
            serviceConsumer.ClearReceivedCalls();
            await pointer.GetPageAsync(4, CancellationToken.None);
            serviceConsumer.DidNotReceive().CreateRequest(END_POINT, PAGE_SIZE, Arg.Any<int>(), Arg.Any<Dictionary<string,string>>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Dispose()
        {
            MockSuccess();
            await pointer.GetPageAsync(3, CancellationToken.None);
            pointer.Dispose();
            Assert.IsTrue(pointer.isDisposed);
        }

        [Test]
        public void DisposeOnCancellation()
        {
            source.Cancel();
            Assert.IsTrue(pointer.isDisposed);
        }

        [Test]
        public void RespectAdditionalCancellationToken()
        {
            MockSuccess();
            var cs = new CancellationTokenSource();
            cs.Cancel();

            // there are problems with `Assert.ThrowsAsync` and `UniTask`, they are not moved forward
            // so I can't use `Delay` or something like this
            Assert.ThrowsAsync<TaskCanceledException>(async () => await pointer.GetPageAsync(3, cs.Token));
        }

        [Test]
        public void PoolDictionary()
        {
            var dict = pointer.CachedPages;

            pointer.Dispose();
            pointer = new LambdaResponsePagePointer<Foo>(END_POINT, PAGE_SIZE, source.Token, serviceConsumer);

            Assert.AreSame(dict, pointer.CachedPages);
        }

        [Test]
        public void ThrowIfDisposed()
        {
            MockSuccess();
            pointer.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(() => pointer.GetPageAsync(2, CancellationToken.None).AsTask());
        }
    }
}
