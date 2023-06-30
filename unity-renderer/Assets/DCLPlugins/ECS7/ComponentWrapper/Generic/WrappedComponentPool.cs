using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.ComponentWrapper.Generic
{
    public record WrappedComponentPool<T> where T: class, IWrappedComponent
    {
        private readonly Func<T> objectFactory;
        private readonly List<T> list;

        public WrappedComponentPool(int capacity, Func<T> objectFactory)
        {
            list = new List<T>(capacity);
            this.objectFactory = objectFactory;
        }

        public PooledWrappedComponent<T> Get()
        {
            if (list.Count == 0)
            {
                return new PooledWrappedComponent<T>(objectFactory(), this);
            }

            int index = list.Count - 1;
            T wrappedComponent = list[index];
            wrappedComponent.ClearFields();
            PooledWrappedComponent<T> result = new PooledWrappedComponent<T>(wrappedComponent, this);
            list.RemoveAt(index);
            return result;
        }

        public void Release(PooledWrappedComponent<T> element)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == element.WrappedComponent)
                {
#if UNITY_EDITOR
                    Debug.LogError($"element {typeof(T)} already released to the pool");
#endif
                    return;
                }
            }

            list.Add(element.WrappedComponent);
        }
    }
}
