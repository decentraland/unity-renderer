using DCL;

namespace UnityGLTF
{
    public class SmartThrottlingCounter : IThrottlingCounter
    {
        private readonly ThrottlingCounter throttlingCounter;

        public SmartThrottlingCounter (double budget)
        {
            throttlingCounter = new ThrottlingCounter(budget)
            {
                enabled = CommonScriptableObjects.rendererState
            };

            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        }

        public bool enabled
        {
            get => throttlingCounter.enabled;
            set => throttlingCounter.enabled = value;
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