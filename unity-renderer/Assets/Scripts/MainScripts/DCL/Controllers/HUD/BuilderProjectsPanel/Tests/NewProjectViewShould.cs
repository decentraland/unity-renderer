using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

public class NewProjectViewShould 
{
    private const string VIEW_PREFAB_PATH = "NewProject/NewProjectFlowView";
    private NewProjectFlowView view;

    [SetUp]
    public void SetUp()
    {
        view = Object.Instantiate(Resources.Load<NewProjectFlowView>(VIEW_PREFAB_PATH)).GetComponent<NewProjectFlowView>();
    }

    [TearDown]
    public void TearDown() { GameObject.Destroy(view.gameObject); }
    
    [Test]
    public void SetSizeCorrectly()
    {
        //Arrange
        int rows = 0;
        int cols = 0;
        view.OnSizeSet += (x, y) =>
        {
            rows = x;
            cols = y;
        };

        //Act
        view.SetSize(2,2);
        
        //Assert
        Assert.AreEqual(2,rows);
        Assert.AreEqual(2,cols);
    }
    
    [Test]
    public void SetTittleAndDescription()
    {
        //Arrange
        string title = "";
        string desc = "";
        view.OnTittleAndDescriptionSet += (x, y) =>
        {
            title = x;
            desc = y;
        };

        //Act
        view.SetTittleAndDescription("2","2");
        
        //Assert
        Assert.AreEqual("2",title);
        Assert.AreEqual("2",desc);
    }

    [Test]
    public void BackPressedCorrectly()
    {
        //Arrange
        view.currentStep = 1;
        
        //Act
        view.BackPressed();
        
        //Assert
        Assert.AreEqual(view.currentStep,0);
    }
    
    [Test]
    public void NextPressedCorrectly()
    {
        //Arrange
        view.currentStep = 0;
        
        //Act
        view.NextPressed();
        
        //Assert
        Assert.AreEqual(view.currentStep,1);
    }
}
