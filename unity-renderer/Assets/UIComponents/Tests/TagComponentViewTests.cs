using NUnit.Framework;
using UnityEngine;

public class TagComponentViewTests
{
    private TagComponentView tagComponent;
    private Texture2D testTexture;

    [SetUp]
    public void SetUp()
    {
        tagComponent = BaseComponentView.Create<TagComponentView>("Tag");
        testTexture = new Texture2D(20, 20);
    }

    [TearDown]
    public void TearDown()
    {
        tagComponent.Dispose();
        GameObject.Destroy(tagComponent.gameObject);
        GameObject.Destroy(testTexture);
    }

    [Test]
    public void ConfigureTagCorrectly()
    {
        // Arrange
        TagComponentModel testModel = new TagComponentModel
        {
            icon = Sprite.Create(testTexture, new Rect(), Vector2.zero),
            text = "Test"
        };

        // Act
        tagComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, tagComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshTagCorrectly()
    {
        // Arrange
        Sprite testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        string testText = "Test";

        tagComponent.model.icon = testSprite;
        tagComponent.model.text = testText;

        // Act
        tagComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testSprite, tagComponent.model.icon, "The icon does not match in the model.");
        Assert.AreEqual(testText, tagComponent.model.text, "The text does not match in the model.");
    }

    [Test]
    public void SetTagTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        tagComponent.SetText(testText);

        // Assert
        Assert.AreEqual(testText, tagComponent.model.text, "The text does not match in the model.");
        Assert.AreEqual(testText, tagComponent.text.text, "The button text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetTagIconCorrectly(bool isNullIcon)
    {
        // Arrange
        Sprite testSprite = isNullIcon ? null : Sprite.Create(testTexture, new Rect(), Vector2.zero);

        // Act
        tagComponent.SetIcon(testSprite);

        // Assert
        Assert.AreEqual(testSprite, tagComponent.model.icon, "The icon does not match in the model.");
        Assert.AreEqual(testSprite, tagComponent.icon.sprite, "The button icon does not match.");

        if (isNullIcon)
            Assert.IsFalse(tagComponent.icon.enabled);
        else
            Assert.IsTrue(tagComponent.icon.enabled);
    }
}