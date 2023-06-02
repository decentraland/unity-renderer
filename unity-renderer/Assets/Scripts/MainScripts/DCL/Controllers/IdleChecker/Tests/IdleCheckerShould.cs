using System.Collections;
using System.Linq;
using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class IdleCheckerShould
    {
        private IdleChecker idleChecker;

        [UnityTest]
        [Category("Flaky")]
        public IEnumerator BeIdleInOneSecond()
        {
            idleChecker = new IdleChecker();
            idleChecker.Initialize();
            idleChecker.SetMaxTime(1); // MaxTime in one second for the test
            idleChecker.Update();

            // It should start as not idle
            Assert.IsFalse(idleChecker.isIdle());

            yield return new DCL.WaitUntil(() =>
            {
                idleChecker.Update(); // We need to update it to update the check the status...
                return idleChecker.isIdle();
            }, 3.0f);

            // It should be on idle, maybe it can fail for timeout...
            Assert.IsTrue(idleChecker.isIdle());
        }
    }
}
