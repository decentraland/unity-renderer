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
    protected override PlatformContext CreatePlatformContext()
    {
        WebRequestController webRequestController = new WebRequestController();
        webRequestController.Initialize(
            genericWebRequest: new WebRequest(),
            assetBundleWebRequest: new WebRequestAssetBundle(),
            textureWebRequest: new WebRequestTexture(),
            null);

        var context = DCL.Tests.PlatformContextFactory.CreateWithCustomMocks
        (
            webRequestController: webRequestController
        );

        return context;
    }

    protected override WorldRuntimeContext CreateRuntimeContext()
    {
        return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
        (
            sceneController: new SceneController(),
            state: new WorldState(),
            componentFactory: new RuntimeComponentFactory(),
            sceneBoundsChecker: new SceneBoundsChecker(new SceneBoundsFeedbackStyle_Simple())
        );
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