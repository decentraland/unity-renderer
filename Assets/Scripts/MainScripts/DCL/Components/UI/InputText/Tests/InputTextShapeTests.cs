using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Components.UI;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class InputTextShapeTests : TestsBase
    {
        [UnityTest]
        public IEnumerator InputTextCreate()
        {
            yield return InitScene(spawnCharController: false);

            ScreenSpaceShape ssshape = TestHelpers.SharedComponentCreate<ScreenSpaceShape, ScreenSpaceShape.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return ssshape.routine;

            GameObject go = new GameObject("Mock camera");
            Camera c = go.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.Color;
            c.backgroundColor = Color.black;
            scene.uiScreenSpaceCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            scene.uiScreenSpaceCanvas.worldCamera = c;

            InputText text = TestHelpers.SharedComponentCreate<InputText, InputText.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new InputText.Model()
                {
                    textModel = new DCL.Components.TextShape.Model()
                    {
                        color = Color.white,
                        opacity = 1,
                    },

                    placeholder = "Chat here!",
                    placeholderColor = Color.grey,
                    focusedBackground = Color.black,
                    parentComponent = ssshape.id,
                    positionX = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    positionY = new UIValue(0.5f, UIValue.Unit.PERCENT),
                    height = new UIValue(100),
                    width = new UIValue(100),
                });

            yield return text.routine;

            yield return new WaitForSeconds(5000000);
        }
    }
}
