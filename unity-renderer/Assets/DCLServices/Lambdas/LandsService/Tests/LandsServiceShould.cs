using Cysharp.Threading.Tasks;
using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.TestTools;

namespace DCLServices.Lambdas.LandsService.Tests
{
    [TestFixture]
    public class LandsServiceShould
    {
        private LandsService landsService;
        private ILambdasService lambdasService;

        [SetUp]
        public void SetUp()
        {
            landsService = new LandsService();

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
                (IReadOnlyList<LandsResponse.LandEntry> lands, int totalAmount) lands = await landsService.RequestOwnedLandsAsync(ADDRESS, 12, PAGE_SIZE, true, ct);

                lambdasService.Received(1)
                              .Get<LandsResponse>(
                                   LandsService.END_POINT,
                                   $"{LandsService.END_POINT}{ADDRESS}/lands",
                                   LandsService.TIMEOUT,
                                   LandsService.ATTEMPTS_NUMBER,
                                   ct,
                                   ("pageSize", PAGE_SIZE.ToString()), ("pageNum", "12"));
            });
    }
}
