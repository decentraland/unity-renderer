using NUnit.Framework;
using UnityEngine;

public class TagComponentViewTests
{
    private TagComponentView tagComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        tagComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<TagComponentView>("Tag");
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        tagComponent.Dispose();
        GameObject.Destroy(tagComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigureTagCorrectly()
    {
        // Arrange
        TagComponentModel testModel = new TagComponentModel
        {
            icon = testSprite,
            text = "Test"
        };

        // Act
        tagComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, tagComponent.model, "The model does not match after configuring the button.");
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
        Sprite testingSprite = isNullIcon ? null : testSprite;

        // Act
        tagComponent.SetIcon(testingSprite);

        // Assert
        Assert.AreEqual(testingSprite, tagComponent.model.icon, "The icon does not match in the model.");
        Assert.AreEqual(testingSprite, tagComponent.icon.sprite, "The button icon does not match.");

        if (isNullIcon)
            Assert.IsFalse(tagComponent.icon.enabled);
        else
            Assert.IsTrue(tagComponent.icon.enabled);
    }
}
