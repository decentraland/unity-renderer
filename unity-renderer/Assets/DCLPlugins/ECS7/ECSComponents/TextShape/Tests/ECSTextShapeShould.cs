﻿using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Font = DCL.ECSComponents.Font;
using Vector2 = UnityEngine.Vector2;

namespace Tests
{
    public class ECSTextShapeShould
    {
        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities testUtils;

        private ECSTextShapeComponentHandler textShapeComponentHandler;
        private IInternalECSComponent<InternalRenderers> renderersInternalComponent;

        [SetUp]
        public void SetUp()
        {
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(111);

            renderersInternalComponent = Substitute.For<IInternalECSComponent<InternalRenderers>>();
            ECSComponentData<InternalRenderers> internalCompData = null;
            renderersInternalComponent.GetFor(scene, entity).Returns((info) => internalCompData);
            renderersInternalComponent.WhenForAnyArgs(
                                          x => x.PutFor(scene, entity, Arg.Any<InternalRenderers>()))
                                      .Do(info =>
                                      {
                                          internalCompData ??= new ECSComponentData<InternalRenderers>
                                          {
                                              scene = info.ArgAt<IParcelScene>(0),
                                              entity = info.ArgAt<IDCLEntity>(1)
                                          };
                                          internalCompData.model = info.ArgAt<InternalRenderers>(2);
                                      });

            textShapeComponentHandler = new ECSTextShapeComponentHandler(AssetPromiseKeeper_Font.i, renderersInternalComponent);
            textShapeComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            textShapeComponentHandler.OnComponentRemoved(scene, entity);
            testUtils.Dispose();
            AssetPromiseKeeper_Font.i.Cleanup();
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            const string TEXT = "TestCorrectly";
            const string TEXT2 = "Temptation";

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, new PBTextShape()
            {
                Text = TEXT
            });

            Assert.AreEqual(TEXT, textShapeComponentHandler.textComponent.text);

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, new PBTextShape()
            {
                Text = TEXT2
            });

