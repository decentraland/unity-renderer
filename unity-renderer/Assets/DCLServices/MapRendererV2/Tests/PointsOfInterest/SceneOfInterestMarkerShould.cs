using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.PointsOfInterest;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.Tests.PointsOfInterest
{
    [TestFixture]
    [Category("EditModeCI")]
    public class SceneOfInterestMarkerShould
    {
        private SceneOfInterestMarker marker;
        private IUnityObjectPool<SceneOfInterestMarkerObject> objectPool;
        private IMapCullingController mapCullingController;

        [SetUp]
        public void Setup()
        {
            objectPool = Substitute.For<IUnityObjectPool<SceneOfInterestMarkerObject>>();

            var obj = new GameObject("TEST");
            var c = obj.AddComponent<SceneOfInterestMarkerObject>();
            c.title = obj.AddComponent<TextMeshPro>();

            objectPool.Get().Returns(c);
            marker = new SceneOfInterestMarker(objectPool, mapCullingController = Substitute.For<IMapCullingController>());
        }

        [Test]
        public void LimitTitle()
        {
            var title = string.Join('a', Enumerable.Repeat(Path.GetRandomFileName(), 10));
            marker.SetData(title, Vector3.down);

            Assert.AreEqual(title.Substring(0, SceneOfInterestMarker.MAX_TITLE_LENGTH), marker.title);
        }

        [Test]
        public void StopTrackingOnDispose()
        {
            marker.Dispose();
            mapCullingController.Received(1).StopTracking(marker);
        }

        [Test]
        public void RentOnBecameVisible()
        {
            marker.OnBecameVisible();
            objectPool.Received(1).Get();
        }

        [Test]
        public void ReturnOnBecameInvisible()
        {
            marker.OnBecameVisible();
            marker.OnBecameInvisible();
            objectPool.Received(1).Release(Arg.Any<SceneOfInterestMarkerObject>());
        }
    }
}
