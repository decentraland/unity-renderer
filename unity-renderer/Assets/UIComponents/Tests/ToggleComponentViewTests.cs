using NUnit.Framework;
using UnityEngine;

public class ToggleComponentViewTests
{
    private ToggleComponentView toggleComponent;

    [SetUp]
    public void SetUp()
    {
        toggleComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<ToggleComponentView>("Toggle_Capsule");
    }

    [TearDown]
    public void TearDown()
    {
        toggleComponent.Dispose();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetOnChanged(bool setOn)
    {
        // Arrange
        bool isOn = !setOn;
        toggleComponent.OnSelectedChanged += (isToggleOn, id, name) => isOn = isToggleOn;

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

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetTextActive(bool isTextActive)
    {
        // Act
        toggleComponent.SetTextActive(isTextActive);

        // Assert
        Assert.AreEqual(isTextActive, toggleComponent.model.isTextActive, "The text active does not match in the model.");
        Assert.AreEqual(isTextActive, toggleComponent.text.gameObject.activeInHierarchy, "The text gameobject active does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetInteractable(bool isInteractable)
    {
        // Act
        toggleComponent.SetInteractable(isInteractable);

        // Assert
        Assert.AreEqual(isInteractable, toggleComponent.toggle.interactable, "The toggle interactable field does not match");
        Assert.AreEqual(isInteractable, toggleComponent.IsInteractable(), "The toggle Is Interactable method does not match");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetIsOnWithoutNotify(bool setOn)
    {
        // Arrange
        bool isOn = !setOn;
        toggleComponent.OnSelectedChanged += (isToggleOn, id, name) => isOn = isToggleOn;

        // Act
        toggleComponent.SetIsOnWithoutNotify(setOn);

        // Assert
        Assert.AreNotEqual(isOn, setOn);
    }
}
