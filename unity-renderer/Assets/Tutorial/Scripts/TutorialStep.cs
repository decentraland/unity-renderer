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
        public float timeBetweenTooltips = 10f;

        WaitForSeconds waitForIdleTime = null;
        WaitUntil waitForRendererEnabled = new WaitUntil(() => RenderingController.i.renderingEnabled);

        void Awake()
        {
            waitForIdleTime = new WaitForSeconds(timeBetweenTooltips);
        }

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

        public virtual IEnumerator WaitIdleTime()
        {
            if (waitForIdleTime != null)
            {
                yield return waitForIdleTime;
            }
            yield return waitForRendererEnabled;
        }

    }
}
