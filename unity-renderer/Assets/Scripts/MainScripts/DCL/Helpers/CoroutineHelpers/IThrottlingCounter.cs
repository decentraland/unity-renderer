namespace UnityGLTF
{
    public interface IThrottlingCounter
    {
        bool EvaluateTimeBudget(double elapsedTime);
    }
}