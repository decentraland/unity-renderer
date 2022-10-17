using DCL;
using DCL.FPSDisplay;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [Explicit]
    public class FPSDisplayTests
    {

        
        private FPSDisplay fpsDisplay;
        
        [SetUp]
        public void SetUp()
        {
            fpsDisplay = Resources.Load<DebugView>("DebugView").GetComponentInChildren<FPSDisplay>(true);
            DataStore.i.debugConfig.isFPSPanelVisible.Set(true);
        }
        
        [Test]
        public void BeCreatedProperly()
        {
            Assert.NotNull(fpsDisplay);
        }

        [UnityTest]
        public IEnumerator ValueUpdatedInLoop()
        {
            string currentTime = $"{DataStore.i.worldTimer.GetCurrentTime()}";

            DebugValue testDebugValue = Resources.Load<DebugValue>("DebugValueDescription");
            testDebugValue.debugValueEnum = DebugValueEnum.FPS_HiccupsLoss;
            fpsDisplay.AddValueToUpdate(testDebugValue);

            yield return new WaitForSeconds(1f);
            
            Assert.AreNotEqual(testDebugValue.textValue.text, currentTime);
            
        }
        
        [Test]
        public void LinealFPSTest()
        {
            LinealBufferFPSCounter counter = new LinealBufferFPSCounter();

            const float tenMillis = 0.01f;
            const float expectedFps = 100.0f;

            for (int i = 0; i < 100; i++)
            {
                counter.AddDeltaTime(tenMillis);
            }

            Assert.AreEqual(counter.CurrentFPSCount(), expectedFps);
        }
    }
}