using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class ECSTweenSystemShould
    {
        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {
            // ...
            // Environment.Dispose();
        }

        [UnityTest]
        public IEnumerator AttachAndUpdateTweenStateComponent()
        {
            return null;
        }

        [UnityTest]
        public IEnumerator UpdateTransformComponent()
        {
            return null;
        }

        [UnityTest]
        public IEnumerator UpdateInternalSBCComponent()
        {
            return null;
        }
    }
}
