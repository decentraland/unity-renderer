using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using DCL;
using System.Linq;
using NUnit.Framework;

namespace Tests
{
    public class FakeBlockerHandler : IBlockerHandler
    {
        public HashSet<Vector2Int> allLoadedParcelCoords = new HashSet<Vector2Int>();

        public void SetupGlobalBlockers(HashSet<Vector2Int> allLoadedParcelCoords, float height, Transform parent)
        {
            this.allLoadedParcelCoords = allLoadedParcelCoords;
        }

        public void CleanBlockers()
        {
            allLoadedParcelCoords.Clear();
        }

        public Dictionary<Vector2Int, PoolableObject> GetBlockers()
        {
            return null;
        }
    }

    public class FakeSceneHandler : ISceneHandler
    {
        public HashSet<Vector2Int> GetAllLoadedScenesCoords()
        {
            var allLoadedParcelCoords = new HashSet<Vector2Int>();
            allLoadedParcelCoords.Add(new Vector2Int(0, 0));
            allLoadedParcelCoords.Add(new Vector2Int(-1, 0));
            allLoadedParcelCoords.Add(new Vector2Int(-1, 1));

            return allLoadedParcelCoords;
        }
    }

    public class BlockerHandlerShould
    {
        IBlockerHandler blockerHandler;
        ISceneHandler sceneHandler;
        GameObject blockersParent;

        [SetUp]
        protected void SetUp()
        {
            sceneHandler = new FakeSceneHandler();
            blockerHandler = new BlockerHandler(new DCLCharacterPosition());
            blockersParent = new GameObject();
        }

        [TearDown]
        protected void TearDown()
        {
            Object.Destroy(blockersParent);
        }

        [Test]
        public void PutBlockersAroundExplorableArea()
        {
            blockerHandler.SetupGlobalBlockers(sceneHandler.GetAllLoadedScenesCoords(), 10, blockersParent.transform);
            var blockers = blockerHandler.GetBlockers();

            Assert.AreEqual(blockers.Count(), 12);
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 0)));
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(-1, 0)));
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(-1, 1)));

            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, 2)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 2)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 2)));
        }

        [Test]
        public void ClearOnlyChangedBlockers()
        {
            blockerHandler.SetupGlobalBlockers(sceneHandler.GetAllLoadedScenesCoords(), 10, blockersParent.transform);
            var blockers = blockerHandler.GetBlockers();

            // Save instance id of some blockers that shouldn't change on the next scene load
            var targetBlocker1InstanceId = blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID();
            var targetBlocker2InstanceId = blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID();
            var targetBlocker3InstanceId = blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID();

            // check blocker that will get removed on next scene load
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

            // Load 2nd scene next to the first one
            var newTotalLoadedCoords = new HashSet<Vector2Int>();
            newTotalLoadedCoords.Add(new Vector2Int(0, 1));
            newTotalLoadedCoords.Add(new Vector2Int(1, 1));
            newTotalLoadedCoords.Add(new Vector2Int(1, 2));
            newTotalLoadedCoords.UnionWith(sceneHandler.GetAllLoadedScenesCoords());

            blockerHandler.SetupGlobalBlockers(newTotalLoadedCoords, 10, blockersParent.transform);
            blockers = blockerHandler.GetBlockers();

            // Check some non-changed blockers:
            Assert.IsTrue(blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID() == targetBlocker1InstanceId);
            Assert.IsTrue(blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID() == targetBlocker2InstanceId);
            Assert.IsTrue(blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID() == targetBlocker3InstanceId);

            // Check removed blocker
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
        }
    }

    public class BlockersControllerShould
    {
        WorldBlockersController blockerController;
        FakeBlockerHandler blockerHandler;
        GameObject blockersParent;

        [SetUp]
        protected void SetUp()
        {
            blockerHandler = new FakeBlockerHandler();
            blockersParent = new GameObject();
            blockerController = new WorldBlockersController(new FakeSceneHandler(), blockerHandler, new DCLCharacterPosition());
        }

        [TearDown]
        protected void TearDown()
        {
            Object.Destroy(blockersParent);
        }

        [Test]
        public void SetupBlockersOnlyWhenEnabled()
        {
            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 3);

            blockerController.SetEnabled(false);
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 0);

            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 0);

            blockerController.SetEnabled(true);
            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 3);
        }
    }
}
