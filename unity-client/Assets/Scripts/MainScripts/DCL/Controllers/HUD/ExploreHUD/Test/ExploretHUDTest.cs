using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class ExploreHUDShould : IntegrationTestSuite_Legacy
    {
        private ExploreHUDController controller;
        private FriendsController_Mock friendsController;

        readonly Vector2Int BASECOORD_TEMPTATION = new Vector2Int(70, -135);
        readonly Vector2Int BASECOORD_FIRST_CELL = new Vector2Int(0, 0);
        readonly Vector2Int BASECOORD_SECOND_CELL = new Vector2Int(20, 20);

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            ExploreHUDController.isTest = true;

            SetupUserProfileController();

            friendsController = new FriendsController_Mock();
            controller = new ExploreHUDController();
            controller.Initialize(friendsController);
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator TriggerToggleCorrectly()
        {
            Assert.NotNull(controller.view);
            Assert.IsFalse(controller.view.popup.gameObject.activeSelf);

            bool wasTriggered = false;
            Action toggleTriggered = () => wasTriggered = true;

            controller.OnToggleTriggered += toggleTriggered;
            controller.toggleExploreTrigger.RaiseOnTriggered();
            controller.OnToggleTriggered -= toggleTriggered;

            Assert.IsTrue(wasTriggered);
            yield break;
        }

        [UnityTest]
        public IEnumerator DisplayCorrectly()
        {
            controller.SetVisibility(true);
            Assert.IsTrue(controller.view.gameObject.activeSelf);

            SimulateFriendsUpdate();
            SimulateHotSceneUpdate();

            yield return null;

            var activeCells = GetActiveCells();
            Assert.AreEqual(activeCells.Count, 3);

            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_FIRST_CELL]), 0);
            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_SECOND_CELL]), 0);
            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_TEMPTATION]), 1);

            friendsController.RaiseUpdateUserStatus("userOnline",
                new FriendsController.UserStatus()
                {
                    position = BASECOORD_FIRST_CELL,
                    presence = PresenceStatus.ONLINE,
                    realm = new FriendsController.UserStatus.Realm() { layer = "layer", serverName = "server" }
                });

            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_FIRST_CELL]), 1);
            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_TEMPTATION]), 0);

            friendsController.RaiseUpdateUserStatus("userOnline",
                new FriendsController.UserStatus()
                {
                    position = BASECOORD_FIRST_CELL,
                    presence = PresenceStatus.OFFLINE,
                    realm = new FriendsController.UserStatus.Realm() { layer = "layer", serverName = "server" }
                });

            Assert.AreEqual(GetFriendsCount(activeCells[BASECOORD_FIRST_CELL]), 0);
        }

        private void SetupFriendController()
        {
            friendsController = new FriendsController_Mock();
        }

        private void SetupUserProfileController()
        {
            UserProfileModel userOnline = new UserProfileModel()
            {
                name = "userOnline",
                userId = "userOnline"
            };
            UserProfileModel userOffline = new UserProfileModel()
            {
                userId = "userOffline",
                name = "userOffline"
            };
            UserProfileModel userOnlineNoRealm = new UserProfileModel()
            {
                name = "userOnlineNoRealm",
                userId = "userOnlineNoRealm"
            };

            UserProfileController.i.AddUserProfileToCatalog(userOnline);
            UserProfileController.i.AddUserProfileToCatalog(userOffline);
            UserProfileController.i.AddUserProfileToCatalog(userOnlineNoRealm);
        }

        private void SimulateFriendsUpdate()
        {
            var userOnline = new FriendsController.UserStatus()
            {
                position = BASECOORD_TEMPTATION,
                presence = PresenceStatus.ONLINE,
                realm = new FriendsController.UserStatus.Realm() { layer = "layer", serverName = "server" },
                userId = "userOnline"
            };

            var userOffline = new FriendsController.UserStatus()
            {
                position = BASECOORD_TEMPTATION,
                presence = PresenceStatus.OFFLINE,
                realm = new FriendsController.UserStatus.Realm() { layer = "layer", serverName = "server" },
                userId = "userOffline"
            };

            var userOnlineNoRealm = new FriendsController.UserStatus()
            {
                position = BASECOORD_TEMPTATION,
                presence = PresenceStatus.ONLINE,
                realm = new FriendsController.UserStatus.Realm() { layer = "", serverName = "" },
                userId = "userOnlineNoRealm"
            };

            friendsController.RaiseUpdateUserStatus("userOnline", userOnline);
            friendsController.RaiseUpdateUserStatus("userOffline", userOffline);
            friendsController.RaiseUpdateUserStatus("userOnlineNoRealm", userOnlineNoRealm);
        }

        private void SimulateHotSceneUpdate()
        {
            var scenes = new HotScenesController.HotSceneInfo[] {
                new HotScenesController.HotSceneInfo()
                {
                    baseCoords = BASECOORD_FIRST_CELL,
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
                    parcels = new Vector2Int[]{ BASECOORD_FIRST_CELL, BASECOORD_FIRST_CELL + new Vector2Int(0,1)},
                    usersTotalCount = 12
                },

                new HotScenesController.HotSceneInfo()
                {
                    baseCoords = BASECOORD_SECOND_CELL,
                    realms = new HotScenesController.HotSceneInfo.Realm[]{
                        new HotScenesController.HotSceneInfo.Realm()
                        {
                            layer = "amber",
                            serverName = "fenrir",
                            usersCount = 1,
                            usersMax = 50
                        }
                    },
                    parcels = new Vector2Int[]{ BASECOORD_SECOND_CELL },
                    usersTotalCount = 1
                },

                new HotScenesController.HotSceneInfo()
                {
                    baseCoords = BASECOORD_TEMPTATION,
                    realms = new HotScenesController.HotSceneInfo.Realm[]{
                        new HotScenesController.HotSceneInfo.Realm()
                        {
                            layer = "red",
                            serverName = "temptation",
                            usersCount = 100,
                            usersMax = 50
                        }
                    },
                    parcels = new Vector2Int[]{ BASECOORD_TEMPTATION,
                                    BASECOORD_TEMPTATION + new Vector2Int(0,1),
                                    BASECOORD_TEMPTATION + new Vector2Int(0,2)},
                    usersTotalCount = 100
                }
             };

            string scenesJson = "[";
            for (int i = 0; i < scenes.Length; i++)
            {
                scenesJson += JsonUtility.ToJson(scenes[i]);
                if (i != scenes.Length - 1) scenesJson += ",";
            }
            scenesJson += "]";

            var json = "{\"chunkIndex\":0,\"chunksCount\":1,\"scenesInfo\":" + scenesJson + "}";
            HotScenesController.i.UpdateHotScenesList(json);
        }

        Dictionary<Vector2Int, HotSceneCellView> GetActiveCells()
        {
            Dictionary<Vector2Int, HotSceneCellView> ret = new Dictionary<Vector2Int, HotSceneCellView>();
            var cells = GameObject.FindObjectsOfType<HotSceneCellView>();
            for (int i = 0; i < cells.Length; i++)
            {
                ret.Add(cells[i].mapInfoHandler.baseCoord, cells[i]);
            }
            return ret;
        }

        int GetFriendsCount(HotSceneCellView cell)
        {
            var friendsView = cell.transform.GetComponentsInChildren<ExploreFriendsView>(includeInactive: false);
            return friendsView.Length;
        }
    }
}