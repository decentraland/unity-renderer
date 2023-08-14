using DCL.Social.Chat;
using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DateSeparatorEntryShould
{
    private DateSeparatorEntry entry;

    [SetUp]
    public void SetUp()
    {
        entry = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<DateSeparatorEntry>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/ChatEntrySeparator.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(entry.gameObject);
    }

    [Test]
    public void SetOldDateText()
    {
        var timestamp = new DateTimeOffset(new DateTime(1989, 1, 19, 10, 30, 0)).ToUnixTimeMilliseconds();
        entry.Populate(new ChatEntryModel{timestamp = (ulong) timestamp});

        Assert.AreEqual("Thursday, 19 January 1989", entry.title.text);
    }

    [Test]
    public void SetTodaysDateText()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        entry.Populate(new ChatEntryModel{timestamp = (ulong) timestamp});

        Assert.AreEqual("Today", entry.title.text);
    }
}
