using DCL;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
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

        [SetUp]
        public void Setup()
        {
            serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<ILambdasService>(() => lambdasService = new LambdasService());

            Environment.Setup(serviceLocator);
            var catalyst = serviceLocator.Get<IServiceProviders>().catalyst;
            catalyst.Lambdas2Url.Returns(TEST_URL);
        }

        [Test]
        public void ConstructUrlWithParams([Values(END_POINT, END_POINT + "/", "/" + END_POINT, END_POINT + "?")] string testEndpoint)
        {
            var url = lambdasService.GetUrl(testEndpoint, new[] { ("param1", "34"), ("param2", "value"), ("param3", "foo") });
            Assert.AreEqual($"{TEST_URL}/{testEndpoint.Trim('/').TrimEnd('?')}?param1=34&param2=value&param3=foo", url);
        }

        [Test]
        public void ConstructUrlWithoutParams([Values(END_POINT, END_POINT + "/", "/" + END_POINT, END_POINT + "?")] string testEndpoint)
        {
            var url = lambdasService.GetUrl(testEndpoint, Array.Empty<(string paramName, string paramValue)>());
            Assert.AreEqual($"{TEST_URL}/{testEndpoint.Trim('/').TrimEnd('?')}", url);
        }

        [Test]
        public void ParseCorrectResponse()
        {
            Assert.AreEqual(true,
                LambdasService.TryParseResponse(END_POINT, Array.Empty<(string paramName, string paramValue)>(),
                    "{\"value\":10}", out TestResponse testResponse));

            Assert.AreEqual(10, testResponse.value);
        }

        [Test]
        public void FailWithIncorrectResponse()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual(false,
                LambdasService.TryParseResponse(END_POINT, Array.Empty<(string paramName, string paramValue)>(),
                    "{incorrectJson}", out TestResponse testResponse));

            Assert.AreEqual(default, testResponse);
        }

        [Test]
        public void InvokeGetRequest()
        {
            var webRequestController = serviceLocator.Get<IWebRequestController>();

            lambdasService.Get<TestClass>(END_POINT, 60, 5, CancellationToken.None, ("param1", "45"));
            webRequestController.Received().Get($"{TEST_URL}/{END_POINT}?param1=45", requestAttemps: 5, timeout: 60);
        }

        [Test]
        public void InvokePostRequest()
        {
            var webRequestController = serviceLocator.Get<IWebRequestController>();
            lambdasService.Post<TestClass, TestResponse>(END_POINT, new TestResponse(), 50, 4, CancellationToken.None, ("param2", "str"));

            webRequestController.Received().Post($"{TEST_URL}/{END_POINT}?param2=str", "{\"value\":3}", requestAttemps: 4, timeout: 50);
        }
    }
}
