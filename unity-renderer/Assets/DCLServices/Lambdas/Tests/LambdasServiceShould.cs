using DCL;
using MainScripts.DCL.Helpers.SentryUtils;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Sentry;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace DCLServices.Lambdas.Tests
{
    [TestFixture]
    public class LambdasServiceShould
    {
        [Serializable]
        private class TestClass { }

        [Serializable]
        public class TestResponse
        {
            public int value = 3;
        }

        private const string TEST_URL = "https://peer.decentraland.org/lambd2";
        private const string END_POINT = "testEndpoint";

        private LambdasService lambdasService;
        private ServiceLocator serviceLocator;
        private IWebRequestMonitor transactionMonitor;
        private DisposableTransaction disposableTransaction;
        private ISpan span;

        [SetUp]
        public void Setup()
        {
            serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<ILambdasService>(() => lambdasService = new LambdasService());

            Environment.Setup(serviceLocator);
            var catalyst = serviceLocator.Get<IServiceProviders>().catalyst;
            catalyst.lambdasUrl.Returns(TEST_URL);

            transactionMonitor = serviceLocator.Get<IWebRequestMonitor>();
            disposableTransaction = new DisposableTransaction(span = Substitute.For<ISpan>());
        }

        [Test][Category("ToFix")]
        public void ConstructUrlWithParams([Values(END_POINT, END_POINT + "/", "/" + END_POINT, END_POINT + "?")] string testEndpoint)
        {
            var url = lambdasService.GetUrl(testEndpoint, new[] { ("param1", "34"), ("param2", "value"), ("param3", "foo") });
            Assert.AreEqual($"{lambdasService.GetLambdasUrl()}/{testEndpoint.Trim('/').TrimEnd('?')}?param1=34&param2=value&param3=foo", url);
        }

        [Test]
        public void ConstructUrlWithoutParams([Values(END_POINT, END_POINT + "/", "/" + END_POINT, END_POINT + "?")] string testEndpoint)
        {
            var url = lambdasService.GetUrl(testEndpoint, Array.Empty<(string paramName, string paramValue)>());
            Assert.AreEqual($"{lambdasService.GetLambdasUrl()}/{testEndpoint.Trim('/').TrimEnd('?')}", url);
        }

        [Test]
        public void InvokeTransactionMonitorOnGet()
        {
            lambdasService.Get<TestClass>(END_POINT, END_POINT, timeout: 60, attemptsNumber: 5, cancellationToken: CancellationToken.None, urlEncodedParams: ("param1", "45"));
            transactionMonitor.Received(1).TrackWebRequest(Arg.Any<IWebRequestAsyncOperation>(), END_POINT);
        }

        [Test]
        public void InvokeTransactionMonitorOnPost()
        {
            lambdasService.Post<TestClass, TestResponse>(END_POINT, END_POINT, new TestResponse(), 50, 4, CancellationToken.None, ("param2", "str"));
            transactionMonitor.Received(1).TrackWebRequest(Arg.Any<IWebRequestAsyncOperation>(), END_POINT, data: JsonUtility.ToJson(new TestResponse()));
        }

        [Test]
        public void ParseCorrectResponse()
        {
            Assert.AreEqual(true,
                LambdasService.TryParseResponse(END_POINT, disposableTransaction, "{\"value\":10}", out TestResponse testResponse));

            Assert.AreEqual(10, testResponse.value);
        }

        [Test]
        public void FailWithIncorrectResponse()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual(false,
                LambdasService.TryParseResponse(END_POINT, disposableTransaction, "{incorrectJson}", out TestResponse testResponse));

            Assert.AreEqual(default, testResponse);
        }

        [Test]
        public void SetTransactionStatusOnParseFail()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual(false,
                LambdasService.TryParseResponse(END_POINT, disposableTransaction, "{incorrectJson}", out TestResponse _));

            span.Received(1).Status = SpanStatus.DataLoss;
        }

        [Test]
        public void InvokeGetRequest()
        {
            var webRequestController = serviceLocator.Get<IWebRequestController>();

            lambdasService.Get<TestClass>(END_POINT, END_POINT, timeout: 60, attemptsNumber: 5, cancellationToken: CancellationToken.None, urlEncodedParams: ("param1", "45"));
            webRequestController.Received().Get($"{lambdasService.GetLambdasUrl()}/{END_POINT}?param1=45", requestAttemps: 5, timeout: 60, disposeOnCompleted: false);
        }

        [Test]
        public void InvokePostRequest()
        {
            var webRequestController = serviceLocator.Get<IWebRequestController>();
            lambdasService.Post<TestClass, TestResponse>(END_POINT, END_POINT, new TestResponse(), 50, 4, CancellationToken.None, ("param2", "str"));

            webRequestController.Received().Post($"{lambdasService.GetLambdasUrl()}/{END_POINT}?param2=str", "{\"value\":3}", requestAttemps: 4, timeout: 50, disposeOnCompleted: false);
        }
    }
}
