using System.Collections;
using System.Collections.Generic;
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
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/ProjectCardView.prefab";
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
            Assert.AreEqual(cardView.projectSizeTxt.text, cardView.GetSizeText(2,2));
        }
        
        [Test]
        public void SetScenesCorrectly()
        {
            //Arrange
            List<Scene> sceneList = new List<Scene>()
            {
                CreateScene(),
                CreateScene()
            };
            
            ((IProjectCardView)cardView).Setup(new ProjectData()
            {
                id = "",
                title = "TesTitle",
                rows = 2,
                cols = 2
            });
            
            //Act
            cardView.SetScenes(sceneList);
        
            //Assert
            
        }
    
        //
        // [Test]
        // public void DisplayCorrectlyWhenSceneIsNotEditable()
        // {
        //     ((IProjectCardView)cardView).Setup(new ProjectData()
        //     {
        //         id = "",
        //         isDeployed = false,
        //         name = "test",
        //         coords = Vector2Int.zero,
        //         size = Vector2Int.zero,
        //         isContributor = true,
        //         isEditable = false
        //     });
        //
        //     Assert.IsFalse(cardView.editorButton.gameObject.activeSelf, "Editor button should not be active");
        //     Assert.IsTrue(cardView.editorLockedGO.activeSelf, "Editor locked indicator should be active");
        // }
        //
        // [Test]
        // public void DisplayCorrectlyWhenSceneIsEditable()
        // {
        //     ((IProjectCardView)cardView).Setup(new ProjectData()
        //     {
        //         id = "",
        //         isDeployed = false,
        //         name = "test",
        //         coords = Vector2Int.zero,
        //         size = Vector2Int.zero,
        //         isContributor = true,
        //         isEditable = true
        //     });
        //
        //     Assert.IsTrue(cardView.editorButton.gameObject.activeSelf, "Editor button should be active");
        //     Assert.IsFalse(cardView.editorLockedGO.activeSelf, "Editor locked indicator should not be active");
        // }

    private Scene CreateScene()
    {
        var metadata = new CatalystSceneEntityPayload();
        metadata.metadata  = new CatalystSceneEntityMetadata();
        metadata.metadata.scene = new CatalystSceneEntityMetadata.Scene();
        metadata.metadata.scene.@base = "0,0";
        metadata.metadata.scene.parcels = new string[] { "0,0" };
        metadata.metadata.display = new CatalystSceneEntityMetadata.Display();
        metadata.metadata.display.navmapThumbnail = "TestURl";
        metadata.metadata.contact = new CatalystSceneEntityMetadata.Contact();
        metadata.metadata.contact.name = "";
        metadata.metadata.policy = new CatalystSceneEntityMetadata.Policy();
        Scene scene = new Scene(metadata, "TestURL");
        scene.parcelsCoord = new [] { new Vector2Int(1, 1) };
        return scene;
    }
}
