using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace DCLServices.Lambdas.LandsService.Tests
{
    public class LandsServiceShould
    {
        private LandsService namesService;
        private ILambdasService lambdasService;

        [SetUp]
        public void SetUp()
        {
            namesService = new LandsService();

            var serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<ILambdasService>(() => lambdasService = Substitute.For<ILambdasService>());

            Environment.Setup(serviceLocator);
        }

        [Test]
        public async Task CallLambdasService()
        {
            const string ADDRESS = "0xddf1eec586d8f8f0eb8c5a3bf51fb99379a55684";
            const int PAGE_SIZE = 30;

            var ct = new CancellationTokenSource().Token;
            var pagePointer = namesService.GetPaginationPointer(ADDRESS, PAGE_SIZE, ct);
            await pagePointer.GetPageAsync(12, CancellationToken.None);

            lambdasService.Received(1)
                          .Get<LandsResponse>(
                               LandsService.END_POINT,
                               LandsService.END_POINT + ADDRESS,
                               LandsService.TIMEOUT,
                               LandsService.ATTEMPTS_NUMBER,
                               ct,
                               ("pageSize", PAGE_SIZE.ToString()), ("pageNum", "12"));
        }
    }
}
