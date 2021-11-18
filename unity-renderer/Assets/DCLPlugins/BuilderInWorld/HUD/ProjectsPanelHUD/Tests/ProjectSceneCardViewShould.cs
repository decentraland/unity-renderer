using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ProjectSceneCardViewShould 
{
    private ProjectSceneCardView cardView;

    [SetUp]
    public void SetUp()
    {
        const string prefabAssetPath =
            "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/Projects/ProjectSceneCardView.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<ProjectSceneCardView>(prefabAssetPath);
        cardView = UnityEngine.Object.Instantiate(prefab);
    }

    [TearDown]
    public void TearDown() { UnityEngine.Object.Destroy(cardView.gameObject); }

    [Test]
    public void SetupAndDisplayInfoCorrectly()
    {
        //Arrange
        Scene data = CreateScene();

        //Act
        ((IProjectSceneCardView)cardView).Setup(data,true);

        //Assert

        //should show editor buttons
        Assert.IsTrue(cardView.outdatedGameObject.gameObject.activeSelf, "Editor button should be active");

        //should show coords and title
        Assert.AreEqual(cardView.sceneNameTxt.text, "TestTitle");
        Assert.AreEqual(cardView.sceneCoordsTxt.text, "1,1");
    }
    
    private Scene CreateScene()
    {
        var metadata = new CatalystSceneEntityPayload();
        metadata.metadata  = new CatalystSceneEntityMetadata();
        metadata.metadata.scene = new CatalystSceneEntityMetadata.Scene();
        metadata.metadata.scene.@base = "1,1";
        metadata.metadata.scene.parcels = new string[] { "1,1" };
        metadata.metadata.display = new CatalystSceneEntityMetadata.Display();
        metadata.metadata.display.navmapThumbnail = "";
        metadata.metadata.display.title = "TestTitle";
        metadata.metadata.contact = new CatalystSceneEntityMetadata.Contact();
        metadata.metadata.contact.name = "";
        metadata.metadata.policy = new CatalystSceneEntityMetadata.Policy();
        Scene scene = new Scene(metadata, "TestURL");
       
        scene.parcelsCoord = null;
        return scene;
    }
}
