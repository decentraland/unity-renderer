using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

public class NewRrojectSecondStepShould 
{
    private const string VIEW_PREFAB_PATH = "NewProject/SecondStep";
    private NewProjectSecondStepView newProjectSecondStepView;

    [SetUp]
    public void SetUp()
    {
        newProjectSecondStepView = Object.Instantiate(Resources.Load<NewProjectSecondStepView>(VIEW_PREFAB_PATH)).GetComponent<NewProjectSecondStepView>();
    }

    [TearDown]
    public void TearDown() { GameObject.Destroy(newProjectSecondStepView.gameObject); }
    
    [Test]
    public void NextPressedCorrectly()
    {
        //Arrange
        bool eventReceived = false;
        newProjectSecondStepView.OnNextPressed += (x,y) => { eventReceived = true; };
        newProjectSecondStepView.nextButton.SetInteractable(true);
        
        //Act
        newProjectSecondStepView.NextPressed();
        
        Assert.IsTrue(eventReceived);
    }
    
    [Test]
    public void BackPressCorrectly()
    {
        //Arrange
        bool eventReceived = false;
        newProjectSecondStepView.OnBackPressed += () => { eventReceived = true; };
        
        //Act
        newProjectSecondStepView.BackPressed();
        
        //Assert
        Assert.IsTrue(eventReceived);
    }
    
    [Test]
    public void ChangeRowsCorreclty()
    {
        //Act
        newProjectSecondStepView.RowsChanged("2");
        
        //Assert
        Assert.AreEqual(2,newProjectSecondStepView.rows);
    }
    
    [Test]
    public void ChangeColsCorreclty()
    {
        //Act
        newProjectSecondStepView.ColumnsChanged("2");
        
        //Assert
        Assert.AreEqual(2,newProjectSecondStepView.colums);
    }

    [Test]
    public void ShowErrorCorrectly()
    {
        //Act
        newProjectSecondStepView.ShowError();
        
        //Assert
        Assert.IsTrue(newProjectSecondStepView.errorGameObject.activeSelf);
        Assert.IsFalse(newProjectSecondStepView.gridGameObject.activeSelf);
        Assert.AreEqual(newProjectSecondStepView.parcelText.color, newProjectSecondStepView.errorTextColor);
    }
    
    [Test]
    public void ShowGridCorrectly()
    {
        //Act
        newProjectSecondStepView.ShowGrid();
        
        //Assert
        Assert.IsFalse(newProjectSecondStepView.errorGameObject.activeSelf);
        Assert.IsTrue(newProjectSecondStepView.gridGameObject.activeSelf);
        Assert.AreEqual(newProjectSecondStepView.parcelText.color, newProjectSecondStepView.normalTextColor);
    }
}
