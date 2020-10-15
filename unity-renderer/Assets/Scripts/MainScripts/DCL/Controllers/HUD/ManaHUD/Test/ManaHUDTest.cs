using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ManaHUDTest : TestsBase
{
    private ManaHUDController controller;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = new ManaHUDController();
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator CreateView()
    {
        Assert.NotNull(controller.view);
        Assert.NotNull(controller.view.gameObject);
        yield break;
    }

    [UnityTest]
    public IEnumerator HideAndShowWithUI()
    {
        CanvasGroup canvasGroup = controller.view.GetComponent<CanvasGroup>();
        Assert.NotNull(canvasGroup, "CanvasGroup is null");
        Assert.IsTrue(canvasGroup.alpha != 1, "CanvasGroup alpha == 1, should started not visible");

        CommonScriptableObjects.allUIHidden.Set(true);
        Assert.IsTrue(canvasGroup.alpha == 0, "CanvasGroup alpha != 0, should be hidden");
        Assert.IsFalse(canvasGroup.interactable, "CanvasGroup is interactable, should be hidden");

        CommonScriptableObjects.allUIHidden.Set(false);
        Assert.IsTrue(canvasGroup.alpha == 1, "CanvasGroup alpha != 1, should be visible");
        Assert.IsTrue(canvasGroup.interactable, "CanvasGroup is not interactable, should be visible");
        yield break;
    }
}
