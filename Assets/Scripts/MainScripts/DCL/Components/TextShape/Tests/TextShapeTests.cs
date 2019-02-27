using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TextShapeTests
    {
        [UnityTest]
        public IEnumerator TextShapeCreateTest()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            string entityId = "e1";

            TestHelpers.CreateSceneEntity(scene, entityId);

            yield return new WaitForSeconds(0.01f);

            Debug.Log("Creating/updating text shape...");
            var textShapeModel = new TextShape.Model()
            {
                value = "Hello world!",

                color = Color.white,
                opacity = 0.5f,
                fontSize = 10,
                fontFamily = "",
                fontWeight = "",

                hAlign = 0,
                vAlign = 0,
                width = 20,
                height = 20,
                resizeToFit = false,
                paddingTop = 10,
                paddingRight = 0,
                paddingBottom = 10,
                paddingLeft = 0,
                zIndex = 0,

                shadowBlur = 0,
                shadowOffsetX = 0,
                shadowOffsetY = 0,
                shadowColor = Color.white
            };

            TextShape textShape = scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "textShapeTest",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TEXT_SHAPE,
                json = JsonUtility.ToJson(textShapeModel)
            })) as TextShape;

            yield return new WaitForSeconds(0.01f);

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();

            Assert.IsTrue(textShape != null, "Component creation fail!");
            Assert.IsTrue(tmpro != null, "TextMeshPro doesn't exists for TextShape!");
            Assert.IsTrue(textShape.text != null, "Unity Text component doesn't exists for TextShape!");


            yield return new WaitForSeconds(0.1f);

            TMProConsistencyAsserts(tmpro, textShapeModel);

            textShapeModel.paddingLeft = 5;
            textShapeModel.paddingRight = 15;
            textShapeModel.value = "Hello world again!";

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "textShapeTest",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TEXT_SHAPE,
                json = JsonUtility.ToJson(textShapeModel)
            }));

            yield return new WaitForSeconds(0.1f);

            TMProConsistencyAsserts(tmpro, textShapeModel);

            yield return null;
        }

        void TMProConsistencyAsserts(TextMeshPro tmpro, TextShape.Model model)
        {
            Assert.AreEqual(model.paddingLeft, tmpro.margin[0], 0.01, string.Format("Left margin must be {0}", model.paddingLeft));
            Assert.AreEqual(model.paddingTop, tmpro.margin[1], 0.01, string.Format("Top margin must be {0}", model.paddingTop));
            Assert.AreEqual(model.paddingRight, tmpro.margin[2], 0.01, string.Format("Right margin must be {0}", model.paddingRight));
            Assert.AreEqual(model.paddingBottom, tmpro.margin[3], 0.01, string.Format("Bottom margin must be {0}", model.paddingBottom));

            Assert.IsTrue(tmpro.text == model.value, "Text wasn't set correctly!");
        }

        [UnityTest]
        public IEnumerator TextShapeAlignTests()
        {
            Assert.AreEqual(TextAlignmentOptions.BottomLeft, TextShape.GetAlignment(0, 0));
            Assert.AreEqual(TextAlignmentOptions.BottomRight, TextShape.GetAlignment(1, 0));
            Assert.AreEqual(TextAlignmentOptions.Center, TextShape.GetAlignment(0.5f, 0.5f));
            Assert.AreEqual(TextAlignmentOptions.TopLeft, TextShape.GetAlignment(0, 1));
            Assert.AreEqual(TextAlignmentOptions.TopRight, TextShape.GetAlignment(1, 1));
            Assert.AreEqual(TextAlignmentOptions.Top, TextShape.GetAlignment(0.5f, 1));
            Assert.AreEqual(TextAlignmentOptions.Right, TextShape.GetAlignment(1, 0.5f));
            Assert.AreEqual(TextAlignmentOptions.Bottom, TextShape.GetAlignment(0.5f, 0));
            Assert.AreEqual(TextAlignmentOptions.Left, TextShape.GetAlignment(0, 0.5f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TextShapeComponentMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new TextShape.Model
            {
                color = Color.green,
                width = 0.25f,
                lineCount = 3,
                resizeToFit = true,
                shadowColor = Color.red
            });

            TextShape textShapeComponent = (TextShape)scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TEXT_SHAPE,
                json = componentJSON
            }));

            // 2. Check configured values
            Assert.AreEqual(Color.green, textShapeComponent.model.color);
            Assert.AreEqual(0.25f, textShapeComponent.model.width);
            Assert.AreEqual(3, textShapeComponent.model.lineCount);
            Assert.IsTrue(textShapeComponent.model.resizeToFit);
            Assert.AreEqual(Color.red, textShapeComponent.model.shadowColor);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new TextShape.Model { });

            scene.EntityComponentUpdate(scene.entities[entityId], CLASS_ID_COMPONENT.TEXT_SHAPE, componentJSON);

            // 4. Check defaulted values
            Assert.AreEqual(Color.white, textShapeComponent.model.color);
            Assert.AreEqual(1f, textShapeComponent.model.width);
            Assert.AreEqual(0, textShapeComponent.model.lineCount);
            Assert.IsFalse(textShapeComponent.model.resizeToFit);
            Assert.AreEqual(new Color(1, 1, 1), textShapeComponent.model.shadowColor);
        }
    }
}
