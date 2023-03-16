using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.MapCameraController
{
    public class MapCameraControllerShould
    {
        private MapRendererV2.MapCameraController.MapCameraController mapCamera;
        private MapCameraObject mapCameraObject;
        private ICoordsUtils coordsUtils;
        private IMapCullingController culling;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject();
            mapCameraObject = go.AddComponent<MapCameraObject>();
            mapCameraObject.mapCamera = go.AddComponent<Camera>();
            coordsUtils = Substitute.For<ICoordsUtils>();
            culling = Substitute.For<IMapCullingController>();

            mapCamera = new MapRendererV2.MapCameraController.MapCameraController(
                Substitute.For<IMapInteractivityControllerInternal>(), mapCameraObject, coordsUtils, culling);
        }

        [Test]
        public void BeConstructed()
        {
            Assert.AreEqual(mapCamera.MapCameraObject, mapCameraObject);
            Assert.AreEqual(mapCamera.CoordUtils, coordsUtils);
            Assert.IsTrue(mapCameraObject.mapCamera.orthographic);
            Assert.IsNull(mapCamera.RenderTexture);
        }

        [Test]
        public void BeInitialized()
        {
            ((IMapCameraControllerInternal)mapCamera).Initialize(new Vector2Int(30, 30), new Vector2Int(10, 20), MapLayer.Atlas);

            Assert.NotNull(mapCamera.RenderTexture);
            Assert.AreEqual(30, mapCamera.RenderTexture.width);
            Assert.AreEqual(30, mapCamera.RenderTexture.height);
            Assert.AreEqual(mapCamera.EnabledLayers, MapLayer.Atlas);
            Assert.AreEqual(mapCamera.ZoomValues, new Vector2Int(10, 20));
        }

        [Test]
        public void ThrowIfAccessingRenderTextureWhenNotInitialized()
        {
            Assert.Throws<Exception>(() => mapCamera.GetRenderTexture());
        }

        [Test]
        public void ReturnRenderTexture()
        {
            ((IMapCameraControllerInternal)mapCamera).Initialize(new Vector2Int(20, 20), Vector2Int.one, MapLayer.Atlas);

            var renderTexture = mapCamera.GetRenderTexture();

            Assert.AreEqual(mapCamera.RenderTexture, renderTexture);
        }

        [TestCase(-5, 100, 200, 100)]
        [TestCase(0, 100, 200, 100)]
        [TestCase(0.25f, 100, 200, 125)]
        [TestCase(0.5f, 100, 200, 150)]
        [TestCase(0.75f, 100, 200, 175)]
        [TestCase(1f, 100, 200, 200)]
        [TestCase(2f, 100, 200, 200)]
        public void SetZoom(float zoom, int minZoom, int maxZoom, float expected)
        {
            ((IMapCameraControllerInternal)mapCamera).Initialize(new Vector2Int(20, 20), new Vector2Int(100, 200), MapLayer.Atlas);

            mapCamera.SetZoom(zoom);

            Assert.AreEqual(expected, mapCameraObject.mapCamera.orthographicSize);
            culling.Received().SetCameraDirty(mapCamera);
        }

        [Test]
        public void SetPosition()
        {
            coordsUtils.CoordsToPositionUnclamped(Arg.Any<Vector2>()).Returns((x) => (Vector3)x.ArgAt<Vector2>(0) * 10); //Multiply input by 10

            mapCamera.SetPosition(Vector2.one);

            Assert.AreEqual(new Vector3(10, mapCamera.CAMERA_HEIGHT_EXPOSED, 10), mapCameraObject.transform.localPosition);
            culling.Received().SetCameraDirty(mapCamera);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetActive(bool value)
        {
            mapCamera.SetActive(value);

            Assert.AreEqual(value, mapCameraObject.isActiveAndEnabled);
        }

        [TearDown]
        public void TearDown()
        {
            mapCamera.Dispose();
        }
    }
}
