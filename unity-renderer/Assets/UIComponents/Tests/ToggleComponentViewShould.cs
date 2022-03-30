using NUnit.Framework;
using UnityEngine;

public class ToggleComponentViewShould
{
    private ToggleComponentView toggleComponent;

    [SetUp]
    public void SetUp()
    {
        toggleComponent = BaseComponentView.Create<ToggleComponentView>("Toggle_Capsule");
    }

    [TearDown]
    public void TearDown()
    {
        toggleComponent.Dispose();
        GameObject.Destroy(toggleComponent.gameObject);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetOnChanged(bool setOn)
    {
        // Arrange
        bool isOn = !setOn;
        toggleComponent.onToggleChange.AddListener((_)=>isOn=_);

        // Act
        toggleComponent.toggle.onValueChanged.Invoke(setOn);

        // Assert
        Assert.AreEqual(isOn, setOn, "The toggle has not responded to the onToggleChange action.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ConfigureToggle(bool isTextActive)
    {
        // Arrange
        ToggleComponentModel testModel = new ToggleComponentModel
        {
            text = "Test",
            isTextActive = isTextActive
        };

        // Act
        toggleComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(toggleComponent.text.gameObject.activeInHierarchy, isTextActive);
        Assert.AreEqual(testModel, toggleComponent.model, "The model does not match after configuring the toggle.");
    }

    [Test]
    public void SetToggleText()
    {
        // Arrange
        string testText = "Test";

        // Act
        toggleComponent.SetText(testText);

        // Assert
        Assert.AreEqual(testText, toggleComponent.model.text, "The text does not match in the model.");
        Assert.AreEqual(testText, toggleComponent.text.text, "The toggle text does not match.");
    }


    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RefreshComponent(bool isTextActive)
    {
        // Arrange
        ToggleComponentModel testModel = new ToggleComponentModel
        {
            text = "Test",
            isTextActive = isTextActive
        };

        // Act
        toggleComponent.Configure(testModel);
        toggleComponent.model.text = "Test2";
        toggleComponent.model.isTextActive = !toggleComponent.model.isTextActive;
        toggleComponent.RefreshControl();

        // Assert
        Assert.AreEqual(toggleComponent.text.gameObject.activeInHierarchy, !isTextActive, "The text active field does not match after the refresh.");
        Assert.AreEqual(toggleComponent.text.text, "Test2", "The text does not match after the refresh.");
    }
}
