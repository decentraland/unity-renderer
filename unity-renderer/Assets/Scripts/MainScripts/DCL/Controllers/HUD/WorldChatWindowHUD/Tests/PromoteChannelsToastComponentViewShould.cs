﻿using DCL.Chat.HUD;
using NUnit.Framework;

public class PromoteChannelsToastComponentViewShould
{
    private PromoteChannelsToastComponentView promoteChannelsToastView;

    [SetUp]
    public void SetUp()
    {
        promoteChannelsToastView = PromoteChannelsToastComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        promoteChannelsToastView.Dispose();
    }

    [Test]
    public void ShowToastCorrectly()
    {
        // Arrange
        promoteChannelsToastView.gameObject.SetActive(false);

        // Act
        promoteChannelsToastView.Show();

        // Assert
        Assert.IsTrue(promoteChannelsToastView.gameObject.activeSelf);
    }

    [Test]
    public void HideToastCorrectly()
    {
        // Arrange
        promoteChannelsToastView.gameObject.SetActive(true);

        // Act
        promoteChannelsToastView.Hide();

        // Assert
        Assert.IsFalse(promoteChannelsToastView.gameObject.activeSelf);
    }

    [Test]
    public void ClickOnCloseCorrectly()
    {
        // Arrange
        bool isClosed = false;
        promoteChannelsToastView.OnClose += () => isClosed = true;

        // Act
        promoteChannelsToastView.closeButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(isClosed);
    }
}
