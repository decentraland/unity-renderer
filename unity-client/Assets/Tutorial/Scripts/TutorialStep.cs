using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialStep : MonoBehaviour
    {
        public enum Id
        {
            NONE = 0,
            INITIAL_SCENE = 1,
            GENESIS_PLAZA = 2,
            CHAT_AND_AVATAR_EXPRESSIONS = 3,
            FINISHED = 99,
        }

        public Id stepId;

        public virtual void OnStepStart()
        {
        }
        public virtual void OnStepFinished()
        {
        }

        public virtual IEnumerator OnStepExecute()
        {
            yield break;
        }

        WaitForSeconds waitForIdleTime = new WaitForSeconds(TutorialController.DEFAULT_STAGE_IDLE_TIME);

        public virtual IEnumerator WaitIdleTime()
        {
            yield return waitForIdleTime;
        }

    }
}
