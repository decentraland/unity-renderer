using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace DCL
{
    public class GlobalUsersPositionMarkerTests
    {
        [Test]
        public void SceneFilterShouldReturnTheCorrectAmount()
        {
            const int MAX_MARKERS = 200;
            const int INITIAL_SCENES_AMOUNT = 20; // Smaller than MAX_MARKERS
            const int FINAL_SCENES_AMOUNT = 237; // Greater than MAX_MARKERS

            List<HotScenesController.HotSceneInfo> initialScenes = new List<HotScenesController.HotSceneInfo>();
            List<HotScenesController.HotSceneInfo> finalScenes = new List<HotScenesController.HotSceneInfo>();

            for (int i = 0; i < FINAL_SCENES_AMOUNT; i++)
            {
                var scene = CreateSceneInfo(Vector2Int.zero);
                if (i < INITIAL_SCENES_AMOUNT)
                {
                    initialScenes.Add(scene);
                }

                finalScenes.Add(scene);
            }

            ScenesFilter filter = new ScenesFilter();
            Assert.AreEqual(INITIAL_SCENES_AMOUNT, filter.Filter(initialScenes, MAX_MARKERS).Count);
            Assert.AreEqual(MAX_MARKERS, filter.Filter(finalScenes, MAX_MARKERS).Count);
        }

        [Test]
        public void SceneFetcherShouldSetIntervalsCorrectly()
        {
            const float INITIAL_INTERVAL = 1;
            const float FOREGROUND_INTERVAL = 1.5f;
            const float BACKROUND_INTERVAL = 3.5f;

            FetchScenesHandler handler = new FetchScenesHandler(INITIAL_INTERVAL, FOREGROUND_INTERVAL, BACKROUND_INTERVAL);
            Assert.AreEqual(INITIAL_INTERVAL, handler.updateInterval);

            handler.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.FOREGROUND);
            Assert.AreEqual(INITIAL_INTERVAL, handler.updateInterval, "It should be using INITIAL_INTERVAL interval waiting for the initial fetch");

            handler.isFirstFetch = false;
            handler.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.FOREGROUND);
            Assert.AreEqual(FOREGROUND_INTERVAL, handler.updateInterval);

            handler.SetUpdateMode(MapGlobalUsersPositionMarkerController.UpdateMode.BACKGROUND);
            Assert.AreEqual(BACKROUND_INTERVAL, handler.updateInterval);
        }

        [Test]
        public void PlayerPositionShouldSetCorrectly()
        {
            Vector2Int FIRST_POSITION = new Vector2Int(0, 0);
            Vector2Int SECOND_POSITION = new Vector2Int(70, -135);
            Vector2Int THIRD_POSITION = new Vector2Int(-34, -495);

            UserPositionHandler handler = new UserPositionHandler();

            CommonScriptableObjects.playerCoords.Set(FIRST_POSITION);
            Assert.AreEqual(FIRST_POSITION, handler.playerCoords);

            CommonScriptableObjects.playerCoords.Set(SECOND_POSITION);
            Assert.AreEqual(SECOND_POSITION, handler.playerCoords);

            CommonScriptableObjects.playerCoords.Set(THIRD_POSITION);
            Assert.AreEqual(THIRD_POSITION, handler.playerCoords);

            handler.Dispose();
        }

        [Test]
        public void MarkersShouldSetCorrectly()
        {
            const int MAX_MARKERS = 5;
            const int EXCLUSION_AREA = 1;

            var markerPrefab = new GameObject().AddComponent<UserMarkerObject>();
            var overlay = new GameObject();

            Func<Vector2Int, Vector2> coordToMapPosFunc = (v) => { return new Vector2(v.x, v.y); };

            // create scenes to test
            var scenes = new List<HotScenesController.HotSceneInfo>();
            scenes.Add(CreateSceneInfo(new Vector2Int(0, 0)));
            scenes.Add(CreateSceneInfo(new Vector2Int(3, 4)));
            scenes.Add(CreateSceneInfo(new Vector2Int(-4, -4)));

            // create handler
            MarkersHandler handler = new MarkersHandler(markerPrefab, overlay.transform, MAX_MARKERS, coordToMapPosFunc);
            Assert.AreEqual(MAX_MARKERS, handler.availableMarkers.Count);
            Assert.AreEqual(0, handler.usedMarkers.Count);

            // set exclusion area and set markers for scenes
            handler.SetExclusionArea(Vector2Int.zero, EXCLUSION_AREA);
            handler.SetMarkers(scenes);

            // check pools count and marker hidden by exclusion area
            Assert.AreEqual(MAX_MARKERS - scenes.Count, handler.availableMarkers.Count);
            Assert.AreEqual(scenes.Count, handler.usedMarkers.Count);
            Assert.AreEqual(scenes.Count - 1, GetActiveGameObjectsInParent(overlay.transform), "A marker should be hidden by exclusion area");

            // move exclusion area and check markers hidden by exclusion area
            handler.SetExclusionArea(new Vector2Int(6, 6), EXCLUSION_AREA);
            Assert.AreEqual(scenes.Count, GetActiveGameObjectsInParent(overlay.transform), "All markers should be visible");

            // remove a scene and check pools count
            scenes.RemoveAt(0);
            handler.SetMarkers(scenes);
            Assert.AreEqual(MAX_MARKERS - scenes.Count, handler.availableMarkers.Count);
            Assert.AreEqual(scenes.Count, handler.usedMarkers.Count);

            handler.Dispose();
            UnityEngine.Object.Destroy(markerPrefab);
            UnityEngine.Object.Destroy(overlay);
        }

        private HotScenesController.HotSceneInfo CreateSceneInfo(Vector2Int coords)
        {
            return new HotScenesController.HotSceneInfo()
            {
                baseCoords = coords,
                realms = new HotScenesController.HotSceneInfo.Realm[]
                {
                    new HotScenesController.HotSceneInfo.Realm()
                    {
                        userParcels = new Vector2Int[] { coords },
                        usersCount = 1
                    }
                },
                usersTotalCount = 1
            };
        }

        private int GetActiveGameObjectsInParent(Transform parent)
        {
            int result = 0;
            foreach (Transform t in parent)
            {
                if (t.gameObject.activeSelf)
                {
                    result++;
                }
            }

            return result;
        }
    }
}