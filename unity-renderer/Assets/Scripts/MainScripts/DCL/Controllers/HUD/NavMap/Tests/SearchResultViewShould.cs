using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class SearchResultViewShould
{
    private SearchRecordComponentView searchRecordComponentView;

    [SetUp]
    public void SetUp()
    {
        searchRecordComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/NavMap/Prefabs/SearchRecord.prefab"))
                                            .GetComponent<SearchRecordComponentView>();
    }

    [Test]
    public void SetRecordTitle()
    {
        searchRecordComponentView.SetRecordText("test text");

        Assert.AreEqual("test text", searchRecordComponentView.recordText.text, "Record text does not match");
        Assert.AreEqual("test text", searchRecordComponentView.recordTextNoPlayerCount.text, "Record text does not match");
    }

    [Test]
    public void SetHistoryRecordIcon()
    {
        searchRecordComponentView.SetIcon(true);

        Assert.True(searchRecordComponentView.historyIcon.activeSelf, "History icon is not active");
    }

    [Test]
    public void SetPlayerCount()
    {
        searchRecordComponentView.SetPlayerCount(10);

        Assert.True(searchRecordComponentView.playerCountParent.activeSelf, "Player count parent is not active");
        Assert.AreEqual("10", searchRecordComponentView.playerCount.text, "Player count text does not match");
    }

    [Test]
    public void SetNoPlayerCount()
    {
        searchRecordComponentView.SetPlayerCount(0);

        Assert.False(searchRecordComponentView.playerCountParent.activeSelf, "Player count parent is active");
    }

    [TearDown]
    public void TearDown()
    {
        searchRecordComponentView.Dispose();
        Object.Destroy(searchRecordComponentView.gameObject);
    }
}
