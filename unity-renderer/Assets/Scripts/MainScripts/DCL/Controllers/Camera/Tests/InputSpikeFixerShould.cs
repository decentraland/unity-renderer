using DCL.Camera;
using NUnit.Framework;
using UnityEngine;

namespace CameraController_Test
{
    public class InputSpikeFixerShould
    {
        private CursorLockMode previousLockState;

        [SetUp]
        public void Setup()
        {
            previousLockState = Cursor.lockState;
        }

        [TearDown]
        public void TearDown()
        {
            Cursor.lockState = previousLockState;
        }
        
        [TestCase(0, 0)]
        [TestCase(10, 10)]
        [TestCase(-10, -10)]
        [TestCase(0.5f, 0.5f)]
        public void NotChangeValueWhenSameLockStateIsSetTwice(float inputValue, float expectedInputValue)
        {
            var inputSpikeFixer = new InputSpikeFixer(() => CursorLockMode.Locked);
            var returnValue = inputSpikeFixer.GetValue(inputValue);
            
            Assert.AreEqual(expectedInputValue, returnValue);
        }
        
        [Test]
        public void ReturnZeroWhenLockStateIsChangedAndValueIsHigherThanTolerance()
        {
            var lockState = CursorLockMode.Locked;
            var inputSpikeFixer = new InputSpikeFixer(() => lockState);
            lockState = CursorLockMode.Confined;
            
            var returnValue = inputSpikeFixer.GetValue(10);
            
            Assert.AreEqual(0, returnValue);
        }
        
        [Test]
        public void ReturnZeroJustOnceWhenLockStateIsChangedAndValueIsLowerThanTolerance()
        {
            var lockState = CursorLockMode.Locked;
            var inputSpikeFixer = new InputSpikeFixer(() => lockState);
            lockState = CursorLockMode.Confined;
            
            var returnValue = inputSpikeFixer.GetValue(0.4f);
            Assert.AreEqual(0, returnValue);
            
            var returnValue2 = inputSpikeFixer.GetValue(0.4f);
            Assert.AreEqual(0.4f, returnValue2);
        }
    }
}
