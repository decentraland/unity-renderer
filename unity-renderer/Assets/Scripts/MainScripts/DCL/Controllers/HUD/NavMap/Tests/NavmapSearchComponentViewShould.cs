using MainScripts.DCL.Controllers.HotScenes;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavmapSearchComponentViewShould
{
    private NavmapSearchComponentView navmapSearchComponentView;

    [SetUp]
    public void SetUp()
    {
        navmapSearchComponentView = Object.Instantiate(
                                               AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/NavMap/Prefabs/NavmapHeader.prefab"))
                                          .GetComponent<NavmapSearchComponentView>();
    }

    [Test]
    public void SetHistoryRecords()
    {
        navmapSearchComponentView.SetHistoryRecords(new []{ "test1", "test2", "test3", "test4" });

        Assert.False(navmapSearchComponentView.noRecordsFound.activeSelf, "No records found is active");
        Assert.AreEqual(4, navmapSearchComponentView.usedRecords.Count, "Used records count does not match");
        Assert.True(navmapSearchComponentView.searchResultsContainer.activeSelf, "Search results container is not active");
    }

    [Test]
    public void SetNoHistoryRecords()
    {
        navmapSearchComponentView.SetHistoryRecords(new string[0]);

        Assert.False(navmapSearchComponentView.noRecordsFound.activeSelf, "No records found is not active");
        Assert.AreEqual(0, navmapSearchComponentView.usedRecords.Count, "Used records count does not match");
        Assert.True(navmapSearchComponentView.searchResultsContainer.activeSelf, "Search results container is active");
    }

    [Test]
    public void SetSearchResults()
    {
        navmapSearchComponentView.SetSearchResultRecords(new List<IHotScenesController.PlaceInfo>(){new IHotScenesController.PlaceInfo(){title = "Test title", user_count = 4, base_position = "10,30"}});

        Assert.False(navmapSearchComponentView.noRecordsFound.activeSelf, "No records found is active");
        Assert.AreEqual(1, navmapSearchComponentView.usedRecords.Count, "Used records count does not match");
        Assert.True(navmapSearchComponentView.searchResultsContainer.activeSelf, "Search results container is not active");
    }

    [Test]
    public void SetNoSearchResults()
    {
        navmapSearchComponentView.SetSearchResultRecords(new List<IHotScenesController.PlaceInfo>());

        Assert.True(navmapSearchComponentView.noRecordsFound.activeSelf, "No records found is not active");
        Assert.AreEqual(0, navmapSearchComponentView.usedRecords.Count, "Used records count does not match");
        Assert.True(navmapSearchComponentView.searchResultsContainer.activeSelf, "Search results container is active");
    }

    [TearDown]
    public void TearDown()
    {
        navmapSearchComponentView.Dispose();
        Object.Destroy(navmapSearchComponentView.gameObject);
    }
}
