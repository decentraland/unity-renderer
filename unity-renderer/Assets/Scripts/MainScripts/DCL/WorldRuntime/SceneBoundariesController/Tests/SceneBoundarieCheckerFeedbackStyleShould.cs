using DCL;
using DCL.Controllers;
using DCL.World.PortableExperiences;
using NSubstitute;
using NUnit.Framework;
using Tests;

public class SceneBoundarieCheckerFeedbackStyleShould : IntegrationTestSuite
{
    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        serviceLocator.Register<ISceneController>(() => new SceneController(Substitute.For<IConfirmedExperiencesRepository>()));
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker(new SceneBoundsFeedbackStyle_Simple()));
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
    }

    [Test]
    public void ChangeFeedbackStyleChange()
    {
        //Arrange
        var redFlickerStyle = new SceneBoundsFeedbackStyle_RedBox();

        //Act
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(redFlickerStyle);

        //Assert
        Assert.AreSame( Environment.i.world.sceneBoundsChecker.GetFeedbackStyle(), redFlickerStyle );
    }
}
