using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using UnityEngine;

public class BlockersControllerShould
{
    private ISceneHandler sceneHandler;
    WorldBlockersController blockerController;
    IBlockerInstanceHandler blockerInstanceHandler;
    GameObject blockersParent;

    [SetUp]
    protected void SetUp()
    {
        RenderProfileManifest.i.Initialize();

        sceneHandler = Substitute.For<ISceneHandler>();

        var allLoadedParcelCoords = new HashSet<Vector2Int>();
        allLoadedParcelCoords.Add(new Vector2Int(0, 0));
        allLoadedParcelCoords.Add(new Vector2Int(-1, 0));
        allLoadedParcelCoords.Add(new Vector2Int(-1, 1));

        sceneHandler.GetAllLoadedScenesCoords().Returns(allLoadedParcelCoords);

        var animationHandler = Substitute.For<IBlockerAnimationHandler>();
        //NOTE(Brian): Call OnFinish() when blockerAnimationHandler.FadeOut is called.
        animationHandler.FadeOut(Arg.Any<GameObject>(), Arg.Invoke());

        var newBlockerInstanceHandler = new BlockerInstanceHandler();
        newBlockerInstanceHandler.Initialize(animationHandler);

        blockerInstanceHandler = newBlockerInstanceHandler;
        blockersParent = new GameObject();
        blockerInstanceHandler.SetParent(blockersParent.transform);

        blockerController = new WorldBlockersController();
        blockerController.Initialize(sceneHandler, blockerInstanceHandler);
    }

    [TearDown]
    protected void TearDown()
    {
        blockerController.Dispose();
        Object.Destroy(blockersParent);
    }

    [Test]
    public void SetupBlockersOnlyWhenEnabled()
    {
        // Arrange
        blockerInstanceHandler = Substitute.For<IBlockerInstanceHandler>();
        blockerInstanceHandler.GetBlockers().Returns(new Dictionary<Vector2Int, PoolableObject>());

        if (blockerController != null)
            blockerController.Dispose();

        blockerController.Initialize(sceneHandler, blockerInstanceHandler);

        // Act-assert #1: first blockers added should be shown
        blockerController.SetupWorldBlockers();
        blockerInstanceHandler.ReceivedWithAnyArgs().ShowBlocker(default);

        blockerInstanceHandler.ClearReceivedCalls();

        // Act-assert #2: if disabled, blockers should be removed
        blockerController.SetEnabled(false);
        blockerInstanceHandler.Received(1).DestroyAllBlockers();

        blockerInstanceHandler.ClearReceivedCalls();

        // Act-assert #3: if disabled, no blockers should be added nor removed
        blockerController.SetupWorldBlockers();
        blockerInstanceHandler.DidNotReceiveWithAnyArgs().ShowBlocker(default);
        blockerInstanceHandler.DidNotReceiveWithAnyArgs().HideBlocker(default, default);

        blockerInstanceHandler.ClearReceivedCalls();

        // Act-assert #4: If enabled again, blockers should be added or removed as needed
        blockerController.SetEnabled(true);
        blockerController.SetupWorldBlockers();
        blockerInstanceHandler.ReceivedWithAnyArgs().ShowBlocker(default);

        blockerController.Dispose();
    }

    [Test]
    public void PutBlockersAroundExplorableArea()
    {
        blockerController.SetupWorldBlockers();
        var blockers = blockerInstanceHandler.GetBlockers();

        Assert.AreEqual(12, blockers.Count);
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
        blockerController.SetupWorldBlockers();
        var blockers = blockerInstanceHandler.GetBlockers();

        // Save instance of some blockers that shouldn't change on the next scene load
        var blocker1 = blockers[new Vector2Int(-1, -1)].gameObject;
        var blocker2 = blockers[new Vector2Int(-2, -1)].gameObject;
        var blocker3 = blockers[new Vector2Int(-2, 0)].gameObject;

        // check blocker that will get removed on next scene load
        Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

        // Load 2nd scene next to the first one
        var newTotalLoadedCoords = new HashSet<Vector2Int>();
        newTotalLoadedCoords.Add(new Vector2Int(0, 1));
        newTotalLoadedCoords.Add(new Vector2Int(1, 1));
        newTotalLoadedCoords.Add(new Vector2Int(1, 2));
        newTotalLoadedCoords.UnionWith(sceneHandler.GetAllLoadedScenesCoords());

        sceneHandler.GetAllLoadedScenesCoords().Returns(newTotalLoadedCoords);
        blockerController.SetupWorldBlockers();

        blockers = blockerInstanceHandler.GetBlockers();

        // Check some non-changed blockers:
        Assert.IsTrue(blockers[new Vector2Int(-1, -1)].gameObject == blocker1);
        Assert.IsTrue(blockers[new Vector2Int(-2, -1)].gameObject == blocker2);
        Assert.IsTrue(blockers[new Vector2Int(-2, 0)].gameObject == blocker3);

        // Check removed blocker
        Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
    }
}