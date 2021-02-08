using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HotScenesControllerTests : IntegrationTestSuite_Legacy
{
    [UnityTest]
    public IEnumerator HotScenesControllerShouldParseJsonCorrectly()
    {
        var controller = HotScenesController.i;

        var hotSceneList = GetTestHotSceneList();

        var payload = new HotScenesController.HotScenesUpdatePayload
        {
            chunkIndex = 0,
            chunksCount = 1,
            scenesInfo = hotSceneList.ToArray()
        };

        Action onListUpdate = () =>
        {
            CheckListEquals(controller.hotScenesList, hotSceneList);
        };
        controller.OnHotSceneListChunkUpdated += onListUpdate;

        controller.UpdateHotScenesList(JsonUtility.ToJson(payload));

        controller.OnHotSceneListChunkUpdated -= onListUpdate;

        CheckListEquals(controller.hotScenesList, hotSceneList);

        yield return null;
    }

    List<HotScenesController.HotSceneInfo> GetTestHotSceneList()
    {
        var hotSceneList = new List<HotScenesController.HotSceneInfo>();
        hotSceneList.Add(new HotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(0, 0),
            realms = new HotScenesController.HotSceneInfo.Realm[]{
                new HotScenesController.HotSceneInfo.Realm()
                {
                    layer = "amber",
                    serverName = "fenrir",
                    usersCount = 10,
                    usersMax = 50
                },
                new HotScenesController.HotSceneInfo.Realm()
                {
                    layer = "blue",
                    serverName = "unicorn",
                    usersCount = 2,
                    usersMax = 50
                }
            },
            usersTotalCount = 12
        });

        hotSceneList.Add(new HotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(20, 20),
            realms = new HotScenesController.HotSceneInfo.Realm[]{
                new HotScenesController.HotSceneInfo.Realm()
                {
                    layer = "amber",
                    serverName = "fenrir",
                    usersCount = 1,
                    usersMax = 50
                }
            },
            usersTotalCount = 1
        });

        hotSceneList.Add(new HotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(70, -135),
            realms = new HotScenesController.HotSceneInfo.Realm[]{
                new HotScenesController.HotSceneInfo.Realm()
                {
                    layer = "red",
                    serverName = "temptation",
                    usersCount = 100,
                    usersMax = 50
                }
            },
            usersTotalCount = 100
        });

        return hotSceneList;
    }
    void CheckListEquals(List<HotScenesController.HotSceneInfo> l1, List<HotScenesController.HotSceneInfo> l2)
    {
        Assert.IsTrue(l1.Count == l2.Count, "HotScenesLists length mismatch");

        for (int i = 0; i < l1.Count; i++)
        {
            Assert.IsTrue(l1[i].baseCoords.x == l2[i].baseCoords.x && l1[i].baseCoords.y == l2[i].baseCoords.y,
                $"HotScenesLists baseCoords mismatch at index {i}");

            Assert.IsTrue(l1[i].usersTotalCount == l2[i].usersTotalCount, $"HotScenesLists usersTotalCount mismatch at index {i}");

            Assert.IsTrue(l1[i].realms.Length == l2[i].realms.Length, $"HotScenesLists realms length mismatch at index {i}");

            for (int j = 0; j < l1[i].realms.Length; j++)
            {
                Assert.IsTrue(l1[i].realms[j].serverName == l2[i].realms[j].serverName, $"HotScenesLists realms serverName mismatch at index {i},{j}");
                Assert.IsTrue(l1[i].realms[j].layer == l2[i].realms[j].layer, $"HotScenesLists realms layer mismatch at index {i},{j}");
                Assert.IsTrue(l1[i].realms[j].usersCount == l2[i].realms[j].usersCount, $"HotScenesLists realms usersCount mismatch at index {i},{j}");
                Assert.IsTrue(l1[i].realms[j].usersMax == l2[i].realms[j].usersMax, $"HotScenesLists realms usersMax mismatch at index {i},{j}");
            }
        }
    }
}
