using System;
using System.Collections.Generic;

namespace DCL.ECS7.ComponentWrapper
{
    public readonly struct WrappedComponentPool<T> where T: IWrappedComponent<T>
    {
        private readonly Queue<PooledWrappedComponent<T>> queue;
        private readonly Func<T> objectFactory;

        public WrappedComponentPool(int capacity, Func<T> objectFactory)
        {
            queue = new Queue<PooledWrappedComponent<T>>(capacity);
            this.objectFactory = objectFactory;
        }

        public PooledWrappedComponent<T> GetElement()
        {
            if (queue.TryDequeue(out PooledWrappedComponent<T> result))
            {
                result.WrappedComponent.ClearFields();
                return result;
            }

            result = new PooledWrappedComponent<T>(objectFactory(), this);
            return result;
        }

        public void AddElement(PooledWrappedComponent<T> element)
        {
            queue.Enqueue(element);
        }
    }

    public readonly struct PooledWrappedComponent<T> : IDisposable where T: IWrappedComponent<T>
    {
        private readonly WrappedComponentPool<T> pool;

        public readonly T WrappedComponent;

        public static implicit operator T(PooledWrappedComponent<T> pooled) =>
            pooled.WrappedComponent;

        internal PooledWrappedComponent(T wrappedComponent, WrappedComponentPool<T> pool)
        {
            this.WrappedComponent = wrappedComponent;
            this.pool = pool;
        }

        public void Dispose()
        {
            pool.AddElement(this);
        }
    }
}
