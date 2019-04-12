using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UIImageTests : TestsBase
    {
        [UnityTest]
        public IEnumerator UIImagePropertiesAreAppliedCorrectly()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            UIImage uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as UIImage;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model { })
            }));

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(uiImageshape.referencesContainer.image.color == new Color(1, 1, 1, 1));
            Assert.IsTrue(uiImageshape.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(100f, uiImageshape.childHookRectTransform.rect.width);
            Assert.AreEqual(100f, uiImageshape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture == null);

            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect);
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

            // Update UIContainerRectShape properties
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    opacity = 0.7f,
                    isPointerBlocker = true,
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    positionX = new UIValue(-55f),
                    positionY = new UIValue(30f),
                    hAlign = "right",
                    vAlign = "bottom",
                    sourceLeft = 64f,
                    sourceTop = 64f,
                    sourceWidth = 64f,
                    sourceHeight = 64f,
                    paddingTop = 10f,
                    paddingRight = 10f,
                    paddingBottom = 10f,
                    paddingLeft = 10f
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageshape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.AreEqual(new Color(1, 1, 1, 1), uiImageshape.referencesContainer.image.color);
            Assert.IsTrue(uiImageshape.referencesContainer.canvasGroup.blocksRaycasts);

            Assert.AreEqual(108f, uiImageshape.childHookRectTransform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageshape.childHookRectTransform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageshape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageshape.referencesContainer.image.texture != null);

            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-65f, 40f); // affected by padding

            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());

            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.x);
            Assert.AreEqual(0f, uiImageshape.referencesContainer.image.uvRect.y);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.width);
            Assert.AreEqual(0.5f, uiImageshape.referencesContainer.image.uvRect.height);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.bottom);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.top);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.left);
            Assert.AreEqual(10, uiImageshape.referencesContainer.paddingLayoutGroup.padding.right);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator UIImageMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIImage.Model, UIImage>(scene, CLASS_ID.UI_IMAGE_SHAPE);
        }


        [UnityTest]
        public IEnumerator UIShapeAlign()
        {
            yield return InitScene();

            DCLCharacterController.i.gravity = 0f;

            // Position character inside parcel (0,0)
            DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(new
            {
                x = 0f,
                y = 0f,
                z = 0f
            }));

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            // Create UIContainerRectShape
            string uiImageShapeId = "uiImageShape";

            UIImage uiImageshape = scene.SharedComponentCreate(JsonUtility.ToJson(new SharedComponentCreateMessage
            {
                classId = (int)CLASS_ID.UI_IMAGE_SHAPE,
                id = uiImageShapeId,
                name = "UIImageShape"
            })) as UIImage;

            yield return uiImageshape.routine;

            // Align to right-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    parentComponent = screenSpaceShape.id,
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "bottom",
                })
            })) as UIImage;

            // waiting for the texture fetching
            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            Vector2 alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "bottom", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "center",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "center", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to right-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "right",
                    vAlign = "top",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "top", "right");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperRight, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "bottom",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "bottom", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "center",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "center", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to center-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "center",
                    vAlign = "top",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "top", "center");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperCenter, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-bottom
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "bottom",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-center
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "center",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "center", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            // Align to left-top
            uiImageshape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = uiImageShapeId,
                json = JsonUtility.ToJson(new UIImage.Model
                {
                    source = TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png",
                    width = new UIValue(128f),
                    height = new UIValue(128f),
                    hAlign = "left",
                    vAlign = "top",
                })
            })) as UIImage;

            yield return uiImageshape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedPosition(screenSpaceShape.childHookRectTransform.rect, uiImageshape.childHookRectTransform.rect, "top", "left");
            Assert.AreEqual(alignedPosition.ToString(), uiImageshape.referencesContainer.paddingLayoutRectTransform.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperLeft, uiImageshape.referencesContainer.alignmentLayoutGroup.childAlignment);

            screenSpaceShape.Dispose();
        }

    }
}
