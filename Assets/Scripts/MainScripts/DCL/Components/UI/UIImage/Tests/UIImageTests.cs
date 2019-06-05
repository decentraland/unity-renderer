using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Tests
{
    public class UIImageTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestPropertiesAreAppliedCorrectly()
        {
            yield return InitScene();

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
                TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
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
            yield return uiImageShape.routine;

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
            yield return InitScene();

            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIImage.Model, UIImage>(scene,
                CLASS_ID.UI_IMAGE_SHAPE);
        }

        [UnityTest]
        public IEnumerator TestOnclickEvent()
        {
            yield return InitScene();

            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture =
                TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            TestHelpers.SharedComponentUpdate(scene, uiImage, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                onClick = uiImageOnClickEventId
            });

            yield return uiImage.routine;

            bool eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiImageOnClickEventId,
                (RectTransform)uiImage.referencesContainer.image.transform,
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

            TestHelpers.SharedComponentUpdate(scene, uiContainer, new UIContainerRect.Model
            {
                parentComponent = uiImage.id
            });
            yield return uiContainer.routine;

            eventResult = false;

            yield return TestHelpers.TestUIClickEventPropagation(
                scene.sceneData.id,
                uiImageOnClickEventId,
                (RectTransform)uiContainer.referencesContainer.image.transform,
                (bool res) =>
                {
                    // Check image object clicking doesn't trigger event
                    eventResult = res;
                });

            Assert.IsFalse(eventResult);
        }

        [UnityTest]
        public IEnumerator TestOnEnterEvent()
        {
            yield return InitScene();

            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture = TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnEnterUUID = "UUIDFakeEventId";

            TestHelpers.SharedComponentUpdate(scene, uiImage, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                onEnter = uiImageOnEnterUUID
            });
            yield return uiImage.routine;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnEnterEvent>
            {
                sceneId = scene.sceneData.id,
                payload = new WebInterface.OnEnterEvent { uuid = uiImageOnEnterUUID },
                eventType = "uuidEvent"
            };

            bool eventTriggered = false;
            yield return TestHelpers.WaitForMessageFromEngine("SceneEvent", JsonUtility.ToJson(sceneEvent),
                () =>
                {
                    uiImage.referencesContainer.OnEnterPressed();
                },
                () =>
                {
                    eventTriggered = true;
                });

            yield return null;

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator TestOnClickOnInvisibleShapeEvent()
        {
            yield return InitScene();

            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture = TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // --------------------------------------------------------------------------------------
            // Visible image that should trigger click events
            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            TestHelpers.SharedComponentUpdate(scene, uiImage, new UIImage.Model
            {
                visible = true,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                onClick = uiImageOnClickEventId
            });
            yield return uiImage.routine;

            // --------------------------------------------------------------------------------------
            // Invisible image
            // Create an invisible image to check that it doesn't trigger click events
            // and that doesn't prevent previous image from triggering click events
            UIImage uiImage2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage2.routine;

            TestHelpers.SharedComponentUpdate(scene, uiImage2, new UIImage.Model
            {
                visible = false,
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),

                onClick = uiImageOnClickEventId
            });
            yield return uiImage2.routine;

            // --------------------------------------------------------------------------------------
            // We need to cast a ray to check clicked objects
            Canvas canvas = screenSpaceShape.canvas;

            Assert.IsTrue(TestHelpers.TestUIClick(canvas, uiImage.referencesContainer.childHookRectTransform));
            Assert.IsFalse(TestHelpers.TestUIClick(canvas, uiImage2.referencesContainer.childHookRectTransform));
        }

        [UnityTest]
        public IEnumerator TestOnClickOnTransparentShapeEvent()
        {
            yield return InitScene();

            UIScreenSpace screenSpaceShape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            Assert.IsFalse(screenSpaceShape == null);

            DCLTexture texture = TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // --------------------------------------------------------------------------------------
            // Visible image that should trigger click events
            UIImage uiImage = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage.routine;

            string uiImageOnClickEventId = "UUIDFakeEventId";

            TestHelpers.SharedComponentUpdate(scene, uiImage, new UIImage.Model
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
            yield return uiImage.routine;

            // --------------------------------------------------------------------------------------
            // Invisible image
            // Create an invisible image to check that it doesn't trigger click events
            // and that doesn't prevent previous image from triggering click events
            UIImage uiImage2 = TestHelpers.SharedComponentCreate<UIImage, UIImage.Model>(scene, CLASS_ID.UI_IMAGE_SHAPE);
            yield return uiImage2.routine;

            TestHelpers.SharedComponentUpdate(scene, uiImage2, new UIImage.Model
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
            yield return uiImage2.routine;

            // --------------------------------------------------------------------------------------
            // We need to cast a ray to check clicked objects
            Canvas canvas = screenSpaceShape.canvas;

            Assert.IsTrue(TestHelpers.TestUIClick(canvas, uiImage2.referencesContainer.childHookRectTransform));
        }

        [UnityTest]
        public IEnumerator TestAlignment()
        {
            yield return InitScene();

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
                TestHelpers.CreateDCLTexture(scene, TestHelpers.GetTestsAssetsPath() + "/Images/atlas.png");
            yield return texture.routine;

            // Align to right-bottom
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                parentComponent = screenSpaceShape.id,
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "bottom"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            Vector2 alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to right-center
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "center"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to right-top
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "right",
                vAlign = "top"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "top", "right");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperRight, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-bottom
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "bottom"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-center
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "center"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to center-top
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "center",
                vAlign = "top"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "top", "center");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.UpperCenter, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-bottom
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "bottom"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "bottom", "left");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.LowerLeft, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-center
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "center"
            });
            yield return uiImageShape.routine;

            // Check alignment position was applied correctly
            alignedPosition = CalculateAlignedAnchoredPosition(screenSpaceShape.childHookRectTransform.rect,
                uiImageShape.childHookRectTransform.rect, "center", "left");
            Assert.AreEqual(alignedPosition.ToString(),
                uiImageShape.referencesContainer.layoutElementRT.anchoredPosition.ToString());
            Assert.AreEqual(TextAnchor.MiddleLeft, uiImageShape.referencesContainer.layoutGroup.childAlignment);

            // Align to left-top
            TestHelpers.SharedComponentUpdate(scene, uiImageShape, new UIImage.Model
            {
                source = texture.id,
                width = new UIValue(128f),
                height = new UIValue(128f),
                hAlign = "left",
                vAlign = "top",
            });
            yield return uiImageShape.routine;

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