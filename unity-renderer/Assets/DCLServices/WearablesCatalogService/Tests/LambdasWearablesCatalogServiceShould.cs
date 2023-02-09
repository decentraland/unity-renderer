using DCLServices.Lambdas;
using NSubstitute;
using NUnit.Framework;

namespace DCLServices.WearablesCatalogService
{
    public class LambdasWearablesCatalogServiceShould
    {
        private LambdasWearablesCatalogService service;
        private ILambdasService lambdasService;

        [SetUp]
        public void SetUp()
        {
            lambdasService = Substitute.For<ILambdasService>();

            service = new LambdasWearablesCatalogService(new BaseDictionary<string, WearableItem>(),
                lambdasService);
            service.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            service.Dispose();
        }
    }
}
