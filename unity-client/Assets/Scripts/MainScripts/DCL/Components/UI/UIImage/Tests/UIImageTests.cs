using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UIImageTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIImage
            UIImage uiImageShape =
                TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImageShape.routine;

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.IsTrue(uiImageShape.referencesContainer.image.color == new Color(1, 1, 1, 1));
            Assert.IsTrue(uiImageShape.referencesContainer.canvasGroup.blocksRaycasts);
            Assert.AreEqual(100f, uiImageShape.childHookRectTransform.rect.width);
            Assert.AreEqual(50f, uiImageShape.childHookRectTransform.rect.height);
            Assert.IsTrue(uiImageShape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageShape.referencesContainer.image.texture == null);

            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect);
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());

            // Update UIImage properties
            DCLTexture texture =
                TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
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
            });

            // Check default properties are applied correctly
            Assert.IsTrue(uiImageShape.referencesContainer.transform.parent == screenSpaceShape.childHookRectTransform);
            Assert.AreEqual(new Color(1, 1, 1, 1), uiImageShape.referencesContainer.image.color);
            Assert.IsTrue(uiImageShape.referencesContainer.canvasGroup.blocksRaycasts);

            Assert.AreEqual(108f, uiImageShape.childHookRectTransform.rect.width); // 128 - 20 (left and right padding)
            Assert.AreEqual(108f, uiImageShape.childHookRectTransform.rect.height); // 128 - 20 (left and right padding)
            Assert.IsTrue(uiImageShape.referencesContainer.image.enabled);
            Assert.IsTrue(uiImageShape.referencesContainer.image.texture != null);

            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "right");
            alignedPosition += new Vector2(-65f, 40f); // affected by padding

            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());

            Assert.AreEqual(0.5f, uiImageShape.referencesContainer.image.uvRect.x);
            Assert.AreEqual(0f, uiImageShape.referencesContainer.image.uvRect.y);
            Assert.AreEqual(0.5f, uiImageShape.referencesContainer.image.uvRect.width);
            Assert.AreEqual(0.5f, uiImageShape.referencesContainer.image.uvRect.height);

            Assert.AreEqual(10, uiImageShape.referencesContainer.paddingLayoutGroup.padding.bottom);
            Assert.AreEqual(10, uiImageShape.referencesContainer.paddingLayoutGroup.padding.top);
            Assert.AreEqual(10, uiImageShape.referencesContainer.paddingLayoutGroup.padding.left);
            Assert.AreEqual(10, uiImageShape.referencesContainer.paddingLayoutGroup.padding.right);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestMissingValuesGetDefaultedOnUpdate()
        {
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIImage.Model, UIImage>(scene,
                CLASS_ID.UI_IMAGE_SHAPE);
        }

        [UnityTest]
        public IEnumerator AddedCorrectlyOnInvisibleParent()
        {
            yield return TestHelpers.TestUIElementAddedCorrectlyOnInvisibleParent<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
        }

        [UnityTest]
        public IEnumerator TestOnClickEvent()
        {
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture =
                TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            yield return TestHelpers.SharedComponentUpdate(uiImage, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                onClick = uiImageOnClickEventId
            });

            bool eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiImageOnClickEventId,
                (RectTransform) uiImage.referencesContainer.image.transform,
                (bool res) =>
                {
                    // Check image object clicking triggers the correct event
                    eventResult = res;
                });

            Assert.IsTrue(eventResult);

            // Check UI children won't trigger the parent/root image component event
            UIContainerRect uiContainer =
                TestHelpers.SharedComponentCreate<UIContainerRect, UIContainerRect.Model>(scene,
                    CLASS_ID.UI_CONTAINER_RECT);
            yield return uiContainer.routine;

            yield return TestHelpers.SharedComponentUpdate(uiContainer, new UIContainerRect.Model
            {
                parentComponent = uiImage.id
            });

            eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiImageOnClickEventId,
                (RectTransform) uiContainer.referencesContainer.image.transform,
                (bool res) =>
                {
                    // Check image object clicking doesn't trigger event
                    eventResult = res;
                });

            Assert.IsFalse(eventResult);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestOnClickOnInvisibleShapeEvent()
        {
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture = TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // --------------------------------------------------------------------------------------
            // Visible image that should trigger click events
            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            yield return TestHelpers.SharedComponentUpdate(uiImage, new UIImage.Model
            {
                visible = true,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                onClick = uiImageOnClickEventId
            });

            // --------------------------------------------------------------------------------------
            // Invisible image
            // Create an invisible image to check that it doesn't trigger click events
            // and that doesn't prevent previous image from triggering click events
            UIImage uiImage2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage2.routine;

            yield return TestHelpers.SharedComponentUpdate(uiImage2, new UIImage.Model
            {
                visible = false,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),

                onClick = uiImageOnClickEventId
            });

            // --------------------------------------------------------------------------------------
            // We need to cast a ray to check clicked objects
            Assert.IsFalse(TestHelpers.TestUIClick(screenSpaceShape.canvas, uiImage2.referencesContainer.childHookRectTransform));
            Assert.IsTrue(TestHelpers.TestUIClick(screenSpaceShape.canvas, uiImage.referencesContainer.childHookRectTransform));
        }

        [UnityTest]
        public IEnumerator TestOnClickOnTransparentShapeEvent()
        {
            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture = TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // --------------------------------------------------------------------------------------
            // Visible image that should trigger click events
            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            yield return TestHelpers.SharedComponentUpdate(uiImage, new UIImage.Model
            {
                visible = true,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                sourceLeft = 0,
                sourceTop = 0,
                sourceWidth = 1,
                sourceHeight = 1,
                onClick = uiImageOnClickEventId
            });

            // --------------------------------------------------------------------------------------
            // Invisible image
            // Create an invisible image to check that it doesn't trigger click events
            // and that doesn't prevent previous image from triggering click events
            UIImage uiImage2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage2.routine;

            yield return TestHelpers.SharedComponentUpdate(uiImage2, new UIImage.Model
            {
                visible = true,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                sourceLeft = 0,
                sourceTop = 0,
                sourceWidth = 1,
                sourceHeight = 1,
                opacity = 0f,
                onClick = uiImageOnClickEventId
            });

            // --------------------------------------------------------------------------------------
            // We need to cast a ray to check clicked objects
            Canvas canvas = screenSpaceShape.canvas;

            Assert.IsTrue(TestHelpers.TestUIClick(canvas, uiImage2.referencesContainer.childHookRectTransform));
        }

        [UnityTest]
        public IEnumerator TestAlignment()
        {
            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            // Create UIImage
            UIImage uiImageShape =
                TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImageShape.routine;

            DCLTexture texture =
                TestHelpers.CreateDCLTexture(scene, DCL.Helpers.Utils.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // Align to right-bottom
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "bottom"
            });

            // Check alignment position was applied correctly
            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to right-center
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "center"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to right-top
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "top"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "top", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-bottom
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "bottom"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-center
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "center"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-top
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "top"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "top", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-bottom
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "bottom"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerLeft, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-center
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "center"
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "left");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleLeft, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-top
            yield return TestHelpers.SharedComponentUpdate(uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "top",
            });

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "top", "left");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperLeft, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            screenSpaceShape.Dispose();
        }
    }
}