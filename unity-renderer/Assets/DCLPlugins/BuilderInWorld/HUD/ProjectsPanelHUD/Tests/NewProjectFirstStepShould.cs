using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

public class NewProjectFirstStepShould 
{
    private const string VIEW_PREFAB_PATH = "NewProject/FirstStep";
    private NewProjectFirstStepView firsStepView;

    [SetUp]
    public void SetUp()
    {
        firsStepView = Object.Instantiate(Resources.Load<NewProjectFirstStepView>(VIEW_PREFAB_PATH)).GetComponent<NewProjectFirstStepView>();
    }

    [TearDown]
    public void TearDown() { GameObject.Destroy(firsStepView.gameObject); }

    [Test]
    public void NextPressCorrectly()
    {
        //Arrange
        bool eventReceived = false;
        firsStepView.OnNextPressed += (x,y) => { eventReceived = true; };
        firsStepView.EnableNextButton();
        
        //Act
        firsStepView.NextPressed();
        
        Assert.IsTrue(eventReceived);
    }
    
    [Test]
    public void BackPressCorrectly()
    {
        //Arrange
        bool eventReceived = false;
        firsStepView.OnBackPressed += () => { eventReceived = true; };
        
        //Act
        firsStepView.BackPressed();
        
        //Assert
        Assert.IsTrue(eventReceived);
    }

    [Test]
    public void EnableNextButtonCorrectly()
    {
        //Act
        firsStepView.EnableNextButton();
        
        //Assert
        Assert.IsTrue(firsStepView.nextButton.IsInteractable());
    }
    
    [Test]
    public void DisableNextButtonCorrectly()
    {
        //Act
        firsStepView.DisableNextButton();
        
        //Assert
        Assert.IsFalse(firsStepView.nextButton.IsInteractable());
    }
}
