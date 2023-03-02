using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSComponents.UIAbstractElements.Tests;
using DCL.UIElements.Structures;
using Decentraland.Common;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tests
{
    public class UIBackgroundHandlerShould : UIComponentsShouldBase
    {
        private const int COMPONENT_ID = 34;

        private UIBackgroundHandler handler;

        [SetUp]
        public void CreateHandler()
        {
            handler = new UIBackgroundHandler(internalUiContainer, COMPONENT_ID, AssetPromiseKeeper_Texture.i);
        }

        [Test]
        public void ConformToSchema()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBUiBackground
            {
                Color = new Color4 { R = 0.5f, G = 0.5f, B = 0.1f, A = 0.95f },
                TextureMode = BackgroundTextureMode.NineSlices,
                TextureSlices = new () { Left = 0.1f, Top = 0.2f, Bottom = 0.5f, Right = 0.5f }
            });

            handler.image.Texture = new Texture2D(64, 64);

            Assert.AreEqual(new StyleColor(new Color(0.5f, 0.5f, 0.1f, 0.95f)), handler.image.canvas.style.unityBackgroundImageTintColor);
            Assert.AreEqual(DCLImageScaleMode.NINE_SLICES, handler.image.ScaleMode);
            Assert.AreEqual(new StyleInt(6), handler.image.canvas.style.unitySliceLeft);
            Assert.AreEqual(new StyleInt(12), handler.image.canvas.style.unitySliceTop);
            Assert.AreEqual(new StyleInt(32), handler.image.canvas.style.unitySliceBottom);
            Assert.AreEqual(new StyleInt(32), handler.image.canvas.style.unitySliceRight);
        }
    }
}
