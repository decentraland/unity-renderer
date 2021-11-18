using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectControllerShould
{
    private ProjectsController controller;
    
    [SetUp]
    public void SetUp()
    {
        const string prefabAssetPath =
            "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/Projects/ProjectCardView.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<ProjectCardView>(prefabAssetPath);
        controller = new ProjectsController(prefab);
    }

    [TearDown]
    public void TearDown() { controller.Dispose();}
    
    [Test]
    public void SetProjectsCorrectly()
    {
        //Arrange
        ProjectData data = new ProjectData()
        {
            id = "",
            title = "TesTitle",
            rows = 2,
            cols = 2
        };
        bool eventCalled = false;
        controller.OnProjectsSet += (x) => { eventCalled = true; };
        
        //Act
        controller.SetProjects(new []{data});
        
        //Assert
        Assert.IsTrue(eventCalled);
    }

    [Test]
    public void UpdateDelpoymentStatus()
    {
        //Arrange
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[]{});
        var cardView = Substitute.For<IProjectCardView>();
        controller.projects.Add("id",cardView);
        
        //Act
        controller.UpdateDeploymentStatus();
        
        //Assert
        cardView.Received().SetScenes(Arg.Any<List<Scene>>());
    }
    
    [Test]
    public void ExpandCalledCorrectly()
    {
        //Arrange
        bool eventCalled = false;
        controller.OnExpandMenuPressed += () => { eventCalled = true; };
        
        //Act
        controller.ExpandMenuPressed();
        
        //Assert
        Assert.IsTrue(eventCalled);
    }
}
