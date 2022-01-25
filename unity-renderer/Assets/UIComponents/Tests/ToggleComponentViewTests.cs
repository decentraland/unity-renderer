using NUnit.Framework;

public class ToggleComponentViewTests
{
    private ToggleComponentView togggleComponent;

    [SetUp]
    public void SetUp()
    {
        togggleComponent = BaseComponentView.Create<ToggleComponentView>("Toggle_Checkbox");
    }

    [TearDown]
    public void TearDown()
    {
        togggleComponent.Dispose();
    }

    [Test]
    public void ConfigureToggleCorrectly()
    {
        // Arrange
        ToggleComponentModel testModel = new ToggleComponentModel
        {
            id = "1",
            text = "Test",
            isOn = true
        };

        // Act
        togggleComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, togggleComponent.model, "The model does not match after configuring the toggle.");
    }

    [Test]
    public void SetToggleTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        togggleComponent.SetText(testText);

        // Assert
        Assert.AreEqual(testText, togggleComponent.model.text, "The text does not match in the model.");
        Assert.AreEqual(testText, togggleComponent.toggleText.text, "The toggle text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetToggleOnOffCorrectly(bool isOn)
    {
        // Arrange
        string testId = "Test";
        togggleComponent.isOn = !isOn;
        togggleComponent.id = testId;

        bool isOnConfirmation = !isOn;
        string changedId = "";
        togggleComponent.OnSelectedChanged += (isOn, id) =>
        {
            isOnConfirmation = isOn;
            changedId = id;
        };

        // Act
        togggleComponent.isOn = isOn;

        // Assert
        Assert.AreEqual(isOn, togggleComponent.model.isOn, "The isOn property does not match in the model.");
        Assert.AreEqual(isOn, togggleComponent.toggle.isOn, "The toggle isOn status does not match.");
        Assert.AreEqual(isOn, isOnConfirmation);
        Assert.AreEqual(testId, changedId);
    }

    [Test]
    public void SetToggleIdCorrectly()
    {
        // Arrange
        string testId = "123";

        // Act
        togggleComponent.id = testId;

        // Assert
        Assert.AreEqual(testId, togggleComponent.model.id, "The id property does not match in the model.");
    }
}
