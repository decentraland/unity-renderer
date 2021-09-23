using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class BaseComponentViewTests
{
    private BaseComponentView baseComponent;
    private CanvasGroup baseComponentCanvas;

    [SetUp]
    public void SetUp()
    {
        baseComponent = BaseComponentView.Create<ButtonComponentView>("Button_Common");
        baseComponentCanvas = baseComponent.GetComponent<CanvasGroup>();
    }

    [TearDown]
    public void TearDown()
    {
        baseComponent.Dispose();
        GameObject.Destroy(baseComponent.gameObject);
    }

    [Test]
    public void InitializeComponentCorrectly()
    {
        // Arrange
        bool initializedEventInvoked = false;
        baseComponent.OnFullyInitialized += () => initializedEventInvoked = true;

        // Act
        baseComponent.Initialize();

        // Assert
        Assert.IsTrue(baseComponent.isFullyInitialized, "The base component should be initialized.");
        Assert.IsTrue(initializedEventInvoked, "The base component should have invoked the OnInitialized event.");
        Assert.IsNotNull(baseComponent.showHideAnimator, "The base component show/hide animator is null.");
    }

    [UnityTest]
    public IEnumerator ShowComponentCorrectly()
    {
        // Arrange
        baseComponentCanvas.alpha = 0f;

        // Act
        baseComponent.Show(true);
        yield return null;

        // Assert
        Assert.IsTrue(baseComponent.showHideAnimator.isVisible, "The base component should be visible.");
    }

    [UnityTest]
    public IEnumerator HideComponentCorrectly()
    {
        // Arrange
        baseComponentCanvas.alpha = 1f;

        // Act
        baseComponent.Hide(true);
        yield return null;

        // Assert
        Assert.IsFalse(baseComponent.showHideAnimator.isVisible, "The base component should not be visible.");
    }
}