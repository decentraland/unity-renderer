using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to lock/unlock the cursor.
    /// </summary>
    public class TutorialStep_LockTheCursor : TutorialStep
    {
        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => mouseCatcher.isLocked);
        }
    }
}