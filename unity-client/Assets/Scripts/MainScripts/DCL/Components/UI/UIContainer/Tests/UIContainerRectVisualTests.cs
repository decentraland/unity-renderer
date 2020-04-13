using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIContainerRectVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [Explicit]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIContainerRectVisualTests_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(UIContainerRectTest1());
    }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIContainerRectTest1()
    {
        yield return InitUIVisualTestScene("UIContainerRectTest");

        string mainContainerId = "herodes";
        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = screenSpaceId,
            color = Color.green,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT)
        }, mainContainerId);

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(100f),
            hAlign = "left",
            vAlign = "top"
        }, "innerPanelTopLeft");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(100f),
            hAlign = "left"
        }, "innerPanelCenterLeft");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            hAlign = "left",
            vAlign = "bottom"
        }, "innerPanelBottomLeft");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            vAlign = "top"
        }, "innerPanelTopCenter");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            vAlign = "top"
        }, "innerPanelTopCenter");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f),
            height = new UIValue(25f)
        }, "innerPanelCenterCenter");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            vAlign = "bottom"
        }, "innerPanelBottomCenter");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(100f),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            hAlign = "right",
            vAlign = "top"
        }, "innerPanelTopRight");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            hAlign = "right"
        }, "innerPanelCenterRight");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            width = new UIValue(25f, UIValue.Unit.PERCENT),
            height = new UIValue(25f, UIValue.Unit.PERCENT),
            hAlign = "right",
            vAlign = "bottom"
        }, "innerPanelBottomRight");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.blue,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT),
            hAlign = "right",
            vAlign = "bottom",
            positionX = new UIValue(-20f, UIValue.Unit.PERCENT),
            positionY = new UIValue(20f, UIValue.Unit.PERCENT)
        }, "innerPanel1");

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, 0f));
    }
}
