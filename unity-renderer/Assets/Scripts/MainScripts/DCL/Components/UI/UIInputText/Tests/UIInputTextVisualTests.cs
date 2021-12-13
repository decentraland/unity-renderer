using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIInputTextVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [Explicit]
    [VisualTest]
    [Category("Explicit")]
    public IEnumerator UIInputTextVisualTests_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(UIInputTextTest1()); }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIInputTextTest1()
    {
        string mainContainerId = "herodes";
        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = screenSpaceId,
            color = Color.green,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT)
        }, mainContainerId);

        yield return CreateUIComponent<UIInputText, Mock_UIInputTextModel>(CLASS_ID.UI_INPUT_TEXT_SHAPE, new Mock_UIInputTextModel
        {
            parentComponent = mainContainerId,
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

        yield return VisualTestUtils.TakeSnapshot("UIInputTextTest", camera, new Vector3(0f, 2f, 0f));
    }

    // Our current architecture for UIInputText/UIText does not represent the one from the SDK
    // so we are forced to use a mock model that contains what we need.
    // Further information: https://github.com/decentraland/unity-renderer/issues/805
    private class Mock_UIInputTextModel : UIInputText.Model
    {
        public float fontSize;
    }
}