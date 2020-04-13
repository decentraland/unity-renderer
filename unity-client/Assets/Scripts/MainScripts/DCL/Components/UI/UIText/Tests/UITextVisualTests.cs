using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UITextVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [Explicit]
    [Category("Visual Tests")]
    public IEnumerator UITextVisualTests_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(UITextTest1());
    }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UITextTest1()
    {
        yield return InitUIVisualTestScene("UITextTest");

        var uiInputText = TestHelpers.SharedComponentCreate<UIText, UIText.Model>(scene, CLASS_ID.UI_TEXT_SHAPE);
        yield return uiInputText.routine;

        yield return TestHelpers.SharedComponentUpdate(uiInputText, new UIText.Model
        {
            parentComponent = screenSpaceId,
            name = "testUIText",
            isPointerBlocker = true,
            hAlign = "center",
            vAlign = "center",
            positionX = new UIValue(0f),
            positionY = new UIValue(0f),
            visible = true,
            textModel = new TextShape.Model
            {
                paddingLeft = 10,
                paddingRight = 10,
                paddingTop = 10,
                paddingBottom = 10,
                outlineWidth = 0.1f,
                outlineColor = Color.green,
                color = Color.red,
                fontSize = 100,
                fontWeight = "normal",
                opacity = 1,
                value = "Hello World!",
                lineSpacing = 0,
                lineCount = 0,
                adaptWidth = true,
                adaptHeight = true,
                textWrapping = false,
                shadowBlur = 0,
                shadowOffsetX = 0,
                shadowOffsetY = 0,
                shadowColor = Color.black,
                hTextAlign = "center",
                vTextAlign = "center",
                width = 300,
                height = 200,
            }
        });

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, 0f));
    }
}
