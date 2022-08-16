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
    private Dictionary<Vector2Int, string> sceneByCoords;

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
        sceneByCoords = new Dictionary<Vector2Int, string> { { Vector2Int.zero, "scene0" } };

        worldState.loadedScenes.Returns(scenes);
        worldState.loadedScenesByCoordinate.Returns(sceneByCoords);
    }

    [Test]
    public void ReportSceneWhenIsSetup()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void DontReportSceneWhenIsNotSetup()
    {
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void DontReportSceneWhenIsNotFromAvatarGlobalScene()
    {
        reporterController.SetUp("Temptation", "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void DontReportSceneAfterAvatarRemoved()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "1");
        reporterController.ReportAvatarRemoved();
        reporterController.reporter.Received(1).ReportAvatarRemoved( "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void DontReportSceneIfAvatarDidntMove()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID,  "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void ReportSceneWhenMovingFromNotLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", null);
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");
    }

    [Test]
    public void ReportSceneWhenSceneIsLoadedAfterAvatarPosition()
    {
        var position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID,  "1");
        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", null);

        var loadScene = Substitute.For<IParcelScene>();
        Vector2Int scenePos = new Vector2Int(1, 0);

        string sceneId = "sceneLoaded";

        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = sceneId,
            parcels = new[] { scenePos }
        });

        scenes.Add(sceneId, loadScene);
        sceneByCoords.Add(scenePos, sceneId);

        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", sceneId);
    }

    [Test]
    public void ReportSceneWhenMovingFromLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID,  "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");

        var loadScene = Substitute.For<IParcelScene>();
        Vector2Int position = new Vector2Int(1, 0);

        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = "sceneLoaded",
            parcels = new[] { position }
        });

        scenes.Add("sceneLoaded", loadScene);
        sceneByCoords.Add(position, "sceneLoaded");

        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "sceneLoaded");
    }

    [Test]
    public void ReportSceneWhenMovingFromLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", "scene0");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", null);
    }

    [Test]
    public void DontReportSceneWhenMovingFromNotLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID, "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", null);
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE * 2, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange( "1", null);
    }
}