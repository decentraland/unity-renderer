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
            var sceneController = TestHelpers.InitializeSceneController(true);

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
                classId = (int)DCL.Models.CLASS_ID.TEXT_SHAPE,
                json = JsonUtility.ToJson(textShapeModel)
            })) as TextShape;

            yield return new WaitForSeconds(0.01f);

            Canvas canvas = textShape.GetComponentInChildren<Canvas>();
            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();

            Assert.NotNull(textShape, "Component creation fail!");
            Assert.NotNull(canvas, "Canvas doesn't exists for TextShape!");
            Assert.NotNull(tmpro, "TextMeshPro doesn't exists for TextShape!");
            Assert.NotNull(textShape.text, "Unity Text component doesn't exists for TextShape!");


            yield return new WaitForSeconds(0.1f);

            TMProConsistencyAsserts(tmpro, textShapeModel);

            textShapeModel.paddingLeft = 5;
            textShapeModel.paddingRight = 15;
            textShapeModel.value = "Hello world again!";

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "textShapeTest",
                classId = (int)DCL.Models.CLASS_ID.TEXT_SHAPE,
                json = JsonUtility.ToJson(textShapeModel)
            }));

            yield return new WaitForSeconds(0.1f);

            TMProConsistencyAsserts(tmpro, textShapeModel);
            
            yield return null;
        }

        void TMProConsistencyAsserts(TextMeshPro tmpro, TextShape.Model model)
        {
            Assert.AreEqual(model.paddingLeft, tmpro.margin[0], 0.01, string.Format("Left margin must be {0}", model.paddingLeft) );
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

    }
}
