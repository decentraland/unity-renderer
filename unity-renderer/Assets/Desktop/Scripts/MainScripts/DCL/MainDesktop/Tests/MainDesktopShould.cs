using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.World.PortableExperiences;
using NSubstitute;
using UnityEngine.TestTools;

namespace Tests
{
    public class MainDesktopShould : IntegrationTestSuite
    {
        private ParcelScene scene;

        protected override void InitializeServices(ServiceLocator serviceLocator)
        {
            serviceLocator.Register<ISceneController>(() => new SceneController(Substitute.For<IConfirmedExperiencesRepository>()));
            serviceLocator.Register<IWorldState>(() => new WorldState());
            serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
        }
    }
}
