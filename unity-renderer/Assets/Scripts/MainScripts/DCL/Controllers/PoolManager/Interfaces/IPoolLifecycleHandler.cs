namespace DCL.Components
{
    public interface IPoolableObjectContainer
    {
        IPoolableObject poolableObject { get; set; }
    }

    public interface IPoolLifecycleHandler
    {
        void OnPoolRelease();
        void OnPoolGet();
    }
}
