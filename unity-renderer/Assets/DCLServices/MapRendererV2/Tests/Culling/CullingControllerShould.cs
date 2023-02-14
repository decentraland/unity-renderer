using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;

namespace DCLServices.MapRendererV2.Tests.Culling
{
    public class CullingControllerShould
    {
        private MapCullingController culling;
        private IMapCullingVisibilityChecker visibilityChecker;

        [SetUp]
        public void SetUp()
        {
            visibilityChecker = Substitute.For<IMapCullingVisibilityChecker>();
            culling = new MapCullingController(visibilityChecker);
        }

        [TestCase(0b000000000, 0, 0b000000001)]
        [TestCase(0b000000001, 0, 0b000000001)]
        [TestCase(0b11110111, 3, 0b11111111)]
        [TestCase(0b11111111, 3, 0b11111111)]
        public void SetCameraDirtyInternal(int initial, int index, int expected)
        {
            culling.DirtyCamerasFlag = initial;

            culling.SetCameraDirtyInternal_Test(index);

            Assert.AreEqual(expected, culling.DirtyCamerasFlag);
        }

        [TestCase(0b000000000, 0, false)]
        [TestCase(0b000000001, 0, true)]
        [TestCase(0b11110111, 3, false)]
        [TestCase(0b11111111, 3, true)]
        public void ReturnIsCameraDirty(int initial, int index, bool expected)
        {
            culling.DirtyCamerasFlag = initial;

            var value = culling.IsCameraDirty_Test(index);

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void ThrowIfAddingCameraOutOfRange()
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(culling.DirtyCamerasFlag);

            for (int i = 0; i < size; i++)
                culling.OnCameraAdded_Test(Substitute.For<IMapCameraControllerInternal>());

            Assert.Throws<Exception>(() => culling.OnCameraAdded_Test(Substitute.For<IMapCameraControllerInternal>()));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void SetDirtyCameraFlagWhenAddingCamera(int initialCameraCount)
        {
            for (int i = 0; i < initialCameraCount; i++)
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
                culling.SetCameraDirtyInternal_Test(i);
            }

            var camera = Substitute.For<IMapCameraControllerInternal>();
            culling.OnCameraAdded_Test(camera);

            Assert.AreEqual(camera, culling.Cameras.Last());
            Assert.IsTrue(culling.IsCameraDirty_Test(initialCameraCount));
        }

        [TestCase(1, 0, 0b00000001)]
        [TestCase(4, 0, 0b00001111)]
        [TestCase(4, 2, 0b00001100)]
        public void SetDirtyOnwardsCameraFlagWhenRemovingCamera(int initialCameraCount, int indexToRemove, int expectedDirtyCameras)
        {
            for (int i = 0; i < initialCameraCount; i++) { culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>()); }

            IMapCameraControllerInternal camera = culling.Cameras.ElementAt(indexToRemove);

            culling.OnCameraRemoved_Test(camera);

            Assert.AreEqual(initialCameraCount - 1, culling.Cameras.Count);
            Assert.AreEqual(expectedDirtyCameras, culling.DirtyCamerasFlag);
        }

        [Test]
        public void ThrowIfSettingDirtyANotTrackedCamera()
        {
            Assert.Throws<Exception>(() => ((IMapCullingController)culling).SetCameraDirty(Substitute.For<IMapCameraControllerInternal>()));
        }

        [TestCase(4, 0, 0b000000001)]
        [TestCase(4, 1, 0b000000010)]
        [TestCase(4, 3, 0b000001000)]
        public void SetCameraDirty(int cameraCount, int index, int expected)
        {
            for (int i = 0; i < cameraCount; i++) { culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>()); }

            ((IMapCullingController)culling).SetCameraDirty(culling.Cameras.ElementAt(index));

