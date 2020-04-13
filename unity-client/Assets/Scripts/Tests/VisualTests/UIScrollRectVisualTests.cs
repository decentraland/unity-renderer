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
    [Category("Visual Tests")]
    public IEnumerator UIScrollRectVisualTests_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(UIScrollRectTest1());
    }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIScrollRectTest1()
    {
        yield return InitUIVisualTestScene("UIScrollRectTest");

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

        yield return CreateUIComponent<UIInputText, UIInputText.Model>(CLASS_ID.UI_INPUT_TEXT_SHAPE, new UIInputText.Model
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
            textModel = new TextShape.Model
            {
                fontSize = 10f
            }
        }, "textInput");

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, 0f));
    }
}
