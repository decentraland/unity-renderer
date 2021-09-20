using NUnit.Framework;
using UnityEngine;

public class ImageComponentViewTests
{
    private ImageComponentView imageComponent;

    [SetUp]
    public void SetUp() { imageComponent = BaseComponentView.Create<ImageComponentView>("Image"); }

    [TearDown]
    public void TearDown()
    {
        imageComponent.Dispose();
        GameObject.Destroy(imageComponent.gameObject);
    }

    [Test]
    public void ConfigureImageCorrectly()
    {
        // Arrange
        ImageComponentModel testModel = new ImageComponentModel
        {
            sprite = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero)
        };

        // Act
        imageComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, imageComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshImageCorrectly()
    {
        // Arrange
        Sprite testSprite = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero);

        imageComponent.model.sprite = testSprite;

        // Act
        imageComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testSprite, imageComponent.model.sprite, "The sprite does not match in the model.");
    }

    [Test]
    public void SetImageCorrectly()
    {
        // Arrange
        Sprite testSprite = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero);

        // Act
        imageComponent.SetImage(testSprite);

        // Assert
        Assert.AreEqual(testSprite, imageComponent.model.sprite, "The sprite does not match in the model.");
        Assert.AreEqual(testSprite, imageComponent.image.sprite, "The image does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingIndicatorVisibleCorrectly(bool isVisible)
    {
        // Arrange
        imageComponent.image.enabled = isVisible;

        // Act
        imageComponent.SetLoadingIndicatorVisible(isVisible);

        // Assert
        Assert.AreNotEqual(isVisible, imageComponent.image.enabled);
        Assert.AreEqual(isVisible, imageComponent.loadingIndicator.activeSelf);
    }
}