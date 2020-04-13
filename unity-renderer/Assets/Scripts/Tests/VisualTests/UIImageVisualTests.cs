using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIImageVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [Explicit]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIImageVisualTests_Generate()
    {
        yield return VisualTestHelpers.GenerateBaselineForTest(UIImageTest1());
    }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIImageTest1()
    {
        yield return InitUIVisualTestScene("UIImageTest");

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
            width = new UIValue(128f),
            height = new UIValue(128f)
        }, "imageBack");

        DCLTexture texture = TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
        yield return texture.routine;
        yield return CreateUIComponent<UIImage, UIImage.Model>(CLASS_ID.UI_IMAGE_SHAPE, new UIImage.Model
        {
            parentComponent = mainContainerId,
            source = texture.id,
            width = new UIValue(128f),
            height = new UIValue(128f),
            sourceWidth = 128f,
            sourceHeight = 128f,
            sourceTop = 0f,
            sourceLeft = 0f,
            paddingLeft = 10f,
            paddingRight = 10f,
            paddingTop = 10f,
            paddingBottom = 10f,
            isPointerBlocker = true,
        }, "testUIImage");

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(0f, 2f, 0f));
    }
}
