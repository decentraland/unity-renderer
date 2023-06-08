using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Helpers
{
    public interface IAsyncEnumerableWithEvent<T> : IUniTaskAsyncEnumerable<T>
    {
        void AddListener(Action<T> callback);
        void RemoveListener(Action<T> callback);
    }

    public class AsyncEnumerableWithEvent<T> : IAsyncEnumerableWithEvent<T>, IDisposable
    {
        private readonly AsyncReactiveProperty<T> activeProperty = new (default);
        private Action<T> callback;

        public void AddListener(Action<T> callback)
        {
            this.callback += callback;
        }

        public void RemoveListener(Action<T> callback)
        {
            this.callback -= callback;
        }

        public void Write(T item)
        {
            UnityEngine.Debug.Log($"Calling event with {item}");
            callback?.Invoke(item);
            activeProperty.Value = item;
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new ()) =>
            activeProperty.GetAsyncEnumerator(cancellationToken);

        public void Dispose()
        {
            activeProperty?.Dispose();
            callback = null;
        }
    }
}
