using DCLServices.MapRendererV2.Culling;
using NSubstitute;
using NUnit.Framework;

namespace DCLServices.MapRendererV2.Tests.Culling
{
    [Category("EditModeCI")]
    public class TrackedStateShould
    {
        private MapCullingController.TrackedState<IMapPositionProvider> state;
        private IMapPositionProvider positionProvider;
        private IMapCullingListener<IMapPositionProvider> listener;

        [SetUp]
        public void SetUp()
        {
            positionProvider = Substitute.For<IMapPositionProvider>();
            listener = Substitute.For<IMapCullingListener<IMapPositionProvider>>();
            state = new MapCullingController.TrackedState<IMapPositionProvider>(positionProvider, listener);
        }

        [TestCase(0b00000001)]
        [TestCase(0b00000010)]
        [TestCase(0b00000011)]
        public void SetCameraFlag(int value)
        {
            state.SetCameraFlag(value);

            Assert.AreEqual(value, state.DirtyCamerasFlag);
        }

        [TestCase(0b00000001, 0, false, 0b00000000)]
        [TestCase(0b00000000, 0, true, 0b00000001)]
        [TestCase(0b00001111, 2, false, 0b00001011)]
        [TestCase(0b00001011, 2, true, 0b00001111)]
        [TestCase(0b00001111, 6, true, 0b01001111)]
        [TestCase(0b01001111, 6, false, 0b00001111)]
        public void SetCameraFlagIndex(int initial, int index, bool value, int expected)
        {
            state.DirtyCamerasFlag = initial;

            state.SetCameraFlag(index, value);

            Assert.AreEqual(expected, state.DirtyCamerasFlag);
        }

        [TestCase(0b00000000, 0, false)]
        [TestCase(0b00000001, 0, true)]
        [TestCase(0b11110111, 3, false)]
        [TestCase(0b00001000, 3, true)]
        public void ReturnIsDirty(int initial, int index, bool expected)
        {
            state.DirtyCamerasFlag = initial;

            Assert.AreEqual(expected, state.IsCameraDirty(index));
        }

        [TestCase(0b00000001)]
        [TestCase(0b00000010)]
        [TestCase(0b00000011)]
        public void SetVisibleFlag(int value)
        {
            state.SetVisibleFlag(value);

            Assert.AreEqual(value, state.VisibleFlag);
        }

        [TestCase(0b00000001, 0, false, 0b00000000)]
        [TestCase(0b00000000, 0, true, 0b00000001)]
        [TestCase(0b00001111, 2, false, 0b00001011)]
        [TestCase(0b00001011, 2, true, 0b00001111)]
        [TestCase(0b00001111, 6, true, 0b01001111)]
        [TestCase(0b01001111, 6, false, 0b00001111)]
        public void SetVisibleFlagIndex(int initial, int index, bool value, int expected)
        {
            state.VisibleFlag = initial;

            state.SetVisibleFlag(index, value);

            Assert.AreEqual(expected, state.VisibleFlag);
        }

        [TestCase(0b00000000, true)]
        [TestCase(0b00000001, false)]
        [TestCase(0b00000101, false)]
        [TestCase(0b00000100, false)]
        [TestCase(0b00110100, false)]
        public void CallListener(int visibleFlag, bool isCulled)
        {
            state.VisibleFlag = visibleFlag;

            state.CallListener();

            if (isCulled)
            {
                listener.Received().OnMapObjectCulled(positionProvider);
                listener.DidNotReceiveWithAnyArgs().OnMapObjectBecameVisible(Arg.Any<IMapPositionProvider>());
            }
            else
            {
                listener.DidNotReceiveWithAnyArgs().OnMapObjectCulled(positionProvider);
                listener.Received().OnMapObjectBecameVisible(Arg.Any<IMapPositionProvider>());
            }
        }
    }
}
