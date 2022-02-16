using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneBoundarieCheckerFeedbackStyleShould : IntegrationTestSuite
{
    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        serviceLocator.Register<ISceneController>(() => new SceneController());
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