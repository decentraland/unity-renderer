using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UIScreenSpaceTests : UITestsBase
    {
        [UnityTest]
        public IEnumerator TestVisibilityUpdate()
        {
            yield return InitScene();

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Canvas canvas = screenSpaceShape.canvas;

            // Check visibility
            Assert.IsTrue(canvas.enabled,
                "When the character is inside the scene, the UIScreenSpaceShape should be visible");

            // Update canvas visibility value manually
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new UIScreenSpace.Model
                {
                    visible = false
                })
            })) as UIScreenSpace;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsFalse(canvas.enabled,
                "When the UIScreenSpaceShape is explicitly updated as 'invisible', its canvas shouldn't be visible");

            // Re-enable visibility
            screenSpaceShape = scene.SharedComponentUpdate(JsonUtility.ToJson(new SharedComponentUpdateMessage
            {
                id = screenSpaceShape.id,
                json = JsonUtility.ToJson(new UIScreenSpace.Model
                {
                    visible = true
                })
            })) as UIScreenSpace;

            yield return screenSpaceShape.routine;

            // Check visibility
            Assert.IsTrue(canvas.enabled,
                "When the UIScreenSpaceShape is explicitly updated as 'visible', its canvas should be visible");

            // Position character outside parcel
            SetCharacterPosition(new Vector3(100, 3f, 100f));

            yield return null;

            // Check visibility
            Assert.IsFalse(canvas.enabled,
                "When the character is outside the scene, the UIScreenSpaceShape shouldn't be visible");

            yield return new WaitForAllMessagesProcessed();

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestScaleWhenCharacterIsElsewhere()
        {
            yield return InitScene();

            // Position character outside parcel
            SetCharacterPosition(new Vector3(50f, 3f, 50f));

            yield return null;

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            RectTransform canvasRectTransform = screenSpaceShape.canvas.GetComponent<RectTransform>();

            const float diffThreshold = 0.1f; //to ensure float point comparison
            Vector2 canvasRealSize = canvasRectTransform.sizeDelta * canvasRectTransform.localScale;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            //Canvas should have the same size of the screen
            Assert.IsTrue(Mathf.Abs((canvasRealSize - screenSize).magnitude) < diffThreshold);

            yield return new WaitForAllMessagesProcessed();

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();
            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<UIScreenSpace.Model, UIScreenSpace>(scene,
                CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        }

        [UnityTest]
        public IEnumerator TestConstrainedPanelMaskAppliesToParcelsUI()
        {
            yield return InitScene();

            yield return null;

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsFalse(scene.isPersistent);
            Assert.IsTrue(screenSpaceShape.childHookRectTransform.GetComponent<UnityEngine.UI.RectMask2D>() != null);

            screenSpaceShape.Dispose();
        }

        [UnityTest]
        public IEnumerator TestConstrainedPanelMaskDoesntApplyToDecentralandUI()
        {
            yield return InitScene();

            scene.isPersistent = true;

            yield return null;

            // Create UIScreenSpaceShape
            UIScreenSpace screenSpaceShape =
                TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return screenSpaceShape.routine;

            Assert.IsTrue(scene.isPersistent);
            Assert.IsTrue(screenSpaceShape.childHookRectTransform.GetComponent<UnityEngine.UI.RectMask2D>() == null);

            screenSpaceShape.Dispose();
        }
    }
}