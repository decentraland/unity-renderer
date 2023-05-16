using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class ModalComponentShould
{
    private ModalComponentView modalComponent;
    private GameObject modelContent;
    [SetUp]
    public void SetUp()
    {
        modalComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<ModalComponentView>("Modal");
    }

    [TearDown]
    public void TearDown()
    {
        modalComponent.Dispose();
        GameObject.Destroy(modalComponent.gameObject);

        if(modelContent != null)
            GameObject.Destroy(modelContent);
    }

    [Test]
    public void CreateContentCorrectly()
    {
        //Arrange
        ModalComponentModel model = new ModalComponentModel();
        modelContent = new GameObject("TestGameObject");
        model.content = modelContent;

        // Act
        modalComponent.Configure(model);

        // Assert
        Assert.NotNull(modalComponent.content);
    }

    [Test]
    public void SendEventCloseButtonCorrectly()
    {
        //Arrange
        bool eventCalled = false;
        modalComponent.OnCloseAction +=() => eventCalled = true;

        // Act
        modalComponent.CloseButtonClicked();

        // Assert
        Assert.IsTrue(eventCalled);
    }
}
