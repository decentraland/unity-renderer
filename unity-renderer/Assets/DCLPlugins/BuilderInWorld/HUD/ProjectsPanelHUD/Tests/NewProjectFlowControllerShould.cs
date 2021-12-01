using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class NewProjectFlowControllerShould
{
    private NewProjectFlowController newProjectFlowController;
    
    [SetUp]
    public void SetUp()
    {
        newProjectFlowController = new NewProjectFlowController(Substitute.For<INewProjectFlowView>());
    }

    [TearDown]
    public void TearDown() { newProjectFlowController.Dispose(); }

    [Test]
    public void StartCreateNewProjectCorrectly()
    {
        //Act
        newProjectFlowController.NewProject();
        
        //Assert
        Assert.IsFalse(string.IsNullOrEmpty(newProjectFlowController.projectData.id));
        Assert.AreEqual(newProjectFlowController.projectData.eth_address, UserProfile.GetOwnUserProfile().ethAddress);
    }

    [Test]
    public void SetTitleAndDescriptionCorrectly()
    {
        //Arrange
        newProjectFlowController.NewProject();
        
        //Act
        newProjectFlowController.SetTitleAndDescription("Title","Description");
        
        //Assert
        Assert.AreEqual("Title",newProjectFlowController.projectData.title);
        Assert.AreEqual("Description",newProjectFlowController.projectData.description);
    }

    [Test]
    public void SetRowsAndColumnsCorrectly()
    {
        //Arrange
        newProjectFlowController.NewProject();
        
        //Act
        newProjectFlowController.SetRowsAndColumns(2,2);
        
        //Assert
        Assert.AreEqual(2,newProjectFlowController.projectData.rows);
        Assert.AreEqual(2,newProjectFlowController.projectData.cols);
    }

    [Test]
    public void FinishCreateNewProjectCorrectly()
    {
        //Arrange
        bool eventCalled = false;
        newProjectFlowController.OnNewProjectCrated += (x) => { eventCalled = true; };
        newProjectFlowController.NewProject();
        
        //Act
        newProjectFlowController.NewProjectCreated();
        
        //Assert
        Assert.GreaterOrEqual(DateTime.Now,newProjectFlowController.projectData.created_at);
        Assert.GreaterOrEqual(DateTime.Now,newProjectFlowController.projectData.updated_at);
        Assert.IsTrue(eventCalled);
    }
}
