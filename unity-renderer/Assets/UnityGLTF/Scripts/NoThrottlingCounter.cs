namespace UnityGLTF.Scripts
{
    public class NoThrottlingCounter : IThrottlingCounter
    {

        public bool EvaluateTimeBudget(double elapsedTime)
        {
            return false;
        }
    }
}