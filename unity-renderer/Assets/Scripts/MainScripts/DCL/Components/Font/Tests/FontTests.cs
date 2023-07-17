using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using DCL.World.PortableExperiences;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

namespace Tests
{
    public class FontTests : IntegrationTestSuite
    {
        const string TEST_BUILTIN_FONT_NAME = "builtin:SF-UI-Text-Regular SDF";

        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override void InitializeServices(ServiceLocator serviceLocator)
        {
            serviceLocator.Register<ISceneController>(() => new SceneController(Substitute.For<IConfirmedExperiencesRepository>()));
            serviceLocator.Register<IWorldState>(() => new WorldState());
            serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
        }

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            coreComponentsPlugin = new CoreComponentsPlugin();
            scene = TestUtils.CreateTestScene();
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator BuiltInFontCreateAndLoadTest()
        {
            DCLFont font =
                TestUtils.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = TEST_BUILTIN_FONT_NAME });
            yield return font.routine;

            var entity = TestUtils.CreateSceneEntity(scene);

            TextShape textShape =
                TestUtils.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model() { font = font.id });
            yield return textShape.routine;

            Assert.IsTrue(font.loaded, "Built-in font didn't load");
            Assert.IsFalse(font.error, "Built-in font has error");

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();
            Assert.IsTrue(font.fontAsset == tmpro.font, "Built-in font didn't apply correctly");
        }

        [UnityTest]
        public IEnumerator BuiltInFontHandleErrorProperly()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TextShape textShape =
                TestUtils.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model());
            yield return textShape.routine;

            TMP_FontAsset defaultFont = textShape.GetComponentInChildren<TextMeshPro>().font;

            DCLFont font =
                TestUtils.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = "no-valid-font" });
            yield return font.routine;

            scene.componentsManagerLegacy.EntityComponentUpdate(entity, CLASS_ID_COMPONENT.TEXT_SHAPE,
                JsonUtility.ToJson(new TextShape.Model { font = font.id }));
            yield return textShape.routine;

            Assert.IsTrue(font.error, "Built-in font error has not araise properly");
            Assert.IsTrue(textShape.GetComponentInChildren<TextMeshPro>().font == defaultFont, "Built-in font didn't apply correctly");
        }

        [UnityTest]
        public IEnumerator BuiltInFontAttachCorrectlyOnTextComponentUpdate()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TextShape textShape =
                TestUtils.EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, new TextShape.Model());
            yield return textShape.routine;

            DCLFont font =
                TestUtils.SharedComponentCreate<DCLFont, DCLFont.Model>(scene, CLASS_ID.FONT, new DCLFont.Model() { src = TEST_BUILTIN_FONT_NAME });
            yield return font.routine;

            scene.componentsManagerLegacy.EntityComponentUpdate(entity, CLASS_ID_COMPONENT.TEXT_SHAPE,
                JsonUtility.ToJson(new TextShape.Model { font = font.id }));
            yield return textShape.routine;

            Assert.IsTrue(font.loaded, "Built-in font didn't load");
            Assert.IsFalse(font.error, "Built-in font has error");

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();
            Assert.IsTrue(font.fontAsset == tmpro.font, "Built-in font didn't apply correctly");
        }
    }
}
