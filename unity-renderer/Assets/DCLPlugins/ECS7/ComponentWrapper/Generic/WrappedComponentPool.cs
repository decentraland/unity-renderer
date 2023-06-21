using System;
using System.Collections.Generic;

namespace DCL.ECS7.ComponentWrapper.Generic
{
    public record WrappedComponentPool<T> where T: class, IWrappedComponent
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
}
