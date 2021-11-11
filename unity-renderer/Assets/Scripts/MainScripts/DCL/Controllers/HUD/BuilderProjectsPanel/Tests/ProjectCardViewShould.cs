using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DCL.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ProjectCardViewShould
{
    private ProjectCardView cardView;

    [SetUp]
    public void SetUp()
    {
        const string prefabAssetPath =
            "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/Projects/ProjectCardView.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<ProjectCardView>(prefabAssetPath);
        cardView = UnityEngine.Object.Instantiate(prefab);
    }

    [TearDown]
    public void TearDown() { UnityEngine.Object.Destroy(cardView.gameObject); }

    [Test]
    public void SetupAndDisplayInfoCorrectly()
    {
        //Arrange
        ProjectData data = new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2
        };

        //Act
        ((IProjectCardView)cardView).Setup(data);

        //Assert

        //should show editor buttons
        Assert.IsTrue(cardView.editorButton.gameObject.activeSelf, "Editor button should be active");

        //should show coords and title
        Assert.AreEqual(cardView.projectNameTxt.text, "TesTitle");
        Assert.AreEqual(cardView.projectSizeTxt.text, cardView.GetSizeText(2, 2));
    }

    [Test]
    public void SetNotSyncCorrectly()
    {
        //Arrange
        List<Scene> sceneList = new List<Scene>();

        ((IProjectCardView)cardView).Setup(new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2,
            scene_id = "Incorrect ID"
        });

        //Act
        cardView.SetScenes(sceneList);

        //Assert
        Assert.AreEqual(ProjectCardView.NOT_PUBLISHED,cardView.projectSyncTxt.text);
        Assert.AreEqual(false,cardView.syncImage.enabled);
    }

    [Test]
    [TestCase("1700-01-01")]
    [TestCase("2999-01-09")]
    public void SetSyncCorrectly(string date)
    {
        //Arrange
        DateTime updatedTime = DateTime.ParseExact(date, "yyyy-dd-MM", CultureInfo.InvariantCulture);
        List<Scene> sceneList = new List<Scene>()
        {
            CreateScene()
        };

        ((IProjectCardView)cardView).Setup(new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2,
            updated_at = updatedTime,
            scene_id = sceneList[0].id
        });

        //Act
        cardView.SetScenes(sceneList);

        //Assert
        Assert.AreEqual(sceneList,cardView.scenesDeployedFromProject);
        Assert.AreEqual(cardView.projectSyncTxt.text, ProjectCardView.PUBLISHED_IN);
        Assert.AreEqual(cardView.syncImage.color, updatedTime >= DateTime.Now ? cardView.desyncColor : cardView.syncColor);
    }

    [Test]
    public void InstanciateScenesCardCorrectly()
    {
        //Arrange
        List<Scene> sceneList = new List<Scene>()
        {
            CreateScene()
        };

        ((IProjectCardView)cardView).Setup(new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2,
            updated_at = DateTime.Now,
            scene_id = sceneList[0].id
        });

        bool eventCalled = false;
        cardView.SetScenes(sceneList);
        cardView.OnExpandMenuPressed += () => { eventCalled = true; };

        //Act
        cardView.ExpandButtonPressed();
        
        //Arrange
        Assert.Greater(cardView.sceneCardViews.Count,0);
        Assert.IsTrue(eventCalled);
    }

    [Test]
    public void EditorButtonCalledCorrectly()
    {
        //Arrange
        ((IProjectCardView)cardView).Setup(new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2,
            updated_at = DateTime.Now
        });
        bool eventCalled = false;
        cardView.OnEditorPressed += (x) => { eventCalled = true; };
        
        //Act
        cardView.EditorButtonClicked();
        
        //Assert
        Assert.IsTrue(eventCalled);
    }
    
    private Scene CreateScene()
    {
        var metadata = new CatalystSceneEntityPayload();
        metadata.metadata  = new CatalystSceneEntityMetadata();
        metadata.metadata.scene = new CatalystSceneEntityMetadata.Scene();
        metadata.metadata.scene.@base = "0,0";
        metadata.metadata.scene.parcels = new string[] { "0,0" };
        metadata.metadata.display = new CatalystSceneEntityMetadata.Display();
        metadata.metadata.display.navmapThumbnail = "";
        metadata.metadata.contact = new CatalystSceneEntityMetadata.Contact();
        metadata.metadata.contact.name = "";
        metadata.metadata.policy = new CatalystSceneEntityMetadata.Policy();
        Scene scene = new Scene(metadata, "TestURL");
        scene.parcelsCoord = null;
        scene.timestamp = BIWUtils.ConvertToMilisecondsTimestamp(DateTime.Now);
        return scene;
    }
}
