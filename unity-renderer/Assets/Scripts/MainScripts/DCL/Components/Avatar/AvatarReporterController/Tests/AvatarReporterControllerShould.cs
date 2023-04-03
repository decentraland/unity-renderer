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
    private Dictionary<int, IParcelScene> scenes;

    [SetUp]
    public void SetUp()
    {
        worldState = Substitute.For<IWorldState>();

        reporterController = Substitute.ForPartsOf<AvatarReporterController>(worldState);
        reporterController.reporter = Substitute.For<IReporter>();

        var scene0 = Substitute.For<IParcelScene>();
        scene0.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            sceneNumber = 1,
            // id = "scene0",
            parcels = new[] { Vector2Int.zero }
        });

        scenes = new Dictionary<int, IParcelScene>() { { 1, scene0 } };

        worldState.GetLoadedScenes().Returns(scenes);
        worldState.GetSceneNumberByCoords(Vector2Int.zero).Returns(1);
        worldState.GetSceneNumberByCoords(Arg.Is<Vector2Int>(v => v != Vector2Int.zero)).Returns(default(int));
    }

    [Test]
    public void ReportSceneWhenIsSetup()
    {
        // reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);
    }

    [Test]
    public void DontReportSceneWhenIsNotSetup()
    {
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", 1);
    }

    [Test]
    public void DontReportSceneWhenIsNotFromAvatarGlobalScene()
    {
        reporterController.SetUp(3, "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", 1);
    }

    [Test]
    public void DontReportSceneAfterAvatarRemoved()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.ReportAvatarRemoved();
        reporterController.reporter.Received(1).ReportAvatarRemoved( "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.DidNotReceive().ReportAvatarSceneChange("1", 1);
    }

    [Test]
    public void DontReportSceneIfAvatarDidntMove()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER,  "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);
    }

    [Test][Category("ToFix")]
    public void ReportSceneWhenMovingFromNotLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", -1);
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);
    }

    [Test][Category("ToFix")]
    public void ReportSceneWhenSceneIsLoadedAfterAvatarPosition()
    {
        var position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER,  "1");
        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", -1);

        var loadScene = Substitute.For<IParcelScene>();
        Vector2Int scenePos = new Vector2Int(1, 0);

        int sceneNumber = 100;

        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            sceneNumber = sceneNumber,
            parcels = new[] { scenePos }
        });

        scenes.Add(sceneNumber, loadScene);
        worldState.GetSceneNumberByCoords(scenePos).Returns(sceneNumber);

        reporterController.ReportAvatarPosition(position);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", sceneNumber);
    }

    [Test]
    public void ReportSceneWhenMovingFromLoadedToLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER,  "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);

        var loadScene = Substitute.For<IParcelScene>();
        Vector2Int position = new Vector2Int(1, 0);

        int sceneNumber = 100;

        loadScene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
        {
            sceneNumber = sceneNumber,
            parcels = new[] { position }
        });

        scenes.Add(sceneNumber, loadScene);
        worldState.GetSceneNumberByCoords(position).Returns(sceneNumber);

        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", sceneNumber);
    }

    [Test][Category("ToFix")]
    public void ReportSceneWhenMovingFromLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.ReportAvatarPosition(Vector3.one);
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", 1);
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", -1);
    }

    [Test][Category("ToFix")]
    public void DontReportSceneWhenMovingFromNotLoadedToNotLoadedScene()
    {
        reporterController.SetUp(EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER, "1");
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange("1", -1);
        reporterController.ReportAvatarPosition(new Vector3(ParcelSettings.PARCEL_SIZE * 2, 0, 0));
        reporterController.reporter.Received(1).ReportAvatarSceneChange( "1", -1);
    }
}
