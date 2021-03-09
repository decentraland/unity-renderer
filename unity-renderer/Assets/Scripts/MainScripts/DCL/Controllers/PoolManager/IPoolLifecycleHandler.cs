namespace DCL.Components
{
    public interface IPoolableObjectContainer
    {
        PoolableObject poolableObject { get; set; }
    }

    public interface IPoolLifecycleHandler
    {
        void OnPoolRelease();
        void OnPoolGet();
    }
}