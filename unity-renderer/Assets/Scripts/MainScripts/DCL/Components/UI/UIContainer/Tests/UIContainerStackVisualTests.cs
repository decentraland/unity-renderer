using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIContainerStackVisualTests : UIVisualTestsBase
{
    [UnityTest]
    [Explicit]
    [VisualTest]
    [Category("Explicit")]
    public IEnumerator UIContainerStackVisualTests_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(UIContainerStackVisualTest()); }

    [UnityTest]
    [VisualTest]
    [Category("Visual Tests")]
    public IEnumerator UIContainerStackVisualTest()
    {
        string mainContainerId = "herodes";
        yield return CreateUIComponent<UIContainerStack, UIContainerStack.Model>(CLASS_ID.UI_CONTAINER_STACK, new UIContainerStack.Model
        {
            parentComponent = screenSpaceId,
            color = Color.green,
            width = new UIValue(50f, UIValue.Unit.PERCENT),
            height = new UIValue(50f, UIValue.Unit.PERCENT)
        }, mainContainerId);

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainerId,
            color = Color.blue,
            width = new UIValue(200f),
            height = new UIValue(200f)
        }, "testContainerRect1");

        DCLTexture texture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/atlas.png");
        yield return texture.routine;
        yield return CreateUIComponent<UIImage, UIImage.Model>(CLASS_ID.UI_IMAGE_SHAPE, new UIImage.Model
        {
            parentComponent = mainContainerId,
            source = texture.id,
            width = new UIValue(100f),
            height = new UIValue(100f),
            positionX = new UIValue(20f),
            sourceWidth = 100f,
            sourceHeight = 100f
        }, "testUIImage");

        string mainContainer2Id = "herodes2";
        yield return CreateUIComponent<UIContainerStack, UIContainerStack.Model>(CLASS_ID.UI_CONTAINER_STACK, new UIContainerStack.Model
        {
            parentComponent = mainContainerId,
            color = Color.red,
            stackOrientation = UIContainerStack.StackOrientation.HORIZONTAL,
            positionX = new UIValue(20f),
            positionY = new UIValue(20f),
            width = new UIValue(200f),
            height = new UIValue(200f)
        }, mainContainer2Id);

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainer2Id,
            color = Color.blue,
            width = new UIValue(200f),
            height = new UIValue(200f),
            positionY = new UIValue(0f)
        }, "testContainerRect2");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainer2Id,
            color = Color.magenta,
            width = new UIValue(200f),
            height = new UIValue(200f),
            positionY = new UIValue(10f)
        }, "testContainerRect3");

        yield return CreateUIComponent<UIContainerRect, UIContainerRect.Model>(CLASS_ID.UI_CONTAINER_RECT, new UIContainerRect.Model
        {
            parentComponent = mainContainer2Id,
            color = Color.yellow,
            width = new UIValue(200f),
            height = new UIValue(200f),
            positionY = new UIValue(20f)
        }, "testContainerRect4");

        yield return VisualTestUtils.TakeSnapshot("UIContainerStackTest", camera, new Vector3(0f, 2f, 0f));
    }
}