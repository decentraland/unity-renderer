namespace DCL.ECS7.ComponentWrapper.Generic
{
    public interface IWrappedComponent<T> : IWrappedComponent where T: class
    {
        T Model { get; }
    }
}
