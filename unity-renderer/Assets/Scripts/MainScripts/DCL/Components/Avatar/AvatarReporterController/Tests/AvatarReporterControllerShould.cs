using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class AvatarReporterControllerShould
{
    private IAvatarReporterController reporterController;
    private IWorldState worldState;
    private Dictionary<string, IParcelScene> scenes;

    [SetUp]
    public void SetUp()
    {
        worldState = Substitute.For<IWorldState>();

        reporterController = Substitute.ForPartsOf<AvatarReporterController>(worldState);
        reporterController.reporter = Substitute.For<IReporter>();

        var scene0 = Substitute.For<IParcelScene>();
        scene0.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = "scene0",
            parcels = new[] { Vector2Int.zero }
        });

        scenes = new Dictionary<string, IParcelScene>() { { "scene0", scene0 } };

        worldState.loadedScenes.Returns(scenes);
    }

    [Test]
    public void ReportSceneWhenIsSetup()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void DontReportSceneWhenIsNotSetup()
    {
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void DontReportSceneWhenIsNotFromAvatarGlobalScene()
    {
        reporterController.SetUp("Temptation", "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void DontReportSceneAfterAvatarRemoved()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarRemoved();
        reporterController.reporter.Received(1).ReportAvatarRemoved("0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void DontReportSceneIfAvatarDidntMove()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void ReportSceneWhenMovingFromNotLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", null);
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");
    }

    [Test]
    public void ReportSceneWhenSceneIsLoadedAfterAvatarPosition()
    {
        var position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", null);

        var loadScene = Substitute.For<IParcelScene>();
        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = "sceneLoaded",
            parcels = new[] { new Vector2Int(1, 0) }
        });

        scenes.Add("sceneLoaded", loadScene);

        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "sceneLoaded");
    }

    [Test]
    public void ReportSceneWhenMovingFromLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");

        var loadScene = Substitute.For<IParcelScene>();
        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = "sceneLoaded",
            parcels = new[] { new Vector2Int(1, 0) }
        });

        scenes.Add("sceneLoaded", loadScene);

        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "sceneLoaded");
    }

    [Test]
    public void ReportSceneWhenMovingFromLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", "scene0");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", null);
    }

    [Test]
    public void DontReportSceneWhenMovingFromNotLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "0", "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", null);
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE * 2, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("0", "1", null);
    }
}