            Assert.AreEqual(expected, culling.DirtyCamerasFlag);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void TrackObjectPosition(int initialTrackedCount)
        {
            for (int i = 0; i < initialTrackedCount; i++) { ((IMapCullingController)culling).StartTracking(Substitute.For<IMapPositionProvider>(), Substitute.For<IMapCullingListener<IMapPositionProvider>>()); }

            var obj = Substitute.For<IMapPositionProvider>();
            var listener = Substitute.For<IMapCullingListener<IMapPositionProvider>>();
            ((IMapCullingController)culling).StartTracking(obj, listener);

            Assert.AreEqual(initialTrackedCount + 1, culling.TrackedObjs.Count);
            Assert.IsTrue(culling.TrackedObjs.ContainsKey(obj));
            Assert.AreEqual(listener, ((MapCullingController.TrackedState<IMapPositionProvider>)culling.TrackedObjs[obj]).Listener);
        }

        [Test]
        public void TrackObjectPositionIgnoringDuplicates()
        {
            var obj = Substitute.For<IMapPositionProvider>();
            var listener = Substitute.For<IMapCullingListener<IMapPositionProvider>>();
            var state = new MapCullingController.TrackedState<IMapPositionProvider>(obj, listener);
            culling.TrackedObjs[obj] = state;

            ((IMapCullingController)culling).StartTracking(obj, listener);

            Assert.AreEqual(1, culling.TrackedObjs.Count);
            Assert.IsTrue(culling.TrackedObjs.ContainsKey(obj));
            Assert.AreEqual(listener, ((MapCullingController.TrackedState<IMapPositionProvider>)culling.TrackedObjs[obj]).Listener);
        }

        [Test]
        public void SetDirtyNewTrackedObjects()
        {
            var obj = Substitute.For<IMapPositionProvider>();
            var listener = Substitute.For<IMapCullingListener<IMapPositionProvider>>();

            ((IMapCullingController)culling).StartTracking(obj, listener);
            var state = culling.TrackedObjs[obj];

            Assert.IsTrue(culling.DirtyObjects.Contains(state));
        }

        [Test]
        public void ResolveDirtyCameras_NoCameraDirty()
        {
            // Arrange
            culling.DirtyCamerasFlag = 0b00000000;

            for (int i = 0; i < 3; i++) // Simulate 3 cameras
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
            }

            for (int i = 0; i < 3; i++) //Simulate 3 tracked objects
            {
                var obj = Substitute.For<IMapPositionProvider>();
                culling.TrackedObjs.Add(obj, new MapCullingController.TrackedState<IMapPositionProvider>(obj, Substitute.For<IMapCullingListener<IMapPositionProvider>>()));
            }

            culling.ResolveDirtyCameras_Test();

