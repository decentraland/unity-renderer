using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIScrollRectVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator UIScrollRectVisualTests_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(UIScrollRectTest1()); }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIScrollRectTest1()
    {
        yield return CreateUIComponent<UIScrollRect, UIScrollRect.Model>(CLASS_ID.UI_SLIDER_SHAPE, new UIScrollRect.Model
        {
            parentComponent = screenSpaceId,
            backgroundColor = Color.gray,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT),
            isVertical = true
        }, "herodes");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = screenSpaceId,
            color = Color.clear,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT),
            isPointerBlocker = false
        }, "testContainer");

        yield return CreateUIComponent<UIInputText, Mock_UIInputTextModel>(CLASS_ID.UI_INPUT_TEXT_SHAPE, new Mock_UIInputTextModel
        {
            parentComponent = screenSpaceId,
            width = new UIValue(80f, UIValue.Unit.PERCENT),
            height = new UIValue(25f),
            vAlign = "bottom",
            hAlign = "left",
            placeholder = "Write message here",
            placeholderColor = Color.gray,
            positionX = new UIValue(10f, UIValue.Unit.PERCENT),
            positionY = new UIValue(10f),
            fontSize = 10f
        }, "textInput");

        yield return VisualTestUtils.TakeSnapshot("UIScrollRectTest", camera, new Vector3(0f, 2f, 0f));
    }

    // Our current architecture for UIInputText/UIText does not represent the one from the SDK
    // so we are forced to use a mock model that contains what we need.
    // Further information: https://github.com/decentraland/unity-renderer/issues/805
    private class Mock_UIInputTextModel : UIInputText.Model
    {
        public float fontSize;
    }
}