namespace UnityGLTF
{
    public static class GLTFThrottlingHelper
    {
        private static bool renderingIsDisabled => !CommonScriptableObjects.rendererState.Get();
        private static float budgetPerFrameInMillisecondsValue = 2f;
        public static float budgetPerFrameInMilliseconds { get => renderingIsDisabled ? float.MaxValue : budgetPerFrameInMillisecondsValue; set => budgetPerFrameInMillisecondsValue = value; }

        private static float timeBudgetCounter = 0f;

        public static bool EvaluateTimeBudget(float timeToDeduct)
        {
            timeBudgetCounter -= timeToDeduct;

            if ( timeBudgetCounter < 0 )
            {
                timeBudgetCounter += budgetPerFrameInMilliseconds / 1000f;
                return true;
            }

            return false;
        }
    }
}