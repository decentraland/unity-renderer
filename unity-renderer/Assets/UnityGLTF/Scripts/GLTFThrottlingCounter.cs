using DCL;
using UnityEngine;

namespace UnityGLTF
{
    public class GLTFThrottlingCounter
    {
        private ThrottlingCounter throttlingCounter = new ThrottlingCounter();

        public GLTFThrottlingCounter ()
        {
            throttlingCounter.enabled = CommonScriptableObjects.rendererState;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        }

        public double budgetPerFrameInMilliseconds
        {
            get => throttlingCounter.budgetPerFrame * 1000;
            set => throttlingCounter.budgetPerFrame = value / 1000.0;
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