            visibilityChecker.DidNotReceiveWithAnyArgs().IsVisible(Arg.Any<IMapPositionProvider>(), Arg.Any<IMapCameraControllerInternal>());
        }

        [Test]
        public void ResolveDirtyCameras_SingleCameraDirty()
        {
            // Arrange
            culling.DirtyCamerasFlag = 0b00000010; // Camera 1 is dirty

            for (int i = 0; i < 3; i++) // Simulate 3 cameras
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
            }

            for (int i = 0; i < 3; i++) //Simulate 3 tracked objects
            {
                var obj = Substitute.For<IMapPositionProvider>();
                culling.TrackedObjs.Add(obj, new MapCullingController.TrackedState<IMapPositionProvider>(obj, Substitute.For<IMapCullingListener<IMapPositionProvider>>()));
            }

            // Act
            culling.ResolveDirtyCameras_Test();

            // Assert
            visibilityChecker.DidNotReceive().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[0]); // Camera 0 is not dirty
            visibilityChecker.Received().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[1]); // Camera 1 is dirty, visibility was checked
            visibilityChecker.DidNotReceive().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[2]); // Camera 2 is not dirty
        }

        [Test]
        public void ResolveDirtyCameras_MultiplCameraDirty()
        {
            // Arrange
            culling.DirtyCamerasFlag = 0b00000110; // Camera 1 and Camera 2 are dirty

            for (int i = 0; i < 3; i++) // Simulate 3 cameras
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
            }

            for (int i = 0; i < 3; i++) //Simulate 3 tracked objects
            {
                var obj = Substitute.For<IMapPositionProvider>();
                culling.TrackedObjs.Add(obj, new MapCullingController.TrackedState<IMapPositionProvider>(obj, Substitute.For<IMapCullingListener<IMapPositionProvider>>()));
            }

            // Act
            culling.ResolveDirtyCameras_Test();

            // Assert
            visibilityChecker.DidNotReceive().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[0]); // Camera 0 is not dirty
            visibilityChecker.Received().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[1]); // Camera 1 is dirty, visibility was checked
            visibilityChecker.Received().IsVisible(Arg.Any<IMapPositionProvider>(), culling.Cameras[2]); // Camera 2 is not dirty
        }

        [Test]
        public void ResolveDirtyCameras_NoLongerTrackerCameraIsDirty()
        {
            // Arrange
            culling.DirtyCamerasFlag = 0b00001000; // Camera 3 is dirty

            for (int i = 0; i < 3; i++) // Simulate 3 cameras
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
            }

            for (int i = 0; i < 3; i++) //Simulate 3 tracked objects
            {
                var obj = Substitute.For<IMapPositionProvider>();
                culling.TrackedObjs.Add(obj, new MapCullingController.TrackedState<IMapPositionProvider>(obj, Substitute.For<IMapCullingListener<IMapPositionProvider>>()));
            }

            // Act
            culling.ResolveDirtyCameras_Test();

            // Assert
            visibilityChecker.DidNotReceiveWithAnyArgs().IsVisible(Arg.Any<IMapPositionProvider>(), Arg.Any<IMapCameraControllerInternal>());
        }

        [Test]
        public void ResolveDirtyObjects()
        {
            // Arrange
            for (int i = 0; i < 3; i++) // Simulate 3 cameras
            {
                culling.Cameras.Add(Substitute.For<IMapCameraControllerInternal>());
            }

            // Simulate 3 tracked objects
            var obj0 = AddTrackedObjectAndSetDirtyCamerasFlag(0b00000011); // Dirty for Camera 0 and Camera 1
            var obj1 = AddTrackedObjectAndSetDirtyCamerasFlag(0b00000110); // Dirty for Camera 1 and Camera 2
            var obj2 = AddTrackedObjectAndSetDirtyCamerasFlag(0b00000101); // Dirty for Camera 0 and Camera 2

            culling.ResolveDirtyObjects_Test(2); //obj2 should not be checked

            // Object 0
            visibilityChecker.Received().IsVisible(obj0, culling.Cameras[0]);
            visibilityChecker.Received().IsVisible(obj0, culling.Cameras[1]);
            visibilityChecker.DidNotReceive().IsVisible(obj0, culling.Cameras[2]);

            //Object 1
            visibilityChecker.DidNotReceive().IsVisible(obj1, culling.Cameras[0]);
            visibilityChecker.Received().IsVisible(obj1, culling.Cameras[1]);
            visibilityChecker.Received().IsVisible(obj1, culling.Cameras[2]);

            //Object2
            visibilityChecker.DidNotReceive().IsVisible(obj2, culling.Cameras[0]);
            visibilityChecker.DidNotReceive().IsVisible(obj2, culling.Cameras[1]);
            visibilityChecker.DidNotReceive().IsVisible(obj2, culling.Cameras[2]);
        }

        private IMapPositionProvider AddTrackedObjectAndSetDirtyCamerasFlag(int dirtyCamerasFlag)
        {
            var obj = Substitute.For<IMapPositionProvider>();
            var state = new MapCullingController.TrackedState<IMapPositionProvider>(obj, Substitute.For<IMapCullingListener<IMapPositionProvider>>());
            culling.TrackedObjs.Add(obj, state);
            state.DirtyCamerasFlag = dirtyCamerasFlag;
            culling.DirtyObjects.AddLast(state);
            return obj;
        }
    }
}
