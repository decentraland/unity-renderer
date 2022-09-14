using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSTextShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private GameObject gameObject;
        private ECSTextShapeComponentHandler textShapeComponentHandler;

        [UnitySetUp]
        protected IEnumerator SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            textShapeComponentHandler = new ECSTextShapeComponentHandler(DataStore.i.ecs7, AssetPromiseKeeper_Font.i);

            MeshesInfo meshesInfo = new MeshesInfo();
            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            entity.meshesInfo.Returns(meshesInfo);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            textShapeComponentHandler.OnComponentCreated(scene, entity);
            
            // We update the model here so the asset promise is ready when we start the test, so we can uses TestCases 
            PBTextShape model = CreateModel();
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            yield return textShapeComponentHandler.promise;
        }

        [TearDown]
        protected void TearDown()
        {
            textShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            string result = "TestCorrectly";
            PBTextShape model = CreateModel();
            model.Text = result;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.text, result);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            textShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(textShapeComponentHandler.textComponent);
            Assert.IsNull(textShapeComponentHandler.textGameObject);
            Assert.IsNull(textShapeComponentHandler.rectTransform);
        }
        
        [Test]
        public void DisposeFontCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            textShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsTrue(textShapeComponentHandler.promise.isForgotten);
        }

        [Test]
        public void UpdateTextColorCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            Color3 color = new Color3();
            color.B = 1f;
            color.R = 0f;
            color.B = 0f;

            model.TextColor = color;
            
            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.color.r,color.R);
            Assert.AreEqual(textShapeComponentHandler.textComponent.color.b,color.B);
            Assert.AreEqual(textShapeComponentHandler.textComponent.color.g,color.B);
        }
        
        [Test]
        public void UpdatePaddingTextCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.PaddingBottom = 5f;
            model.PaddingTop = 6f;
            model.PaddingLeft = 7f;
            model.PaddingRight = 15f;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.x, model.PaddingLeft);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.y, model.PaddingTop);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.z, model.PaddingRight);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.w, model.PaddingBottom);
        }

        [TestCase("builtin:SF-UI-Text-Regular SDF","Inter-Regular SDF")]
        [TestCase("builtin:SF-UI-Text-Heavy SDF", "Inter-Heavy SDF")]
        [TestCase("builtin:SF-UI-Text-Semibold SDF", "Inter-SemiBold SDF")]
        [TestCase("builtin:LiberationSans SDF", "LiberationSans SDF")]
        [TestCase("SansSerif", "Inter-Regular SDF")]
        [TestCase("SansSerif_Heavy", "Inter-Heavy SDF")]
        [TestCase("SansSerif_Bold", "Inter-Bold SDF")]
        [TestCase("SansSerif_SemiBold", "Inter-Regular SDF")]
        public void UpdateFontTextCorrectly(string fontSrc, string fontName)
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.Font = fontSrc;
            textShapeComponentHandler.promise.OnSuccessEvent += assetFont =>
            {
                // Assert
                Assert.AreEqual(textShapeComponentHandler.textComponent.font.name, fontName);
            };

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
        }

        [Test]
        public void UpdateOpacityCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.Opacity = 0.5f;
            model.TextColor = new Color3();

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(0.5f,textShapeComponentHandler.textComponent.color.a);
        }
        
        [Test]
        public void UpdateFontSizeCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.FontSize = 17f;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.fontSize,17f);
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void UpdateFontAutoSizeCorrectly(bool enabled)
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.FontAutoSize = enabled;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.enableAutoSizing,enabled);
        }
        
        [TestCase("left","top")]
        [TestCase("right","bottom")]
        [TestCase("right","top")]
        [TestCase("left","bottom")]
        public void UpdateFontAutoSizeCorrectly(string hAlignment, string vAlignment)
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.VTextAlign = vAlignment;
            model.HTextAlign = hAlignment;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.textComponent.alignment,textShapeComponentHandler.GetAlignment(model.VTextAlign, model.HTextAlign));
        }

        [TestCase(100f,100f)]
        [TestCase(200f,200f)]
        [TestCase(300f,300f)]
        [TestCase(0,0)]
        public void UpdateWidthAndHeighCorrectly(float height, float width)
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.TextWrapping = true;
            model.Width = width;
            model.Height = height;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(textShapeComponentHandler.rectTransform.sizeDelta,new Vector2(width,height));
        }
        
        [Test]
        public void UpdateLineCountCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.LineSpacing = 5f;
            model.LineCount = 2;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(5f, textShapeComponentHandler.textComponent.lineSpacing);
            Assert.AreEqual(2, textShapeComponentHandler.textComponent.maxVisibleLines);
        }
        
        [Test]
        public void UpdateTextWrappingCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            model.TextWrapping = false;

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(Vector2.zero, textShapeComponentHandler.rectTransform.sizeDelta);
        }

        [Test]
        public void UpdateOutlineCorrectly()
        {
            // Arrange
            Color3 color = new Color3();
            color.B = 1f;
            color.R = 0f;
            color.B = 0f;
            
            PBTextShape model = CreateModel();
            model.OutlineWidth = 0.75f;
            model.OutlineColor = color;
            var outlineColor  = new UnityEngine.Color(model.OutlineColor.R, model.OutlineColor.B, model.OutlineColor.B, 1);

            // Act
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Assert
            Assert.AreEqual( model.OutlineWidth, textShapeComponentHandler.textComponent.outlineWidth);
            Assert.AreEqual( outlineColor.b, textShapeComponentHandler.textComponent.outlineColor.b/255f);
            Assert.AreEqual( outlineColor.r, textShapeComponentHandler.textComponent.outlineColor.r/255f);
            Assert.AreEqual( outlineColor.g, textShapeComponentHandler.textComponent.outlineColor.g/255f);
        }
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBTextShape model = CreateModel();
            byte[] byteArray;
            
            // Act
            byteArray = TextShapeSerialization.Serialize(model);

            // Assert
            Assert.IsNotNull(byteArray);
        }
        
        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var color = new Color3();
            PBTextShape model = CreateModel();
            model.TextColor = color;
            model.Text = "Text";
            model.Visible = true;
            model.FontSize = 5f;
            model.LineCount = 5;

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.TextColor, newModel.TextColor);
            Assert.AreEqual(model.Text, newModel.Text);
            Assert.AreEqual(model.FontSize, newModel.FontSize);
            Assert.AreEqual(model.LineCount, newModel.LineCount);
        }

        private PBTextShape SerializaAndDeserialize(PBTextShape pbBox)
        {
            var bytes = TextShapeSerialization.Serialize(pbBox);

            return TextShapeSerialization.Deserialize(bytes);
        }

        private PBTextShape CreateModel()
        {
            PBTextShape model = new PBTextShape();
            model.Font = "SansSerif";
            return model;
        }
    }
}
