using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class ImageComponentViewTests
{
    private ImageComponentView imageComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        imageComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<ImageComponentView>("Image");
        imageComponent.imageObserver = Substitute.For<ILazyTextureObserver>();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        imageComponent.Dispose();
        GameObject.Destroy(imageComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigureImageCorrectly()
    {
        // Arrange
        ImageComponentModel testModel = new ImageComponentModel
        {
            sprite = testSprite
        };

        // Act
        imageComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, imageComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void SetImageFromSpriteCorrectly()
    {
        // Act
        imageComponent.SetImage(testSprite);

        // Assert
        Assert.AreEqual(testSprite, imageComponent.model.sprite, "The sprite does not match in the model.");
        Assert.AreEqual(testSprite, imageComponent.image.sprite, "The image does not match.");
    }

    [Test]
    public void SetImageFromTextureCorrectly()
    {
        // Arrange
        imageComponent.model.texture = null;

        // Act
        imageComponent.SetImage(testTexture);

        // Assert
        Assert.AreEqual(testTexture, imageComponent.model.texture, "The texture does not match in the model.");
        Assert.IsTrue(imageComponent.loadingIndicator.activeSelf);
        imageComponent.imageObserver.Received().RefreshWithTexture(testTexture);
    }

    [Test]
    public void SetImageFromUriCorrectly()
    {
        // Arrange
        string testUri = "testUri";
        imageComponent.model.uri = null;

        // Act
        imageComponent.SetImage(testUri);

        // Assert
        Assert.AreEqual(testUri, imageComponent.model.uri, "The uri does not match in the model.");
        Assert.IsTrue(imageComponent.loadingIndicator.activeSelf);
        imageComponent.imageObserver.Received().RefreshWithUri(testUri);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetFitParentCorrectly(bool fitParent)
    {
        // Act
        imageComponent.SetFitParent(fitParent);

        // Assert
        Assert.AreEqual(fitParent, imageComponent.model.fitParent, "The fitParent does not match in the model.");
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

    [UnityTest]
    public IEnumerator CallOnImageObserverUpdatedCorrectly()
    {
        // Arrange
        Sprite loadedSprite = null;
        imageComponent.OnLoaded += (sprite) => loadedSprite = sprite;
        imageComponent.currentSprite = null;
        imageComponent.SetLoadingIndicatorVisible(true);

        // Act
        imageComponent.OnImageObserverUpdated(testTexture);
        yield return null;

        // Assert
        Assert.IsNotNull(loadedSprite);
        Assert.AreEqual(loadedSprite, imageComponent.currentSprite);
        Assert.AreEqual(loadedSprite, imageComponent.model.sprite, "The sprite does not match in the model.");
        Assert.AreEqual(loadedSprite, imageComponent.image.sprite, "The image does not match.");
        Assert.IsFalse(imageComponent.loadingIndicator.activeSelf);
    }
}
