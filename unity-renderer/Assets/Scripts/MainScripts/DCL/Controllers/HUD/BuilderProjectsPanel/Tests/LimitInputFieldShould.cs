using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

public class LimitInputFieldShould 
{
    private const string VIEW_PREFAB_PATH = "NewProject/FirstStep";
    private LimitInputField inputField;
    private NewProjectFirstStepView gameObjectToDestroy;

    [SetUp]
    public void SetUp()
    {
        gameObjectToDestroy = Object.Instantiate(Resources.Load<NewProjectFirstStepView>(VIEW_PREFAB_PATH));
        inputField = gameObjectToDestroy.GetComponent<NewProjectFirstStepView>().titleInputField;
    }

    [TearDown]
    public void TearDown() { GameObject.Destroy(gameObjectToDestroy); }

    [Test]
    public void SetChangeInputCorrectly()
    {
        //Arrange
        string testString="";
        inputField.OnInputChange += s => { testString = s; };
        
        //Act
        inputField.InputChanged("test");
        
        //Assert
        Assert.AreEqual(testString,"test");
        Assert.AreEqual(inputField.GetValue(),"test");
    }

    [Test]
    public void SetInputAvailableCorrectly()
    {
        //Arrange
        inputField.hasBeenEmpty = false;
        inputField.hasPassedLimit = true;

        bool eventReceived = false;
        inputField.OnInputAvailable += () => { eventReceived = true; };
        
        //Act
        inputField.InputAvailable();
        
        //Assert
        Assert.IsTrue(eventReceived);
    }

    [Test]
    public void SetEmptyCorrectly()
    {
        //Arrange
        inputField.hasBeenEmpty = false;
        
        //Act
        inputField.Empty();
        
        //Assert
        Assert.IsTrue(inputField.hasBeenEmpty);
    }
    
    [Test]
    public void SetLimitReachedCorrectly()
    {
        //Arrange
        inputField.hasPassedLimit = false;
        
        //Act
        inputField.LimitReached();
        
        //Assert
        Assert.IsTrue(inputField.hasPassedLimit);
    }
}
