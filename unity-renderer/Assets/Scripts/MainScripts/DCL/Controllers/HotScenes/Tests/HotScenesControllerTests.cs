using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using MainScripts.DCL.Controllers.HotScenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[Obsolete("MapRenderer")]
public class HotScenesControllerTests : IntegrationTestSuite_Legacy
{
    private HotScenesController hotScenesController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        hotScenesController = TestUtils.CreateComponentWithGameObject<HotScenesController>("HotScenesController");
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(hotScenesController.gameObject);
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator HotScenesControllerShouldParseJsonCorrectly()
    {
        var controller = hotScenesController;

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

    List<IHotScenesController.HotSceneInfo> GetTestHotSceneList()
    {
        var hotSceneList = new List<IHotScenesController.HotSceneInfo>();
        hotSceneList.Add(new IHotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(0, 0),
            realms = new IHotScenesController.HotSceneInfo.Realm[]
            {
                new IHotScenesController.HotSceneInfo.Realm()
                {
                    layer = "amber",
                    serverName = "fenrir",
                    usersCount = 10,
                    maxUsers = 50
                },
                new IHotScenesController.HotSceneInfo.Realm()
                {
                    layer = "blue",
                    serverName = "unicorn",
                    usersCount = 2,
                    maxUsers = 50
                }
            },
            usersTotalCount = 12
        });

        hotSceneList.Add(new IHotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(20, 20),
            realms = new IHotScenesController.HotSceneInfo.Realm[]
            {
                new IHotScenesController.HotSceneInfo.Realm()
                {
                    layer = "amber",
                    serverName = "fenrir",
                    usersCount = 1,
                    maxUsers = 50
                }
            },
            usersTotalCount = 1
        });

        hotSceneList.Add(new IHotScenesController.HotSceneInfo()
        {
            baseCoords = new Vector2Int(70, -135),
            realms = new IHotScenesController.HotSceneInfo.Realm[]
            {
                new IHotScenesController.HotSceneInfo.Realm()
                {
                    layer = "red",
                    serverName = "temptation",
                    usersCount = 100,
                    maxUsers = 50
                }
            },
            usersTotalCount = 100
        });

        return hotSceneList;
    }

    void CheckListEquals(List<IHotScenesController.HotSceneInfo> l1, List<IHotScenesController.HotSceneInfo> l2)
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
                Assert.IsTrue(l1[i].realms[j].maxUsers == l2[i].realms[j].maxUsers, $"HotScenesLists realms usersMax mismatch at index {i},{j}");
            }
        }
    }
}
