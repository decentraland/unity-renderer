using DCLServices.MapRendererV2.MapCameraController;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.MapCameraController
{
    public class RentMapCameraControllerShould
    {
        private RentMapCameraController rentMapCameraController;
        private Func<IMapCameraController> cameraControllerBuilder;

        [SetUp]
        public void SetUp()
        {
            rentMapCameraController = new RentMapCameraController(() => cameraControllerBuilder?.Invoke()); //Use lambda so we can override the return value
        }

        [Test]
        public void StartWithAnEmptyPool()
        {
            Assert.AreEqual(0, rentMapCameraController.Pool.CountActive);
            Assert.AreEqual(0, rentMapCameraController.Pool.CountInactive);
        }

        [Test]
        public void GetMapControllerWithPropertiesSet()
        {
            var mapToReturn = Substitute.For<IMapCameraController>();
            cameraControllerBuilder = () => mapToReturn;

            var mapReturned = rentMapCameraController.Get(77, Vector2Int.left);

            Assert.AreEqual(mapToReturn, mapReturned);
            mapToReturn.Received().SetZoom(77);
            mapToReturn.Received().SetPosition(Vector2Int.left);
            Assert.AreEqual(1, rentMapCameraController.Pool.CountActive);
            Assert.AreEqual(0, rentMapCameraController.Pool.CountInactive);
        }

        [Test]
        public void ReturnObjectsToThePoolWhenReleasing()
        {
            cameraControllerBuilder = () => Substitute.For<IMapCameraController>();
            var mapController = rentMapCameraController.Pool.Get();

            rentMapCameraController.Release(mapController);

            Assert.AreEqual(0, rentMapCameraController.Pool.CountActive);
            Assert.AreEqual(1, rentMapCameraController.Pool.CountInactive);
        }

        [Test]
        public void IgnoreNullMapControllerForRelease()
        {
            rentMapCameraController.Release(null);

            Assert.AreEqual(0, rentMapCameraController.Pool.CountActive);
            Assert.AreEqual(0, rentMapCameraController.Pool.CountInactive);
        }
    }
}
