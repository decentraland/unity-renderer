using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ButtonComponentViewTests
{
    private ButtonComponentView buttonComponent;
    private Texture2D testTexture;

    [SetUp]
    public void SetUp()
    {
        buttonComponent = CreateButtonComponent();
        testTexture = new Texture2D(20, 20);
    }

    [TearDown]
    public void TearDown()
    {
        buttonComponent.Dispose();
        GameObject.Destroy(buttonComponent.gameObject);
        GameObject.Destroy(testTexture);
    }

    [Test]
    public void SetOnClickCorrectly()
    {
        // Arrange
        bool isClicked = false;
        buttonComponent.onClick.AddListener(() => isClicked = true);

        // Act
        buttonComponent.button.onClick.Invoke();

        // Assert
        Assert.IsTrue(isClicked, "The button has not responded to the onClick action.");
    }

    [Test]
    public void ConfigureButtonCorrectly()
    {
        // Arrange
        ButtonComponentModel testModel = new ButtonComponentModel
        {
            icon = Sprite.Create(testTexture, new Rect(), Vector2.zero),
            text = "Test",
            onClick = new Button.ButtonClickedEvent()
        };

        // Act
        buttonComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, buttonComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshButtonCorrectly()
    {
        // Arrange
        Sprite testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        string testText = "Test";
        Button.ButtonClickedEvent testClickedEvent = new Button.ButtonClickedEvent();

        buttonComponent.model.icon = testSprite;
        buttonComponent.model.text = testText;
        buttonComponent.model.onClick = testClickedEvent;

        // Act
        buttonComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testSprite, buttonComponent.model.icon, "The icon does not match in the model.");
        Assert.AreEqual(testText, buttonComponent.model.text, "The text does not match in the model.");
        Assert.AreEqual(testClickedEvent, buttonComponent.model.onClick, "The onClick event does not match in the model.");
    }

    [Test]
    public void SetButtonTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        buttonComponent.SetText(testText);

        // Assert
        Assert.AreEqual(testText, buttonComponent.model.text, "The text does not match in the model.");
        Assert.AreEqual(testText, buttonComponent.text.text, "The button text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetButtonIconCorrectly(bool isNullIcon)
    {
        // Arrange
        Sprite testSprite = isNullIcon ? null : Sprite.Create(testTexture, new Rect(), Vector2.zero);

        // Act
        buttonComponent.SetIcon(testSprite);

        // Assert
        Assert.AreEqual(testSprite, buttonComponent.model.icon, "The icon does not match in the model.");
        Assert.AreEqual(testSprite, buttonComponent.icon.sprite, "The button icon does not match.");

        if (isNullIcon)
            Assert.IsFalse(buttonComponent.icon.enabled);
        else
            Assert.IsTrue(buttonComponent.icon.enabled);
    }

    protected virtual ButtonComponentView CreateButtonComponent() { return BaseComponentView.Create<ButtonComponentView>("Button_Common"); }
}

public class JumpInButtonComponentViewTests : ButtonComponentViewTests
{
    protected override ButtonComponentView CreateButtonComponent() { return BaseComponentView.Create<ButtonComponentView>("Button_JumpIn"); }
}

public class LinkButtonComponentViewTests : ButtonComponentViewTests
{
    protected override ButtonComponentView CreateButtonComponent() { return BaseComponentView.Create<ButtonComponentView>("Button_Link"); }
}