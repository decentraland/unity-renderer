using System.Collections;
using DCL.Chat.HUD;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ChannelMembersComponentViewShould
{
    private ChannelMembersComponentView experienceRowComponent;

    [SetUp]
    public void SetUp()
    {
        experienceRowComponent = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<ChannelMembersComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/ChannelMembersHUD.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        experienceRowComponent.Dispose();
    }

    [Test]
    public void ClearAllEntriesCorrectly()
    {
        // Arrange
        string testId = "testId";
        ChannelMemberEntryModel testModel = new ChannelMemberEntryModel
        {
            userId = testId,
            isOnline = true,
            thumnailUrl = "testUri",
            userName = "testName"
        };

        experienceRowComponent.memberList.Set(testId, testModel);

        // Act
        experienceRowComponent.ClearAllEntries();

        // Assert
        Assert.AreEqual(0, experienceRowComponent.memberList.Count());
        Assert.AreEqual($"Results ({0})", experienceRowComponent.resultsHeaderLabel.text);
    }

    [Test]
    public void ShowLoadingCorrectly()
    {
        // Arrange
        experienceRowComponent.loadingContainer.SetActive(false);
        experienceRowComponent.memberList.gameObject.SetActive(true);
        experienceRowComponent.resultsHeaderLabel.gameObject.SetActive(true);

        // Act
        experienceRowComponent.ShowLoading();

        // Assert
        Assert.IsTrue(experienceRowComponent.loadingContainer.activeSelf);
        Assert.IsFalse(experienceRowComponent.memberList.gameObject.activeSelf);
        Assert.IsFalse(experienceRowComponent.resultsHeaderLabel.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator SetCorrectly()
    {
        // Arrange
        experienceRowComponent.memberList.Clear();
        string testId = "testId";
        ChannelMemberEntryModel testModel = new ChannelMemberEntryModel
        {
            userId = testId,
            isOnline = true,
            thumnailUrl = "",
            userName = "testName"
        };

        // Act
        experienceRowComponent.Set(testModel);
        // wait for the queued entry to be added in the next frame
        yield return null;

        // Assert
        Assert.IsTrue(experienceRowComponent.memberList.Contains(testId));
        Assert.AreEqual($"Results ({1})", experienceRowComponent.resultsHeaderLabel.text);
    }

    [UnityTest]
    public IEnumerator RemoveCorrectly()
    {
        // Act
        yield return SetCorrectly();
        experienceRowComponent.Remove("testId");

        // Assert
        Assert.IsFalse(experienceRowComponent.memberList.Contains("testId"));
        Assert.AreEqual($"Results ({0})", experienceRowComponent.resultsHeaderLabel.text);
    }

    [Test]
    public void ShowCorrectly()
    {
        // Arrange
        experienceRowComponent.gameObject.SetActive(false);

        // Act
        experienceRowComponent.Show();

        // Assert
        Assert.IsTrue(experienceRowComponent.gameObject.activeSelf);
    }

    [Test]
    public void HideCorrectly()
    {
        // Arrange
        experienceRowComponent.gameObject.SetActive(true);

        // Act
        experienceRowComponent.Hide();

        // Assert
        Assert.IsFalse(experienceRowComponent.gameObject.activeSelf);
    }

    [Test]
    public void ClearSearchInputCorrectly()
    {
        // Arrange
        experienceRowComponent.searchBar.inputField.text = "test text";

        // Act
        experienceRowComponent.ClearSearchInput();

        // Assert
        Assert.AreEqual("", experienceRowComponent.searchBar.Text);
    }

    [Test]
    public void HideLoadingCorrectly()
    {
        // Arrange
        experienceRowComponent.loadingContainer.SetActive(true);
        experienceRowComponent.memberList.gameObject.SetActive(false);
        experienceRowComponent.resultsHeaderLabel.gameObject.SetActive(false);

        // Act
        experienceRowComponent.HideLoading();

        // Assert
        Assert.IsFalse(experienceRowComponent.loadingContainer.activeSelf);
        Assert.IsTrue(experienceRowComponent.memberList.gameObject.activeSelf);
        Assert.IsTrue(experienceRowComponent.resultsHeaderLabel.gameObject.activeSelf);
    }

    [Test]
    public void ShowLoadingMoreCorrectly()
    {
        // Arrange
        experienceRowComponent.loadMoreContainer.SetActive(false);

        // Act
        experienceRowComponent.ShowLoadingMore();

        // Assert
        Assert.IsTrue(experienceRowComponent.loadMoreContainer.activeSelf);
    }

    [Test]
    public void HideLoadingMoreCorrectly()
    {
        // Arrange
        experienceRowComponent.loadMoreContainer.SetActive(true);

        // Act
        experienceRowComponent.HideLoadingMore();

        // Assert
        Assert.IsFalse(experienceRowComponent.loadMoreContainer.activeSelf);
    }

    [Test]
    public void ShowResultsHeaderCorrectly()
    {
        // Arrange
        experienceRowComponent.resultsHeaderLabelContainer.SetActive(false);

        // Act
        experienceRowComponent.ShowResultsHeader();

        // Assert
        Assert.IsTrue(experienceRowComponent.resultsHeaderLabelContainer.activeSelf);
    }

    [Test]
    public void HideResultsHeaderCorrectly()
    {
        // Arrange
        experienceRowComponent.resultsHeaderLabelContainer.SetActive(true);

        // Act
        experienceRowComponent.HideResultsHeader();

        // Assert
        Assert.IsFalse(experienceRowComponent.resultsHeaderLabelContainer.activeSelf);
    }
}
