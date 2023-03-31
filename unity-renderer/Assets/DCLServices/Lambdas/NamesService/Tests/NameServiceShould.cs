using Cysharp.Threading.Tasks;
using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.TestTools;

namespace DCLServices.Lambdas.NamesService.Tests
{
    [TestFixture]
    public class NameServiceShould
    {
        private NamesService namesService;
        private ILambdasService lambdasService;

        [SetUp]
        public void SetUp()
        {
            namesService = new NamesService();

            var serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<ILambdasService>(() => lambdasService = Substitute.For<ILambdasService>());

            Environment.Setup(serviceLocator);
        }

        [UnityTest][Category("ToFix")]
        public IEnumerator CallLambdasService() =>
            UniTask.ToCoroutine(async () =>
            {
                const string ADDRESS = "0xddf1eec586d8f8f0eb8c5a3bf51fb99379a55684";
                const int PAGE_SIZE = 30;

                var ct = new CancellationTokenSource().Token;
                (IReadOnlyList<NamesResponse.NameEntry> names, int totalAmount) names = await namesService.RequestOwnedNamesAsync(ADDRESS, 3, PAGE_SIZE, true, ct);

                lambdasService.Received(1)
                              .Get<NamesResponse>(
                                   NamesService.END_POINT,
                                   $"{NamesService.END_POINT}{ADDRESS}/names",
                                   NamesService.TIMEOUT,
                                   NamesService.ATTEMPTS_NUMBER,
                                   ct,
                                   ("pageSize", PAGE_SIZE.ToString()), ("pageNum", "3"));
            });
    }
}