            Assert.AreEqual(TEXT2, textShapeComponentHandler.textComponent.text);
        }

        [UnityTest]
        public IEnumerator DisposeComponentCorrectly()
        {
            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, new PBTextShape());
            var gameObject = textShapeComponentHandler.textComponent.gameObject;

            textShapeComponentHandler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(gameObject);
        }

        [Test]
        public void UpdateTextColorCorrectly()
        {
            PBTextShape model = new PBTextShape();
            Color4 color = new Color4();
            color.B = 1f;
            color.R = 0f;
            color.B = 0f;

            model.TextColor = color;

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(textShapeComponentHandler.textComponent.color.r, color.R);
            Assert.AreEqual(textShapeComponentHandler.textComponent.color.b, color.B);
            Assert.AreEqual(textShapeComponentHandler.textComponent.color.g, color.B);
        }

        [Test]
        public void UpdatePaddingTextCorrectly()
        {
            PBTextShape model = new PBTextShape
            {
                PaddingBottom = 5f,
                PaddingTop = 6f,
                PaddingLeft = 7f,
                PaddingRight = 15f
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.x, model.PaddingLeft);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.y, model.PaddingTop);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.z, model.PaddingRight);
            Assert.AreEqual(textShapeComponentHandler.textComponent.margin.w, model.PaddingBottom);
        }

        [UnityTest]
        public IEnumerator UpdateFontTextCorrectly()
        {
            var fonts = new Dictionary<Font, string>()
            {
                { Font.FSansSerif, "Inter-Regular SDF" },
                { Font.FLiberationSans, "LiberationSans SDF" },
            };

            foreach (var fontsMapPair in fonts)
            {
                textShapeComponentHandler.OnComponentModelUpdated(scene, entity, new PBTextShape
                {
                    Font = fontsMapPair.Key
                });

                yield return textShapeComponentHandler.fontPromise;

                Assert.AreEqual(fontsMapPair.Value, textShapeComponentHandler.textComponent.font.name, $"for key {fontsMapPair.Key}");
            }
        }

        [Test]
        public void UpdateOpacityCorrectly()
        {
            PBTextShape model = new PBTextShape
            {
                TextColor = new Color4() { R = 1.0f, G = 1.0f, B = 1.0f, A = 0.5f}
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(0.5f, textShapeComponentHandler.textComponent.color.a);
        }

        [Test]
        public void UpdateFontSizeCorrectly()
        {
            PBTextShape model = new PBTextShape
            {
                FontSize = 17f
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(17f, textShapeComponentHandler.textComponent.fontSize);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UpdateFontAutoSizeCorrectly(bool enabled)
        {
            PBTextShape model = new PBTextShape
            {
                FontAutoSize = enabled
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(enabled, textShapeComponentHandler.textComponent.enableAutoSizing);
        }

        [TestCase(TextAlignMode.TamTopLeft, ExpectedResult = TextAlignmentOptions.TopLeft)]
        [TestCase(TextAlignMode.TamBottomRight, ExpectedResult = TextAlignmentOptions.BottomRight)]
        [TestCase(TextAlignMode.TamTopRight, ExpectedResult = TextAlignmentOptions.TopRight)]
        [TestCase(TextAlignMode.TamBottomLeft, ExpectedResult = TextAlignmentOptions.BottomLeft)]
        public TextAlignmentOptions UpdateFontAutoSizeCorrectly(TextAlignMode alignment)
        {
            PBTextShape model = new PBTextShape
            {
                TextAlign = alignment
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            return textShapeComponentHandler.textComponent.alignment;
        }

        [TestCase(100f, 100f, ExpectedResult = new float[] { 100f, 100f })]
        [TestCase(200f, 200f, ExpectedResult = new float[] { 200, 200f })]
        [TestCase(300f, 300f, ExpectedResult = new float[] { 300f, 300f })]
        [TestCase(0, 0, ExpectedResult = new float[] { 0, 0 })]
        public float[] UpdateWidthAndHeighCorrectly(float height, float width)
        {
            PBTextShape model = new PBTextShape
            {
                TextWrapping = true,
                Width = width,
                Height = height
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            var sizeDelta = ((RectTransform)textShapeComponentHandler.textComponent.transform).sizeDelta;
            return new float[] { sizeDelta.x, sizeDelta.y };
        }

        [Test]
        public void UpdateLineCountCorrectly()
        {
            PBTextShape model = new PBTextShape
            {
                LineSpacing = 5f,
                LineCount = 2
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(5f, textShapeComponentHandler.textComponent.lineSpacing);
            Assert.AreEqual(2, textShapeComponentHandler.textComponent.maxVisibleLines);
        }

        [Test]
        public void UpdateTextWrappingCorrectly()
        {
            PBTextShape model = new PBTextShape
            {
                TextWrapping = false
            };

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(Vector2.zero, ((RectTransform)textShapeComponentHandler.textComponent.transform).sizeDelta);
        }

        [UnityTest]
        public IEnumerator UpdateOutlineCorrectly()
        {
            Color3 color = new Color3();
            color.B = 1f;
            color.R = 0f;
            color.B = 0f;

            PBTextShape model = new PBTextShape
            {
                OutlineWidth = 0.75f,
                OutlineColor = color
            };

            var outlineColor = new Color(model.OutlineColor.R, model.OutlineColor.B, model.OutlineColor.B, 1);

            textShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            yield return textShapeComponentHandler.fontPromise;

            // Assert
            Assert.AreEqual(model.OutlineWidth, textShapeComponentHandler.textComponent.outlineWidth);
            Assert.AreEqual(outlineColor.b, textShapeComponentHandler.textComponent.outlineColor.b / 255f);
            Assert.AreEqual(outlineColor.r, textShapeComponentHandler.textComponent.outlineColor.r / 255f);
            Assert.AreEqual(outlineColor.g, textShapeComponentHandler.textComponent.outlineColor.g / 255f);
        }

        [Test]
        public void AddRendererOnCreated()
        {
            renderersInternalComponent.Received(1)
                                      .PutFor(scene, entity,
                                          Arg.Is<InternalRenderers>(
                                              i => i.renderers.Contains(textShapeComponentHandler.textComponent.GetComponent<MeshRenderer>())));
        }

        [Test]
        public void RemoveRendererOnRemoved()
        {
            renderersInternalComponent.ClearReceivedCalls();

            textShapeComponentHandler.OnComponentRemoved(scene, entity);

            renderersInternalComponent.Received(1).RemoveFor(scene, entity);
        }
    }
}