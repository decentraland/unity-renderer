using DCL;
using DCL.FPSDisplay;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [Explicit]
    public class FPSDisplayTests : IntegrationTestSuite_Legacy
    {
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