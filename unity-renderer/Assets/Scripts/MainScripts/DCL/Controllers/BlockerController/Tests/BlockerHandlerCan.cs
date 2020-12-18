using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using DCL;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

public class BlockerHandlerCan
{
    private IBlockerInstanceHandler blockerInstanceHandler;
    private IBlockerAnimationHandler blockerAnimationHandler;
    private ISceneHandler sceneHandler;
    private GameObject blockersParent;

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

        blockerAnimationHandler = Substitute.For<IBlockerAnimationHandler>();

        //NOTE(Brian): Call OnFinish() when blockerAnimationHandler.FadeOut is called. 
        blockerAnimationHandler.FadeOut(Arg.Any<GameObject>(), Arg.Invoke());

        blockerInstanceHandler = new BlockerInstanceHandler(blockerAnimationHandler);

        blockersParent = new GameObject();

        blockerInstanceHandler.SetParent(blockersParent.transform);
    }

    [TearDown]
    protected void TearDown()
    {
        Object.Destroy(blockersParent);
    }

    [Test]
    public void ShowBlocker()
    {
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0));
        blockerAnimationHandler.ReceivedWithAnyArgs(1).FadeIn(default);
        Assert.AreEqual(1, blockerInstanceHandler.GetBlockers().Count);
    }

    [Test]
    public void HideBlocker()
    {
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0), true);
        blockerInstanceHandler.HideBlocker(new Vector2Int(0, 0), false);
        blockerAnimationHandler.ReceivedWithAnyArgs(1).FadeOut(default, default);
        Assert.AreEqual(0, blockerInstanceHandler.GetBlockers().Count);
    }

    [Test]
    public void ShowBlockerInstantly()
    {
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0), true);
        blockerAnimationHandler.DidNotReceiveWithAnyArgs().FadeIn(default);
        Assert.AreEqual(1, blockerInstanceHandler.GetBlockers().Count);
    }

    [Test]
    public void HideBlockerInstantly()
    {
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0), true);
        blockerInstanceHandler.HideBlocker(new Vector2Int(0, 0), true);
        blockerAnimationHandler.DidNotReceiveWithAnyArgs().FadeOut(default, default);
        Assert.AreEqual(0, blockerInstanceHandler.GetBlockers().Count);
    }

    [Test]
    public void SetParent()
    {
        // Arrange
        GameObject testParent = new GameObject();
        blockerInstanceHandler.SetParent(testParent.transform);
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0), true);

        // Act
        var blocker = blockerInstanceHandler.GetBlockers().First().Value.gameObject;

        // Assert
        Assert.IsTrue(blocker.transform.parent == testParent.transform);

        // Dispose
        Object.Destroy(testParent);
    }

    [Test]
    public void DestroyAllBlockers()
    {
        // Arrange
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 0), true);
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 1), true);
        blockerInstanceHandler.ShowBlocker(new Vector2Int(0, 2), true);

        Assert.AreEqual(3, blockerInstanceHandler.GetBlockers().Count);

        // Act
        blockerInstanceHandler.DestroyAllBlockers();

        // Assert
        blockerAnimationHandler.DidNotReceiveWithAnyArgs().FadeOut(default, default);
        Assert.AreEqual(0, blockerInstanceHandler.GetBlockers().Count);
    }
}