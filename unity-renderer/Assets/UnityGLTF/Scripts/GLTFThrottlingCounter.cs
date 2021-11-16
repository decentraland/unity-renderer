using DCL;
using UnityEngine;

namespace UnityGLTF
{
    public class GLTFThrottlingCounter
    {
        public static GLTFThrottlingCounter i => new GLTFThrottlingCounter();
        private ThrottlingCounter throttlingCounter = new ThrottlingCounter();

        public GLTFThrottlingCounter ()
        {
            throttlingCounter.enabled = CommonScriptableObjects.rendererState;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        }

        public double budgetPerFrameInMilliseconds
        {
            get => throttlingCounter.evaluationTimeElapsedCap;
            set => throttlingCounter.evaluationTimeElapsedCap = value;
        }

        public bool EvaluateTimeBudget(double elapsedTime)
        {
            return throttlingCounter.EvaluateTimeBudget(elapsedTime);
        }

        private void OnRendererStateChange(bool current, bool previous)
        {
            throttlingCounter.enabled = current;
        }
    }
}