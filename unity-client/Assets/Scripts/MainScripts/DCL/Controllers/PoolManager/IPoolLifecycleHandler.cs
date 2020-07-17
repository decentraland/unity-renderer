namespace DCL.Components
{
    public interface IPoolLifecycleHandler
    {
        void OnPoolRelease();
        void OnPoolGet();
    }
}