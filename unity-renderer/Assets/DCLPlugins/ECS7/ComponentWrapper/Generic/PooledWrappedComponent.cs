namespace DCL.ECS7.ComponentWrapper.Generic
{
    public record PooledWrappedComponent<T> : IPooledWrappedComponent where T: class, IWrappedComponent
    {
        private readonly WrappedComponentPool<T> pool;

        public readonly T WrappedComponent;

        public IWrappedComponent WrappedComponentBase => WrappedComponent;

        public static implicit operator T(PooledWrappedComponent<T> pooled) =>
            pooled.WrappedComponent;

        internal PooledWrappedComponent(T wrappedComponent, WrappedComponentPool<T> pool)
        {
            this.WrappedComponent = wrappedComponent;
            this.pool = pool;
        }

        public void Dispose()
        {
            pool.Release(this);
        }
    }
}
