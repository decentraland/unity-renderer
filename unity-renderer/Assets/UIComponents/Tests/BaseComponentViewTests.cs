using NUnit.Framework;
using UnityEngine;

public class BaseComponentViewTests
{
    private BaseComponentView baseComponent;

    [SetUp]
    public void SetUp() { baseComponent = BaseComponentView.Create<ButtonComponentView>("Button_Common"); }

    [TearDown]
    public void TearDown()
    {
        baseComponent.Dispose();
        GameObject.Destroy(baseComponent.gameObject);
    }

    [Test]
    public void ShowComponentCorrectly()
    {
        // Act
        baseComponent.Show(true);

        // Assert
        Assert.IsTrue(baseComponent.isVisible, "The base component should be visible.");
    }

    [Test]
    public void HideComponentCorrectly()
    {
        // Act
        baseComponent.Hide(true);

        // Assert
        Assert.IsFalse(baseComponent.isVisible, "The base component should not be visible.");
    }
}