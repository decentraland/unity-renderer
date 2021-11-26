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
        serviceLocator.Set<IWebRequestController>(WebRequestController.Create());
        serviceLocator.Set<ISceneController>(new SceneController());
        serviceLocator.Set<IWorldState>(new WorldState());
        serviceLocator.Set<ISceneBoundsChecker>(new SceneBoundsChecker(new SceneBoundsFeedbackStyle_Simple()));
        serviceLocator.Set<IRuntimeComponentFactory>(new RuntimeComponentFactory());
    }

    [Test]
    public void ChangeFeedbackStyleChange()
    {
        //Arrange
        var redFlickerStyle = new SceneBoundsFeedbackStyle_RedFlicker();

        //Act
        Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(redFlickerStyle);

        //Assert
        Assert.AreSame( Environment.i.world.sceneBoundsChecker.GetFeedbackStyle(), redFlickerStyle );
    }
}