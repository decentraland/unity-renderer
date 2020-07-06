using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class BlockerHandlerShould : TestsBase
    {
        protected override bool justSceneSetUp => true;

        [UnityTest]
        public IEnumerator CreateOnlyHullBlockersForBigScenes()
        {
            var go = new GameObject();

            var blockerHandler = new BlockerHandler();
            blockerHandler.SetupBlockers(new HashSet<Vector2Int>(Utils.GetBottomLeftZoneArray(Vector2Int.zero, new Vector2Int(10, 10))), 100, go.transform);

            Assert.AreEqual(36, go.transform.childCount, "Blockers count is unexpected. Remember that blockers only should spawn surrounding the scene, not inside. The spawning code might be broken.");

            blockerHandler.CleanBlockers();

            UnityEngine.Object.Destroy(go);
            yield break;
        }

        [UnityTest]
        public IEnumerator ReleaseBlockersOnUnload()
        {
            var go = new GameObject();

            var blockerHandler = new BlockerHandler();
            blockerHandler.SetupBlockers(new HashSet<Vector2Int>(Utils.GetBottomLeftZoneArray(Vector2Int.zero, new Vector2Int(10, 10))), 100, go.transform);
            yield return null;
            blockerHandler.CleanBlockers();
            yield return null;

            Assert.AreEqual(0, go.transform.childCount, "Blockers couldn't be released properly!");
            UnityEngine.Object.Destroy(go);
        }
    }
}